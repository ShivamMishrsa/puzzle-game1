using System;
using System.Collections.Generic;
using UnityEngine;

namespace ColorGateBlast
{
    /// <summary>
    /// The inner puzzle grid: builds the board / gates / blocks, turns pointer drags into block slides
    /// (all rules come from <see cref="BoardModel"/>), and reports exits and committed moves.
    /// Dragging a block into a border gate of its own colour removes it from the grid.
    /// </summary>
    public sealed class GridManager : MonoBehaviour
    {
        /// <summary>Raised when a block leaves through a gate: (block spec, world centre at exit, exit side).</summary>
        public event Action<BlockSpec, Vector3, Side> BlockExited;
        /// <summary>Raised once per finished drag that changed the board (costs one move).</summary>
        public event Action MoveCommitted;
        /// <summary>Short feedback strings ("Frozen!", "Track full!").</summary>
        public event Action<string> Message;

        /// <summary>Asked before a block may exit; return false when the conveyor has no free slot.</summary>
        public Func<bool> CanExit;
        public bool InputEnabled;

        private sealed class GateView
        {
            public GateSpec spec;
            public GameObject overlay;
            public TextMesh overlayLabel;
        }

        private Camera cam;
        private LevelDefinition def;
        private BoardModel board;
        private BlockView[] views;
        private readonly List<GateView> gateViews = new List<GateView>();
        private Transform root;
        private float cs = 1f;
        private Vector2 originBL;

        private int dragging = -1;
        private Vector2 grabGrid;
        private Int2 startPos;
        private bool dragExited;

        public BoardModel Board { get { return board; } }
        public int RemainingBlocks { get { return board == null ? 0 : board.BlockCount - board.ExitedCount; } }
        public bool IsDragging { get { return dragging >= 0; } }

        public void SetCamera(Camera c) { cam = c; }

        public Vector3 CellWorld(int x, int y)
        {
            return new Vector3(originBL.x + (x + 0.5f) * cs, originBL.y + (y + 0.5f) * cs, 0f);
        }

        public Vector3 CellWorld(Int2 p) { return CellWorld(p.x, p.y); }

        public void Clear()
        {
            dragging = -1;
            if (root != null) Destroy(root.gameObject);
            root = null;
            views = null;
            board = null;
            gateViews.Clear();
        }

        public void Build(LevelDefinition level, Vector2 center, float cellSize)
        {
            Clear();
            def = level;
            cs = cellSize;
            board = new BoardModel(def);
            originBL = center - new Vector2(def.gridWidth * cs * 0.5f, def.gridHeight * cs * 0.5f);

            root = new GameObject("BoardRoot").transform;
            root.SetParent(transform, false);

            BuildPlate(center);
            BuildGates();
            BuildBlocks();
        }

        // ------------------------------------------------------------------ construction

        private void BuildPlate(Vector2 center)
        {
            float margin = cs * 0.42f;
            float w = def.gridWidth * cs + margin * 2f;
            float h = def.gridHeight * cs + margin * 2f;

            GameObject plate = new GameObject("Plate");
            plate.transform.SetParent(root, false);
            plate.transform.position = new Vector3(center.x, center.y, 0.25f);
            plate.AddComponent<MeshFilter>().sharedMesh = ProceduralMeshes.Slab(w, h, 0.12f, cs * 0.4f, cs * 0.1f, "BoardPlate");
            plate.AddComponent<MeshRenderer>().sharedMaterial = Mats.Lit(ColorPalette.BoardPlate, 0.08f);

            MeshBuilder mb = new MeshBuilder();
            float size = cs * 0.94f;
            for (int y = 0; y < def.gridHeight; y++)
                for (int x = 0; x < def.gridWidth; x++)
                {
                    Vector3 c = CellWorld(x, y);
                    mb.AddSlab(new Vector2(c.x, c.y), size, size, 0.03f, cs * 0.2f, cs * 0.05f, 2);
                }
            GameObject cells = new GameObject("Cells");
            cells.transform.SetParent(root, false);
            cells.transform.position = new Vector3(0, 0, 0.13f);
            cells.AddComponent<MeshFilter>().sharedMesh = mb.ToMesh("BoardCells");
            cells.AddComponent<MeshRenderer>().sharedMaterial = Mats.Lit(ColorPalette.BoardCell, 0.05f);
        }

        private void BuildGates()
        {
            float thick = cs * 0.34f;
            for (int g = 0; g < def.gates.Count; g++)
            {
                GateSpec gs = def.gates[g];
                bool horizontal = gs.side == Side.Top || gs.side == Side.Bottom;
                float len = gs.length * cs - cs * 0.12f;
                Vector3 c;
                if (horizontal)
                {
                    float x = originBL.x + (gs.start + gs.length * 0.5f) * cs;
                    float y = gs.side == Side.Top ? originBL.y + def.gridHeight * cs + thick * 0.5f + cs * 0.03f
                                                  : originBL.y - thick * 0.5f - cs * 0.03f;
                    c = new Vector3(x, y, 0.1f);
                }
                else
                {
                    float y = originBL.y + (gs.start + gs.length * 0.5f) * cs;
                    float x = gs.side == Side.Right ? originBL.x + def.gridWidth * cs + thick * 0.5f + cs * 0.03f
                                                    : originBL.x - thick * 0.5f - cs * 0.03f;
                    c = new Vector3(x, y, 0.1f);
                }
                float gw = horizontal ? len : thick;
                float gh = horizontal ? thick : len;

                GameObject go = new GameObject("Gate_" + g);
                go.transform.SetParent(root, false);
                go.transform.position = c;
                go.AddComponent<MeshFilter>().sharedMesh = ProceduralMeshes.Slab(gw, gh, 0.07f, thick * 0.4f, thick * 0.18f, "Gate");
                go.AddComponent<MeshRenderer>().sharedMaterial = Mats.Lit(ColorPalette.Get(gs.color), 0.25f);

                GateView view = new GateView();
                view.spec = gs;
                if (gs.locked)
                {
                    GameObject ov = new GameObject("Lock");
                    ov.transform.SetParent(go.transform, false);
                    ov.transform.localPosition = new Vector3(0, 0, -0.08f);
                    ov.AddComponent<MeshFilter>().sharedMesh = ProceduralMeshes.Slab(gw * 0.96f, gh * 0.96f, 0.04f, thick * 0.38f, thick * 0.12f, "GateLock");
                    ov.AddComponent<MeshRenderer>().sharedMaterial = Mats.Lit(ColorPalette.LockGray, 0.1f);
                    view.overlay = ov;
                    view.overlayLabel = Mats.Label(ov.transform, "LOCK", Mathf.Min(thick * 0.6f, len * 0.3f), Color.white, new Vector3(0, 0, -0.06f));
                    if (!horizontal) view.overlayLabel.transform.localRotation = Quaternion.Euler(0, 0, 90f);
                }
                gateViews.Add(view);
            }
        }

        private void BuildBlocks()
        {
            views = new BlockView[def.blocks.Count];
            for (int i = 0; i < def.blocks.Count; i++)
            {
                GameObject go = new GameObject("Block_" + i);
                go.transform.SetParent(root, false);
                BlockView v = go.AddComponent<BlockView>();
                v.Init(i, def.blocks[i], cs, CellWorld(def.blocks[i].origin));
                views[i] = v;
            }
        }

        // ------------------------------------------------------------------ input

        private Vector2 ScreenToGrid(Vector2 screen)
        {
            Vector3 w = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -cam.transform.position.z));
            return new Vector2((w.x - originBL.x) / cs, (w.y - originBL.y) / cs);
        }

        private void Update()
        {
            if (board == null || cam == null) return;

            if (!InputEnabled)
            {
                if (dragging >= 0) EndDrag();
                return;
            }

            if (dragging < 0)
            {
                if (InputHelper.PressedThisFrame) BeginDrag(InputHelper.PointerPosition);
                return;
            }

            DragUpdate(InputHelper.PointerPosition);
            if (dragging >= 0 && (!InputHelper.IsPressed || InputHelper.ReleasedThisFrame)) EndDrag();
        }

        private void BeginDrag(Vector2 screen)
        {
            Vector2 g = ScreenToGrid(screen);
            int idx = board.At(Mathf.FloorToInt(g.x), Mathf.FloorToInt(g.y));
            if (idx < 0) return;

            views[idx].Reveal();
            if (board.IsFrozen(idx))
            {
                views[idx].Shake();
                if (JuiceManager.Instance != null) JuiceManager.Instance.PlayError();
                if (Message != null) Message("Frozen! Free blocks to thaw it");
                return;
            }

            dragging = idx;
            grabGrid = g;
            startPos = board.Pos[idx];
            dragExited = false;
            views[idx].SetLifted(true);
            if (JuiceManager.Instance != null) JuiceManager.Instance.PlayTick();
        }

        private static int Sign(int v) { return v > 0 ? 1 : (v < 0 ? -1 : 0); }

        private void DragUpdate(Vector2 screen)
        {
            Vector2 delta = ScreenToGrid(screen) - grabGrid;
            int tx = Mathf.RoundToInt(startPos.x + delta.x);
            int ty = Mathf.RoundToInt(startPos.y + delta.y);
            Int2 cur = board.Pos[dragging];

            for (int guard = 0; guard < 40; guard++)
            {
                int dx = tx - cur.x, dy = ty - cur.y;
                if (dx == 0 && dy == 0) break;

                bool xFirst = Mathf.Abs(dx) >= Mathf.Abs(dy);
                bool stepped = false;
                for (int pass = 0; pass < 2 && !stepped; pass++)
                {
                    bool doX = (pass == 0) == xFirst;
                    Int2 dir = doX ? new Int2(Sign(dx), 0) : new Int2(0, Sign(dy));
                    if (dir.x == 0 && dir.y == 0) continue;

                    StepResult r = board.Probe(dragging, cur, dir);
                    if (r == StepResult.Moved)
                    {
                        cur = cur + dir;
                        board.Commit(dragging, cur);
                        views[dragging].SetTarget(CellWorld(cur));
                        stepped = true;
                    }
                    else if (r == StepResult.Exit)
                    {
                        if (CanExit == null || CanExit())
                        {
                            DoExit(dragging, dir);
                            return;
                        }
                        if (Message != null) Message("Track full! Wait for turrets");
                        if (JuiceManager.Instance != null) JuiceManager.Instance.PlayError();
                        views[dragging].Shake();
                    }
                }
                if (!stepped) break;
            }
        }

        private void DoExit(int i, Int2 dir)
        {
            BlockSpec spec = def.blocks[i];
            BlockView view = views[i];

            Vector3 center = CellWorld(board.Pos[i]) + ProceduralMeshes.Centroid(spec.cells, cs);
            bool wasKey = spec.kind == BlockKind.Key;

            board.ExitBlock(i);
            views[i] = null;
            dragging = -1;
            dragExited = true;

            Side side = dir.y > 0 ? Side.Top : (dir.y < 0 ? Side.Bottom : (dir.x > 0 ? Side.Right : Side.Left));
            Vector3 worldDir = new Vector3(dir.x, dir.y, 0f);
            float travel = (Mathf.Max(spec.ExtentX(), spec.ExtentY()) + 1.2f) * cs;
            view.SetLifted(false);
            view.PlayExit(worldDir, travel, null);

            if (JuiceManager.Instance != null) JuiceManager.Instance.BlockLeft(center, ColorPalette.Get(spec.color));

            if (wasKey) UnlockGates();
            RefreshIce();

            if (BlockExited != null) BlockExited(spec, center, side);
            if (MoveCommitted != null) MoveCommitted();
        }

        private void EndDrag()
        {
            int idx = dragging;
            dragging = -1;
            if (idx < 0 || views == null || views[idx] == null) return;
            views[idx].SetLifted(false);
            if (!dragExited && board.Pos[idx] != startPos && MoveCommitted != null) MoveCommitted();
        }

        // ------------------------------------------------------------------ specials

        private void UnlockGates()
        {
            for (int i = 0; i < gateViews.Count; i++)
            {
                GateView gv = gateViews[i];
                if (gv.overlay == null) continue;
                if (JuiceManager.Instance != null)
                {
                    JuiceManager.Instance.Burst(gv.overlay.transform.position, ColorPalette.Get(gv.spec.color), 10, cs * 0.12f);
                    JuiceManager.Instance.PlayUnlock();
                }
                Destroy(gv.overlay);
                gv.overlay = null;
            }
            if (Message != null) Message("Gates unlocked!");
        }

        private void RefreshIce()
        {
            if (views == null) return;
            for (int i = 0; i < views.Length; i++)
            {
                if (views[i] == null) continue;
                BlockSpec s = def.blocks[i];
                if (s.kind != BlockKind.Ice) continue;
                views[i].SetIceRemaining(s.iceThawExits - board.ExitedCount);
            }
        }
    }
}
