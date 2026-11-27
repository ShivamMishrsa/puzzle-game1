using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace ColorGateBlast
{
    /// <summary>
    /// Rule enforcer and progression.
    /// Win  = every part of the monument is dismantled AND the tray is cleared.
    /// Lose = out of moves before the monument is dismantled, OR the conveyor is at capacity with no valid move left
    ///        (or nothing left to shoot with).
    /// Also owns level loading, camera framing, the workshop bench, combo streaks and the rewarded-ad rescue actions.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class LevelProgressor : MonoBehaviour
    {
        private const string LevelPref = "cgb_level";

        [Header("Scene references (filled by Tools > Color Gate Blast > Setup Phase 1)")]
        public GridManager grid;
        public ConveyorTrackManager track;
        public MonumentManager monument;
        public UIManager ui;
        public HintSystem hint;
        public FXManager fx;
        public SoundSynth sound;
        public AdManager ads;
        public Camera cam;
        [Tooltip("Optional: levels to play. Empty = all LevelData assets in Resources/Levels.")]
        public LevelData[] levels;

        [Header("Workshop style (applied identically to every level)")]
        public float cameraFov = 38f;
        public float cameraPitch = 58f;
        private int urlLevel = -1;
        private WorkshopCameraRig rig;
        private WorkshopEnvironmentBuilder environment;
        private WorkshopLightingSetup lighting;
        private Vector3 trayCenter, monumentAnchor;

        private enum LoseKind { OutOfMoves, TrackJam, NoMoves, NoAmmo }

        private LevelDefinition def;
        private int levelIndex;
        private GameState state = GameState.Preparing;
        private int movesLeft, movesUsed;
        private bool adBusy;
        private LoseKind loseKind;
        private float checkTimer;
        private int streak;
        private float lastExitTime = -10f;
        private int sameColorStreak;
        private BlockColor lastExitColor = BlockColor.Gray;
        private int overdriveSurges;

        public GameState State { get { return state; } }
        public LevelDefinition Level { get { return def; } }
        public int MovesLeft { get { return movesLeft; } }

        private void Awake()
        {
            Application.targetFrameRate = 60;
            Physics.gravity = new Vector3(0f, -12f, 0f);
            if (cam == null) cam = Camera.main;
        }

        /// <summary>
        /// WebGL only: the page URL can override the framing and the start level, e.g.
        /// index.html?pitch=55&amp;dist=27&amp;tx=0&amp;ty=0.8&amp;tz=1.5&amp;mz=4.5&amp;level=21
        /// (handy for tuning the camera without rebuilding).
        /// </summary>
        private void ApplyUrlOverrides()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            string url = Application.absoluteURL;
            int q = url.IndexOf('?');
            if (q < 0) return;
            string[] pairs = url.Substring(q + 1).Split('&');
            for (int i = 0; i < pairs.Length; i++)
            {
                string[] kv = pairs[i].Split('=');
                if (kv.Length != 2) continue;
                float v;
                if (!float.TryParse(kv[1], NumberStyles.Float, CultureInfo.InvariantCulture, out v)) continue;
                switch (kv[0])
                {
                    case "pitch": cameraPitch = v; break;
                    case "fov": cameraFov = v; break;
                    case "level": urlLevel = (int)v; break;
                }
            }
#endif
        }

        private void Start()
        {
            ApplyUrlOverrides();
            if (cam == null) cam = Camera.main;
            rig = GetComponent<WorkshopCameraRig>(); if (rig == null) rig = gameObject.AddComponent<WorkshopCameraRig>();
            environment = GetComponent<WorkshopEnvironmentBuilder>(); if (environment == null) environment = gameObject.AddComponent<WorkshopEnvironmentBuilder>();
            lighting = GetComponent<WorkshopLightingSetup>(); if (lighting == null) lighting = gameObject.AddComponent<WorkshopLightingSetup>();
            rig.Fov = cameraFov;
            rig.Pitch = cameraPitch;
            rig.Init(cam);
            if (fx != null) fx.SetCamera(cam);
            grid.SetCamera(cam);
            monument.SetCamera(cam);

            grid.CanExit = delegate (BlockColor c) { return track.CanAccept(c); };
            grid.BlockExited += OnBlockExited;
            grid.MoveCommitted += OnMoveCommitted;
            grid.Message += ui.ShowToast;
            track.SlotsChanged += RefreshTrackUi;
            monument.PartDestroyed += delegate (int p) { ui.SetTargets(monument.AliveCount); };
            ui.RestartClicked += delegate { StartLevel(levelIndex); };
            ui.NextClicked += OnNextClicked;
            ui.HintClicked += OnHintClicked;
            ui.ExtraSlotClicked += OnExtraSlotClicked;
            ui.ReviveClicked += OnReviveClicked;
            ui.PliersClicked += OnPliersClicked;
            grid.PliersUsed += delegate { ui.SetPliersArmed(false); ui.ShowToast("Snip!"); };
            hint.Init(grid, track, monument);
            hint.Message += ui.ShowToast;

            if (levels == null || levels.Length == 0) levels = Resources.LoadAll<LevelData>("Levels");
            Array.Sort(levels, delegate (LevelData a, LevelData b) { return a.LevelNumber.CompareTo(b.LevelNumber); });
            if (levels.Length == 0)
            {
                Debug.LogError("No levels found. Run Tools > Color Gate Blast > Generate Levels 1-10.");
                ui.ShowToast("No levels! Tools > Color Gate Blast > Generate Levels 1-10");
                return;
            }

            int wanted = urlLevel > 0 ? urlLevel : PlayerPrefs.GetInt(LevelPref, 1);
            int start = 0;
            for (int i = 0; i < levels.Length; i++) if (levels[i].LevelNumber <= wanted) start = i;
            StartLevel(start);
        }

        // ------------------------------------------------------------------ camera + bench

        /// <summary>
        /// One layout for every level: a single rounded conveyor loop encloses the monument turntable (top) and the riveted tray (bottom).
        /// The camera rig frames that loop; the workshop props and lighting are rebuilt around it.
        /// </summary>
        private void LayoutWorkshop(float cs)
        {
            float trayHalfW = def.GridWidth * cs * 0.5f, trayHalfH = def.GridHeight * cs * 0.5f;
            float gap = cs * 0.85f + 0.3f;
            float monRadius = MonumentManager.MonumentScale * 1.8f + 0.15f; // turntable radius + margin
            float loopHalfX = Mathf.Max(trayHalfW + gap, monRadius + 0.9f);
            float zBottom = -(trayHalfH + gap);
            trayCenter = new Vector3(0f, 0f, 0f);
            monumentAnchor = new Vector3(0f, 0f, trayHalfH + gap + monRadius + 0.5f);
            float zTop = monumentAnchor.z + monRadius + 0.6f;
            Vector3 loopCenter = new Vector3(0f, 0f, (zTop + zBottom) * 0.5f);
            loopHalf = new Vector2(loopHalfX, (zTop - zBottom) * 0.5f);
            loopMid = loopCenter;
        }

        /// <summary>Frames the finished monument + loop and rebuilds the props / lighting around them.</summary>
        private void FrameWorkshop()
        {
            rig.Frame(loopMid, loopHalf.x, loopHalf.y, monument.TopWorldPoint());
            environment.Build(rig, loopMid, loopHalf.x, loopHalf.y, def.levelID);
            lighting.Apply(cam, rig.Distance);
        }

        private Vector2 loopHalf;
        private Vector3 loopMid;

        // ------------------------------------------------------------------ level flow

        public void StartLevel(int index)
        {
            if (levels == null || levels.Length == 0) return;
            StopAllCoroutines();
            hint.ClearHint();
            levelIndex = Mathf.Clamp(index, 0, levels.Length - 1);
            def = levels[levelIndex].definition;

            state = GameState.Preparing;
            adBusy = false;
            streak = 0;
            sameColorStreak = 0;
            lastExitColor = BlockColor.Gray;
            overdriveSurges = 0;
            ui.HidePanels();
            ui.SetPliersArmed(false);

            float cs = Mathf.Min(1.25f, 7.0f / def.GridWidth, 7.0f / def.GridHeight);
            LayoutWorkshop(cs);
            monument.Build(def, monumentAnchor);
            FrameWorkshop();
            track.Build(def, monument, loopMid, loopHalf.x, loopHalf.y);
            grid.Build(def, trayCenter, cs);

            movesLeft = def.moveLimit;
            movesUsed = 0;
            ui.SetLevel(def.levelID, def.title, def.tier);
            ui.SetMoves(movesLeft);
            ui.SetTargets(monument.AliveCount);
            RefreshTrackUi();
            ui.SetHintAvailable(true);
            ui.SetRotateHint(def.showRotateHint);
            if (!string.IsNullOrEmpty(def.hint)) ui.ShowToast(def.hint);

            grid.InputEnabled = true;
            state = GameState.Playing;
            checkTimer = 0f;
        }

        private void OnBlockExited(int blockIndex, Vector3 center, GateDirection dir, bool piercing)
        {
            BlockInfo info = grid.Board.Blocks[blockIndex];
            Int2 d = Dirs.Of(dir);
            Vector3 gateWorld = center + new Vector3(d.x, 0f, d.y) * (grid.CellSize * 3f);
            track.SpawnTurret(info.color, info.Tiles, center, gateWorld, piercing);

            // consecutive gate exits walk up the pentatonic scale C D E G A
            streak = (Time.time - lastExitTime < 3.2f) ? streak + 1 : 0;
            lastExitTime = Time.time;
            if (sound != null) sound.PlayCombo(streak);

            // Color-Chain Overdrive (levels 21+): three same-colour exits in a row
            sameColorStreak = (info.color == lastExitColor) ? sameColorStreak + 1 : 1;
            lastExitColor = info.color;
            if (def.levelID >= 21 && sameColorStreak >= 3)
            {
                sameColorStreak = 0;
                overdriveSurges++;
                track.TriggerOverdrive(3f);
                if (sound != null) sound.Play(Sfx.Overdrive);
                ui.ShowToast("OVERDRIVE SURGE!");
                if (fx != null) { fx.Shake(0.12f, 0.3f); fx.Haptic(); }
            }
            RefreshTrackUi();
        }

        private void OnMoveCommitted()
        {
            if (state != GameState.Playing) return;
            movesUsed++;
            movesLeft = Mathf.Max(0, movesLeft - 1);
            ui.SetMoves(movesLeft);
        }

        private void RefreshTrackUi()
        {
            ui.SetTrack(track.UsedSlots, track.MaxSlots, track.Unlimited);
            ui.SetExtraSlotVisible(state == GameState.Playing && track.IsFull);
        }

        // ------------------------------------------------------------------ rules

        private void Update()
        {
            if (state != GameState.Playing || grid.Board == null) return;

            bool boardCleared = grid.RemainingBlocks == 0;
            bool monumentDone = monument.AliveCount == 0;
            if (boardCleared && monumentDone)
            {
                Win();
                return;
            }
            if (grid.IsDragging) return;

            checkTimer += Time.deltaTime;
            if (checkTimer < 0.3f) return;
            checkTimer = 0f;

            // Judge defeat only once the conveyor can no longer change anything by itself.
            if (monument.InFlight > 0 || !track.IsSettled()) return;

            if (boardCleared)
            {
                Lose(LoseKind.NoAmmo);
            }
            else if (movesLeft <= 0)
            {
                Lose(LoseKind.OutOfMoves);
            }
            else if (!HasAnyMove())
            {
                Lose(track.IsFull ? LoseKind.TrackJam : LoseKind.NoMoves);
            }
        }

        private bool HasAnyMove()
        {
            bool[] allowed = new bool[ColorCount.Total];
            for (int c = 0; c < allowed.Length; c++) allowed[c] = track.CanAccept((BlockColor)c);
            List<Move> moves = new List<Move>();
            grid.Board.GenerateMoves(moves, allowed);
            return moves.Count > 0;
        }

        private void Win()
        {
            state = GameState.Won;
            grid.InputEnabled = false;
            hint.ClearHint();
            ui.SetExtraSlotVisible(false);

            int par = def.parMoves;
            int stars = movesUsed <= par ? 3 : (movesUsed <= par + (def.moveLimit - par) / 2 ? 2 : 1);

            int next = def.levelID + 1;
            if (next > PlayerPrefs.GetInt(LevelPref, 1))
            {
                PlayerPrefs.SetInt(LevelPref, next);
                PlayerPrefs.Save();
            }
            if (sound != null) sound.Play(Sfx.Win);
            if (fx != null)
            {
                fx.Shake(0.15f, 0.4f);
                for (int i = 0; i < 5; i++)
                    fx.GoldSparks(new Vector3(UnityEngine.Random.Range(-4f, 4f), UnityEngine.Random.Range(1f, 4f), UnityEngine.Random.Range(-4f, 6f)), 18);
            }
            StartCoroutine(ShowWinLater(stars));
        }

        private IEnumerator ShowWinLater(int stars)
        {
            yield return new WaitForSeconds(1.4f);
            ui.ShowWin(stars, movesUsed, def.parMoves, levelIndex + 1 < levels.Length, def.reward);
        }

        private void Lose(LoseKind kind)
        {
            state = GameState.Lost;
            loseKind = kind;
            grid.InputEnabled = false;
            hint.ClearHint();
            ui.SetExtraSlotVisible(false);
            if (sound != null) sound.Play(Sfx.Fail);

            switch (kind)
            {
                case LoseKind.OutOfMoves:
                    ui.ShowLose("OUT OF MOVES", "You ran out of moves before dismantling the " + MonumentName() + ".", "+5 MOVES / REVIVE");
                    break;
                case LoseKind.TrackJam:
                    ui.ShowLose("TRACK FULL", "The conveyor is at capacity (" + track.UsedSlots + "/" + track.MaxSlots + ") and no block can leave.", "+1 TRACK SLOT");
                    break;
                case LoseKind.NoMoves:
                    ui.ShowLose("NO MOVES LEFT", "Every remaining block is stuck.", "");
                    break;
                default:
                    ui.ShowLose("OUT OF AMMO", "The tray is clear but the " + MonumentName() + " still has parts left.", "");
                    break;
            }
        }

        private string MonumentName()
        {
            return def.monument == MonumentKind.CassettePlayer ? "Cassette Player" : "Alarm Clock";
        }

        // ------------------------------------------------------------------ buttons (rewarded ads, interstitials)

        private void OnNextClicked()
        {
            int next = Mathf.Min(levelIndex + 1, levels.Length - 1);
            int completed = def.levelID;
            bool shown = ads != null && ads.TryShowInterstitial(completed, delegate { StartLevel(next); });
            if (!shown) StartLevel(next);
        }

        private void OnHintClicked()
        {
            if (state != GameState.Playing || adBusy || ads == null) return;
            adBusy = true;
            ads.ShowRewarded(RewardPlacement.Hint, delegate (bool earned)
            {
                adBusy = false;
                if (earned && state == GameState.Playing) hint.RequestHint();
            });
        }

        /// <summary>Pliers booster: a rewarded ad arms it, then the next tapped tray piece is snipped off. Tapping again cancels.</summary>
        private void OnPliersClicked()
        {
            if (state != GameState.Playing || adBusy || ads == null) return;
            if (grid.PliersArmed)
            {
                grid.PliersArmed = false;
                ui.SetPliersArmed(false);
                return;
            }
            adBusy = true;
            ads.ShowRewarded(RewardPlacement.Pliers, delegate (bool earned)
            {
                adBusy = false;
                if (!earned || state != GameState.Playing) return;
                grid.PliersArmed = true;
                ui.SetPliersArmed(true);
                ui.ShowToast("Tap any piece to snip it off");
            });
        }

        private void OnExtraSlotClicked()
        {
            if (state != GameState.Playing || adBusy || ads == null) return;
            adBusy = true;
            ads.ShowRewarded(RewardPlacement.ExtraTrackSlot, delegate (bool earned)
            {
                adBusy = false;
                if (!earned || state != GameState.Playing) return;
                track.AddSlot(); // permanently +1 maxTrackSlots for the current level
                ui.ShowToast("+1 track slot!");
                RefreshTrackUi();
            });
        }

        private void OnReviveClicked()
        {
            if (state != GameState.Lost || adBusy || ads == null) return;
            adBusy = true;
            ads.ShowRewarded(RewardPlacement.Revive, delegate (bool earned)
            {
                adBusy = false;
                if (!earned || state != GameState.Lost) return;
                if (loseKind == LoseKind.OutOfMoves)
                {
                    movesLeft += 5;
                    ui.SetMoves(movesLeft);
                }
                else if (loseKind == LoseKind.TrackJam)
                {
                    track.AddSlot();
                }
                ui.HidePanels();
                state = GameState.Playing;
                grid.InputEnabled = true;
                checkTimer = 0f;
                RefreshTrackUi();
            });
        }
    }
}
