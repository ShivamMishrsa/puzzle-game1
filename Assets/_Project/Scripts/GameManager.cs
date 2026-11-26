using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorGateBlast
{
    /// <summary>
    /// Game flow: loads LevelData assets from Resources/Levels, lays the board / conveyor / core out in world
    /// space, counts moves, and decides win / fail.
    /// Win  = every block has left the grid AND every Target Core layer is destroyed.
    /// Fail = out of moves, or the conveyor is full and nothing can fire any more (jam), or no block can move.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class GameManager : MonoBehaviour
    {
        private const string LevelPref = "cgb_level";

        [Header("Scene references (filled by Tools > Color Gate Blast > Setup Main Scene)")]
        public GridManager grid;
        public ConveyorTrackManager track;
        public TargetCoreManager core;
        public JuiceManager juice;
        public UIManager ui;
        public MonetizationManager monetization;
        public Camera cam;

        [Header("Shader templates (kept referenced so URP shaders are never stripped)")]
        public Material litTemplate;
        public Material particleTemplate;

        [Header("Layout (world units, portrait 9:16 -> 9 x 16)")]
        public Vector2 coreCenter = new Vector2(0f, 3.3f);
        public Vector2 boardCenter = new Vector2(0f, -3.7f);

        private enum FailKind { OutOfMoves, TrackJam, NoMoves, NoAmmo }

        private LevelData[] levels = new LevelData[0];
        private int levelIndex;
        private LevelDefinition def;
        private GameState state = GameState.Loading;
        private int movesLeft;
        private int movesUsed;
        private bool extraSlotUsed;
        private bool adBusy;
        private FailKind failKind;
        private float checkTimer;

        public GameState State { get { return state; } }

        private void Awake()
        {
            Application.targetFrameRate = 60;
            Mats.Init(litTemplate, particleTemplate);
            if (cam == null) cam = Camera.main;
        }

        private void Start()
        {
            ConfigureCamera();
            if (juice != null) juice.SetCamera(cam);
            if (grid != null) grid.SetCamera(cam);

            grid.CanExit = delegate { return track.HasFreeSlot; };
            grid.BlockExited += OnBlockExited;
            grid.MoveCommitted += OnMoveCommitted;
            grid.Message += ui.ShowToast;
            track.SlotsChanged += RefreshSlotsUi;
            ui.RestartClicked += delegate { StartLevel(levelIndex); };
            ui.NextClicked += delegate { StartLevel(Mathf.Min(levelIndex + 1, levels.Length - 1)); };
            ui.ContinueAdClicked += OnContinueAd;
            ui.ExtraSlotClicked += OnExtraSlot;

            levels = Resources.LoadAll<LevelData>("Levels");
            Array.Sort(levels, delegate (LevelData a, LevelData b) { return a.LevelNumber.CompareTo(b.LevelNumber); });
            if (levels.Length == 0)
            {
                Debug.LogError("No levels found in Assets/_Project/Resources/Levels. Run Tools > Color Gate Blast > Generate Starter Levels 1-10.");
                ui.ShowToast("No levels! Tools > Color Gate Blast > Generate Starter Levels 1-10");
                return;
            }

            int wanted = PlayerPrefs.GetInt(LevelPref, 1);
            int start = 0;
            for (int i = 0; i < levels.Length; i++) if (levels[i].LevelNumber <= wanted) start = i;
            StartLevel(start);
        }

        private void ConfigureCamera()
        {
            cam.orthographic = true;
            float aspect = Screen.height > 0 ? (float)Screen.width / Screen.height : 0.5625f;
            cam.orthographicSize = Mathf.Max(8f, 4.5f / aspect);
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.transform.rotation = Quaternion.identity;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = ColorPalette.Background;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 30f;
        }

        // ------------------------------------------------------------------ level flow

        public void StartLevel(int index)
        {
            if (levels.Length == 0) return;
            StopAllCoroutines();
            levelIndex = Mathf.Clamp(index, 0, levels.Length - 1);
            def = levels[levelIndex].definition;

            state = GameState.Loading;
            adBusy = false;
            extraSlotUsed = false;
            ui.HidePanels();

            float coreCell = Mathf.Min(1.0f, 8.4f / (def.coreWidth + 2), 5.4f / (def.coreHeight + 2));
            float boardCell = Mathf.Min(1.3f, 7.4f / def.gridWidth, 5.8f / def.gridHeight);

            core.Build(def, coreCenter, coreCell);
            track.Build(def, core, coreCenter, coreCell);
            grid.Build(def, boardCenter, boardCell);
            if (juice != null) juice.ResetCombo();

            movesLeft = def.moveLimit;
            movesUsed = 0;
            ui.SetLevel(def.levelNumber, def.tier);
            ui.SetMoves(movesLeft);
            RefreshSlotsUi();
            ui.SetExtraSlotButton(true, true);

            grid.InputEnabled = true;
            state = GameState.Playing;
            checkTimer = 0f;
        }

        private void OnBlockExited(BlockSpec spec, Vector3 worldPos, Side side)
        {
            track.Enqueue(spec.color, spec.TileCount, worldPos);
            RefreshSlotsUi();
        }

        private void OnMoveCommitted()
        {
            if (state != GameState.Playing) return;
            movesUsed++;
            movesLeft = Mathf.Max(0, movesLeft - 1);
            ui.SetMoves(movesLeft);
        }

        private void RefreshSlotsUi()
        {
            ui.SetSlots(track.UsedSlots, track.MaxSlots);
        }

        private void Update()
        {
            if (state != GameState.Playing || grid.Board == null) return;

            bool boardCleared = grid.RemainingBlocks == 0;
            bool coreCleared = core.IsCleared && core.InFlight == 0;
            if (boardCleared && coreCleared && track.PendingCount == 0)
            {
                Win();
                return;
            }
            if (grid.IsDragging) return;

            checkTimer += Time.deltaTime;
            if (checkTimer < 0.25f) return;
            checkTimer = 0f;

            // Only judge failure once the conveyor has nothing left it can do by itself.
            if (core.InFlight > 0 || !track.IsSettled()) return;

            if (boardCleared)
            {
                Fail(FailKind.NoAmmo);
            }
            else if (movesLeft <= 0)
            {
                Fail(FailKind.OutOfMoves);
            }
            else if (track.IsFull)
            {
                Fail(FailKind.TrackJam);
            }
            else if (!HasAnyMove())
            {
                Fail(FailKind.NoMoves);
            }
        }

        private bool HasAnyMove()
        {
            List<Move> moves = new List<Move>();
            grid.Board.GenerateMoves(moves, track.HasFreeSlot);
            return moves.Count > 0;
        }

        // ------------------------------------------------------------------ results

        private void Win()
        {
            state = GameState.Won;
            grid.InputEnabled = false;

            int par = def.parMoves;
            int stars = movesUsed <= par ? 3 : (movesUsed <= par + (def.moveLimit - par) / 2 ? 2 : 1);

            int next = def.levelNumber + 1;
            if (next > PlayerPrefs.GetInt(LevelPref, 1))
            {
                PlayerPrefs.SetInt(LevelPref, next);
                PlayerPrefs.Save();
            }

            if (juice != null)
            {
                juice.PlayWin();
                juice.Shake(0.12f, 0.35f);
                for (int i = 0; i < 6; i++)
                    juice.Burst(new Vector3(UnityEngine.Random.Range(-3.5f, 3.5f), UnityEngine.Random.Range(-1f, 6f), -2f),
                        ColorPalette.Get(UnityEngine.Random.Range(0, Mathf.Max(2, def.colorCount))), 18, 0.2f, 5f);
            }
            StartCoroutine(ShowWinLater(stars));
        }

        private IEnumerator ShowWinLater(int stars)
        {
            yield return new WaitForSeconds(0.9f);
            ui.ShowWin(stars, movesUsed, def.parMoves, levelIndex + 1 < levels.Length);
        }

        private void Fail(FailKind kind)
        {
            state = GameState.Failed;
            failKind = kind;
            grid.InputEnabled = false;
            if (juice != null) juice.PlayFail();

            switch (kind)
            {
                case FailKind.OutOfMoves:
                    ui.ShowFail("OUT OF MOVES", "You ran out of moves before clearing the board.", "+5 MOVES  (AD)", true);
                    break;
                case FailKind.TrackJam:
                    ui.ShowFail("TRACK JAMMED", "The conveyor is full and no turret can fire.\nA free slot lets the next block out.", "+1 TRACK SLOT  (AD)", true);
                    break;
                case FailKind.NoMoves:
                    ui.ShowFail("NO MOVES LEFT", "Every block is stuck.", "", false);
                    break;
                default:
                    ui.ShowFail("OUT OF AMMO", "The board is clear but the Target Core still has shields.", "", false);
                    break;
            }
        }

        // ------------------------------------------------------------------ rewarded ads

        private void OnContinueAd()
        {
            if (state != GameState.Failed || adBusy) return;
            adBusy = true;
            monetization.ShowRewarded(RewardPlacement.ContinueAfterFail, delegate (bool earned)
            {
                adBusy = false;
                if (!earned || state != GameState.Failed) return;
                if (failKind == FailKind.OutOfMoves)
                {
                    movesLeft += 5;
                    ui.SetMoves(movesLeft);
                }
                else if (failKind == FailKind.TrackJam)
                {
                    track.AddSlot();
                    RefreshSlotsUi();
                }
                ui.HidePanels();
                state = GameState.Playing;
                grid.InputEnabled = true;
                checkTimer = 0f;
            });
        }

        private void OnExtraSlot()
        {
            if (state != GameState.Playing || extraSlotUsed || adBusy) return;
            adBusy = true;
            monetization.ShowRewarded(RewardPlacement.ExtraTrackSlot, delegate (bool earned)
            {
                adBusy = false;
                if (!earned || state != GameState.Playing) return;
                extraSlotUsed = true;
                track.AddSlot();
                RefreshSlotsUi();
                ui.SetExtraSlotButton(false, false);
                ui.ShowToast("+1 track slot!");
            });
        }
    }
}
