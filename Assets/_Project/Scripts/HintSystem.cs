using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace ColorGateBlast
{
    /// <summary>
    /// Smart hint: takes a snapshot of the LIVE game (tray, monument HP, turrets on the belt), asks the same solver the
    /// level checker uses for the next move, then highlights that block with a pulsing golden outline and draws an
    /// animated glowing arrow path from the block to its target gate until the player moves.
    /// Triggered from the Hint button after a Rewarded Ad (see LevelProgressor).
    /// </summary>
    public sealed class HintSystem : MonoBehaviour
    {
        public event Action<string> Message;

        private GridManager grid;
        private ConveyorTrackManager track;
        private MonumentManager monument;
        private Task<LevelSolver.SolveResult> pending;
        private int hintedBlock = -1;
        private Transform arrowRoot;
        private readonly List<Transform> chevrons = new List<Transform>();
        private Vector3 pathA, pathB;
        private bool pathActive;
        private Mesh chevronMesh;

        public bool IsBusy { get { return pending != null; } }

        public void Init(GridManager g, ConveyorTrackManager t, MonumentManager m)
        {
            grid = g;
            track = t;
            monument = m;
            grid.DragStarted += ClearHint;
            grid.MoveCommitted += ClearHint;
        }

        /// <summary>Moves the player still has (set by the level flow); the hint must fit inside this budget.</summary>
        public int MovesLeft;

        private LevelSolver.SolveResult SolveBudget(PuzzleState snap, int cb, int ch)
        {
            LevelSolver.SolveResult r = MovesLeft > 0 ? LevelSolver.Solve(snap, cb, ch, MovesLeft) : null;
            if (r == null || !r.solved) r = LevelSolver.Solve(snap, cb, ch);
            return r;
        }

        /// <summary>Starts a background solve of the current position.</summary>
        public void RequestHint()
        {
            if (pending != null || grid == null || grid.Board == null || monument.Model == null) return;
            ClearHint();
            PuzzleState snap = new PuzzleState(grid.Level, grid.Board, monument.Model, track.Snapshot());
#if UNITY_WEBGL && !UNITY_EDITOR
            // WebGL has no threads: solve on the main thread with a smaller budget so the page stays responsive.
            Present(SolveBudget(snap, 12000, 90000));
#else
            pending = Task<LevelSolver.SolveResult>.Factory.StartNew(delegate () { return SolveBudget(snap, 30000, 300000); });
            if (Message != null) Message("Thinking...");
#endif
        }

        private void Update()
        {
            if (pending != null && pending.IsCompleted)
            {
                LevelSolver.SolveResult r = pending.IsFaulted ? null : pending.Result;
                pending = null;
                Present(r);
            }
            AnimatePath();
        }

        private void Present(LevelSolver.SolveResult r)
        {
            Move m = new Move();
            bool have = false;
            if (r != null && r.solved && r.moves > 0) { m = r.firstMove; have = true; }
            if (!have)
            {
                // fall back to any legal exit, then any legal slide
                List<Move> all = new List<Move>();
                grid.Board.GenerateMoves(all, null);
                for (int i = 0; i < all.Count && !have; i++) if (all[i].exit) { m = all[i]; have = true; }
                if (!have && all.Count > 0) { m = all[0]; have = true; }
            }
            if (!have)
            {
                if (Message != null) Message("No hint available - try restarting the level");
                return;
            }
            Show(m);
        }

        private void Show(Move m)
        {
            BlockView v = grid.GetView(m.block);
            if (v == null) return;
            hintedBlock = m.block;
            v.SetHint(true);

            BlockInfo info = grid.Board.Blocks[m.block];
            BlockColor curColor = grid.Board.ColorOf(m.block);
            Vector3 start = v.CentroidWorld;
            Vector3 offset = v.CentroidWorld - grid.CellWorld(grid.Board.Pos[m.block]);
            Vector3 end;
            bool viaPortal;
            Int2 pad = FindPadOnPath(m, out viaPortal);
            if (viaPortal)
            {
                end = grid.CellWorld(pad) + offset; // the arrow points INTO the portal pad
            }
            else if (m.exit)
            {
                int gi = FindGate(m);
                end = gi >= 0 ? grid.GateCenterWorld(gi) : start + new Vector3(Dirs.Of(m.dir).x, 0f, Dirs.Of(m.dir).y) * 3f;
            }
            else
            {
                end = grid.CellWorld(m.to) + offset;
            }
            start.y = 0.65f;
            end.y = 0.65f;
            StartPath(start, end, GameMaterials.ColorOf(curColor));

            string dirName = m.dir.ToString().ToLower();
            string msg;
            string who = info.IsPrism ? "prism (now " + curColor + ")" : curColor + " block";
            if (viaPortal) msg = "Hint: slide the " + who + " " + dirName + " onto the portal pad - it warps!";
            else if (m.exit) msg = "Hint: slide the " + who + " " + dirName + " into its gate";
            else msg = "Hint: slide the " + who + " " + dirName;
            if (info.partner >= 0 && grid.Board.Linked[m.block]) msg += " (its welded partner comes along)";
            if (m.exit && TurretWouldUnscrew(curColor)) msg += " (its turret unscrews a brass bolt)";
            if (Message != null) Message(msg);
        }

        /// <summary>True when a block of this colour leaving will unscrew a visible brass screw (shown in the hint text).</summary>
        private bool TurretWouldUnscrew(BlockColor c)
        {
            MonumentModel mm = monument.Model;
            for (int j = 0; j < mm.Parts.Length; j++)
                if (mm.IsScrewTargetable(j) && mm.Parts[j].screwColor == c) return true;
            return false;
        }

        /// <summary>
        /// Walks the move cell by cell; if a 1x1 block steps onto a portal pad on the way, returns that pad
        /// (the hint arrow then points into the pad instead of past it).
        /// </summary>
        private Int2 FindPadOnPath(Move m, out bool found)
        {
            found = false;
            BoardModel b = grid.Board;
            if (b.Blocks[m.block].cells.Length != 1) return new Int2(0, 0);
            Int2 dir = Dirs.Of(m.dir);
            Int2 o = b.Pos[m.block];
            for (int guard = 0; guard < 32; guard++)
            {
                Int2 next = o + dir;
                if (b.IsPortal(next)) { found = true; return next; }
                if (!b.InGrid(next) || next == m.to) break;
                o = next;
            }
            return new Int2(0, 0);
        }

        private int FindGate(Move m)
        {
            BoardModel b = grid.Board;
            BlockInfo info = b.Blocks[m.block];
            bool horiz = m.dir == GateDirection.Up || m.dir == GateDirection.Down;
            int lo = int.MaxValue, hi = int.MinValue;
            for (int i = 0; i < info.cells.Length; i++)
            {
                Int2 p = m.to + info.cells[i];
                int lane = horiz ? p.x : p.y;
                if (lane < lo) lo = lane;
                if (lane > hi) hi = lane;
            }
            List<GateSpec> gates = b.Def.gates;
            for (int g = 0; g < gates.Count; g++)
            {
                GateSpec gs = gates[g];
                if (gs.direction == m.dir && gs.color == b.ColorOf(m.block) && lo >= gs.start && hi < gs.start + gs.length) return g;
            }
            return -1;
        }

        public void ClearHint()
        {
            if (hintedBlock >= 0)
            {
                BlockView v = grid != null ? grid.GetView(hintedBlock) : null;
                if (v != null) v.SetHint(false);
                hintedBlock = -1;
            }
            pathActive = false;
            if (arrowRoot != null) arrowRoot.gameObject.SetActive(false);
        }

        // ------------------------------------------------------------------ animated arrow path

        private void StartPath(Vector3 a, Vector3 b, Color tint)
        {
            pathA = a;
            pathB = b;
            if (arrowRoot == null)
            {
                arrowRoot = new GameObject("HintArrows").transform;
                arrowRoot.SetParent(transform, false);
                MeshBuilder mb = new MeshBuilder();
                mb.AddTriangleUp(new Vector3(0f, 0f, 0.34f), new Vector3(0.3f, 0f, -0.18f), new Vector3(0f, 0f, -0.04f));
                mb.AddTriangleUp(new Vector3(0f, 0f, 0.34f), new Vector3(0f, 0f, -0.04f), new Vector3(-0.3f, 0f, -0.18f));
                chevronMesh = mb.ToMesh("Chevron");
                for (int i = 0; i < 6; i++)
                {
                    GameObject c = new GameObject("Chevron" + i);
                    c.transform.SetParent(arrowRoot, false);
                    c.AddComponent<MeshFilter>().sharedMesh = chevronMesh;
                    c.AddComponent<MeshRenderer>().sharedMaterial = GameMaterials.UnlitGlow(new Color(1f, 0.82f, 0.2f), 2.4f);
                    chevrons.Add(c.transform);
                }
            }
            arrowRoot.gameObject.SetActive(true);
            pathActive = true;
        }

        private void AnimatePath()
        {
            if (!pathActive || arrowRoot == null) return;
            Vector3 d = pathB - pathA;
            float len = d.magnitude;
            if (len < 0.01f) return;
            Quaternion rot = Quaternion.LookRotation(new Vector3(d.x, 0f, d.z).normalized, Vector3.up);
            for (int i = 0; i < chevrons.Count; i++)
            {
                float u = Mathf.Repeat(Time.time * 0.7f + i / (float)chevrons.Count, 1f);
                chevrons[i].position = Vector3.Lerp(pathA, pathB, u) + new Vector3(0f, 0.04f * Mathf.Sin(Time.time * 8f + i), 0f);
                chevrons[i].rotation = rot;
                float fade = Mathf.Sin(u * Mathf.PI);
                chevrons[i].localScale = Vector3.one * (0.55f + 0.6f * fade);
            }
        }
    }
}
