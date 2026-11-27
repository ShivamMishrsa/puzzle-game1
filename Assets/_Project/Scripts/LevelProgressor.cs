using System;
using System.Collections;
using System.Collections.Generic;
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

        [Header("World layout (table plane y = 0, +z away from the player)")]
        public Vector3 trayCenter = new Vector3(0f, 0f, -3.3f);
        public Vector3 monumentAnchor = new Vector3(0f, 0f, 4.5f);
        public Vector3 cameraTarget = new Vector3(0f, 0.8f, 0.9f);
        public float cameraPitch = 62f;
        public float cameraDistance = 24f;

        private enum LoseKind { OutOfMoves, TrackJam, NoMoves, NoAmmo }

        private LevelDefinition def;
        private int levelIndex;
        private GameState state = GameState.Preparing;
        private int movesLeft, movesUsed;
        private bool adBusy;
        private LoseKind loseKind;
        private float checkTimer;
        private Transform table;
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

        private void Start()
        {
            ConfigureCamera();
            BuildTable();
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

            int wanted = PlayerPrefs.GetInt(LevelPref, 1);
            int start = 0;
            for (int i = 0; i < levels.Length; i++) if (levels[i].LevelNumber <= wanted) start = i;
            StartLevel(start);
        }

        // ------------------------------------------------------------------ camera + bench

        private void ConfigureCamera()
        {
            float aspect = Screen.height > 0 ? (float)Screen.width / Screen.height : 0.5625f;
            float tanHalfH = 5.9f / cameraDistance;                       // keep 11.8 world units visible across the screen
            float fov = 2f * Mathf.Atan(tanHalfH / aspect) * Mathf.Rad2Deg;
            cam.orthographic = false;
            cam.fieldOfView = Mathf.Clamp(fov, 32f, 70f);
            Vector3 dir = new Vector3(0f, Mathf.Sin(cameraPitch * Mathf.Deg2Rad), -Mathf.Cos(cameraPitch * Mathf.Deg2Rad));
            cam.transform.position = cameraTarget + dir * cameraDistance;
            cam.transform.LookAt(cameraTarget, Vector3.up);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.07f, 0.06f, 0.06f);
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 80f;
        }

        private void BuildTable()
        {
            table = new GameObject("WorkshopBench").transform;
            table.SetParent(transform, false);
            MeshBuilder mb = new MeshBuilder();
            mb.AddSlab(0f, 0f, 44f, 44f, 0.4f, 0.6f, 0.12f, -0.4f, 3);
            GameObject top = new GameObject("BenchTop");
            top.transform.SetParent(table, false);
            top.AddComponent<MeshFilter>().sharedMesh = mb.ToMesh("BenchTop", 0.14f);
            top.AddComponent<MeshRenderer>().sharedMaterial = GameMaterials.WorkshopWood;
        }

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

            float cs = Mathf.Min(1.25f, 7.0f / def.GridWidth, 7.0f / def.GridHeight);
            monument.Build(def, monumentAnchor);
            track.Build(def, monument, trayCenter, cs);
            grid.Build(def, trayCenter, cs);

            movesLeft = def.moveLimit;
            movesUsed = 0;
            ui.SetLevel(def.levelID, def.title, def.tier);
            ui.SetMoves(movesLeft);
            ui.SetTargets(monument.AliveCount);
            RefreshTrackUi();
            ui.SetHintAvailable(true);
            ui.SetRotateHint(def.levelID == 21 || def.levelID == 31);
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
