using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorGateBlast
{
    /// <summary>
    /// The recess-tray puzzle: builds the 3D tray, glowing gates (with animated shutter doors) and the blocks, turns
    /// pointer drags into block slides (rules come from <see cref="BoardModel"/>) and reports exits.
    /// Dragging a block into the open gate of its colour removes it from the tray.
    /// </summary>
    public sealed class GridManager : MonoBehaviour
    {
        public const float TrayTop = 0.3f;

        /// <summary>Raised when a block leaves through a gate: (block index, world centre at exit, exit direction, gate is a Clockwork Piercing Gate).</summary>
        public event Action<int, Vector3, GateDirection, bool> BlockExited;
        /// <summary>Raised once per finished drag that changed the board (costs one move).</summary>
        public event Action MoveCommitted;
        /// <summary>Short feedback strings ("Frozen!", "Track full!").</summary>
        public event Action<string> Message;
        /// <summary>Raised when the player starts dragging a block (used to clear a shown hint).</summary>
        public event Action DragStarted;

        /// <summary>Asked before a block may exit; return false when the conveyor has no free slot for that colour.</summary>
        public Func<BlockColor, bool> CanExit;
        public bool InputEnabled;

        private sealed class PortalView
        {
            public Int2 cell;
            public Int2 partner;
            public Renderer ring;
            public Transform[] orbiters;
            public bool cyan;
            public Vector3 center;
        }

        private sealed class GateView
        {
            public int index;
            public GateSpec spec;
            public GameObject door;
            public Renderer led;
            public Vector3 closedPos, openPos;
            public Vector3 closedScale, openScale;
            public bool lastOpen;
            public Vector3 center;
            public GameObject lockVisual;
        }

        private Camera cam;
        private LevelDefinition def;
        private BoardModel board;
        private BlockView[] views;
        private readonly List<GateView> gateViews = new List<GateView>();
        private Transform root;
        private float cs = 1f;
        private Vector3 originBL; // world position of the bottom-left corner of the grid (at TrayTop)
        private int dragging = -1;
        private Vector2 grabGrid;
        private Int2 startPos;
        private bool dragExited;
        private readonly LineRenderer[] preview = new LineRenderer[4];
        private float gatePulse;
        private readonly List<PortalView> portalViews = new List<PortalView>();
        private readonly List<Transform> spinners = new List<Transform>();
        private float nextNudge;

        public BoardModel Board { get { return board; } }
        public LevelDefinition Level { get { return def; } }
        public float CellSize { get { return cs; } }
        public int RemainingBlocks { get { return board == null ? 0 : board.RemainingMovable; } }
        public bool IsDragging { get { return dragging >= 0; } }
        public BlockView GetView(int i) { return (views != null && i >= 0 && i < views.Length) ? views[i] : null; }

        public void SetCamera(Camera c) { cam = c; }

        public Vector3 CellWorld(int x, int y) { return new Vector3(originBL.x + (x + 0.5f) * cs, TrayTop, originBL.z + (y + 0.5f) * cs); }
        public Vector3 CellWorld(Int2 p) { return CellWorld(p.x, p.y); }

        public Vector3 GateCenterWorld(int gateIndex) { return gateViews[gateIndex].center; }

        public void Clear()
        {
            StopAllCoroutines(); // door / key-flight animations must not outlive the objects they animate
            dragging = -1;
            if (root != null) Destroy(root.gameObject);
            root = null;
            views = null;
            board = null;
            gateViews.Clear();
            portalViews.Clear();
            spinners.Clear();
        }

        public void Build(LevelDefinition level, Vector3 centerWorld, float cellSize)
        {
            Clear();
            def = level;
            cs = cellSize;
            board = new BoardModel(def);
            originBL = new Vector3(centerWorld.x - def.GridWidth * cs * 0.5f, TrayTop, centerWorld.z - def.GridHeight * cs * 0.5f);

            root = new GameObject("BoardRoot").transform;
            root.SetParent(transform, false);

            BuildTray(centerWorld);
            BuildGates();
            BuildPortals();
            BuildRails();
            BuildBlocks();
            BuildPreviewLines();
            UpdateShutters(false);
        }

        // ------------------------------------------------------------------ construction

        private void BuildTray(Vector3 center)
        {
            float margin = cs * 0.55f;
            float w = def.GridWidth * cs + margin * 2f;
            float d = def.GridHeight * cs + margin * 2f;

            GameObject baseGo = new GameObject("TrayBase");
            baseGo.transform.SetParent(root, false);
            baseGo.AddComponent<MeshFilter>().sharedMesh = new MeshBuilder().AddSlabReturn(center.x, center.z, w, d, TrayTop, cs * 0.35f, cs * 0.08f, 0f, 4).ToMesh("TrayBase", 0.4f);
            baseGo.AddComponent<MeshRenderer>().sharedMaterial = GameMaterials.Gunmetal;

            MeshBuilder wells = new MeshBuilder();
            float size = cs * 0.95f;
            for (int y = 0; y < def.GridHeight; y++)
                for (int x = 0; x < def.GridWidth; x++)
                {
                    Vector3 c = CellWorld(x, y);
                    wells.AddSlab(c.x, c.z, size, size, 0.03f, cs * 0.16f, cs * 0.04f, TrayTop - 0.015f, 2);
                }
            GameObject wellGo = new GameObject("CellWells");
            wellGo.transform.SetParent(root, false);
            wellGo.AddComponent<MeshFilter>().sharedMesh = wells.ToMesh("CellWells");
            wellGo.AddComponent<MeshRenderer>().sharedMaterial = GameMaterials.Rubber;

            // corner bolts
            MeshBuilder bolts = new MeshBuilder();
            float bx = w * 0.5f - margin * 0.5f, bz = d * 0.5f - margin * 0.5f;
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sz = -1; sz <= 1; sz += 2)
                    bolts.AddSphere(new Vector3(center.x + sx * bx, TrayTop + 0.02f, center.z + sz * bz), cs * 0.09f, 8, 4);
            GameObject boltGo = new GameObject("TrayBolts");
            boltGo.transform.SetParent(root, false);
            boltGo.AddComponent<MeshFilter>().sharedMesh = bolts.ToMesh("TrayBolts");
            boltGo.AddComponent<MeshRenderer>().sharedMaterial = GameMaterials.Brass;
        }

        private void BuildGates()
        {
            float thick = cs * 0.3f;
            for (int g = 0; g < def.gates.Count; g++)
            {
                GateSpec gs = def.gates[g];
                bool horiz = gs.direction == GateDirection.Up || gs.direction == GateDirection.Down;
                float len = gs.length * cs * 0.94f;
                Vector3 c;
                if (horiz)
                {
                    float x = originBL.x + (gs.start + gs.length * 0.5f) * cs;
                    float z = gs.direction == GateDirection.Up ? originBL.z + def.GridHeight * cs + thick * 0.55f : originBL.z - thick * 0.55f;
                    c = new Vector3(x, 0f, z);
                }
                else
                {
                    float z = originBL.z + (gs.start + gs.length * 0.5f) * cs;
                    float x = gs.direction == GateDirection.Right ? originBL.x + def.GridWidth * cs + thick * 0.55f : originBL.x - thick * 0.55f;
                    c = new Vector3(x, 0f, z);
                }
                float gw = horiz ? len : thick, gd = horiz ? thick : len;

                GameObject go = new GameObject("Gate_" + g);
                go.transform.SetParent(root, false);
                // brass frame + glowing colour bar
                MeshBuilder fb = new MeshBuilder();
                fb.AddSlab(c.x, c.z, gw + 0.12f, gd + 0.12f, 0.2f, thick * 0.3f, thick * 0.1f, TrayTop - 0.1f, 3);
                GameObject frame = new GameObject("Frame");
                frame.transform.SetParent(go.transform, false);
                frame.AddComponent<MeshFilter>().sharedMesh = fb.ToMesh("GateFrame");
                frame.AddComponent<MeshRenderer>().sharedMaterial = GameMaterials.Brass;

                MeshBuilder bb = new MeshBuilder();
                bb.AddSlab(c.x, c.z, gw, gd, 0.07f, thick * 0.28f, thick * 0.08f, TrayTop + 0.1f, 3);
                GameObject bar = new GameObject("GlowBar");
                bar.transform.SetParent(go.transform, false);
                bar.AddComponent<MeshFilter>().sharedMesh = bb.ToMesh("GateBar");
                bar.AddComponent<MeshRenderer>().sharedMaterial = GameMaterials.Glow(gs.color);

                GateView gv = new GateView();
                gv.index = g;
                gv.spec = gs;
                gv.center = new Vector3(c.x, TrayTop + 0.1f, c.z);
                if (gs.isShutterGate)
                {
                    MeshBuilder db = new MeshBuilder();
                    db.AddSlab(0f, 0f, gw * 1.02f, gd * 1.02f, 0.42f, thick * 0.2f, thick * 0.06f, 0f, 2);
                    GameObject door = new GameObject("ShutterDoor");
                    door.transform.SetParent(go.transform, false);
                    door.AddComponent<MeshFilter>().sharedMesh = db.ToMesh("ShutterDoor", 1.2f);
                    door.AddComponent<MeshRenderer>().sharedMaterial = GameMaterials.Hazard;
                    gv.door = door;
                    gv.closedPos = new Vector3(c.x, TrayTop + 0.13f, c.z);
                    gv.closedScale = Vector3.one;
                    // open: the door retracts to a thin slab at the gate's start end
                    Vector3 axis = horiz ? new Vector3(1f, 0f, 0f) : new Vector3(0f, 0f, 1f);
                    float shrink = 0.14f;
                    gv.openScale = horiz ? new Vector3(shrink, 1f, 1f) : new Vector3(1f, 1f, shrink);
                    gv.openPos = gv.closedPos - axis * (len * 0.5f * (1f - shrink));
                    door.transform.position = gv.closedPos;

                    GameObject led = new GameObject("LED");
                    led.transform.SetParent(go.transform, false);
                    Vector3 side = horiz ? new Vector3(0f, 0f, (gs.direction == GateDirection.Up ? 1f : -1f) * (thick * 0.62f)) : new Vector3((gs.direction == GateDirection.Right ? 1f : -1f) * (thick * 0.62f), 0f, 0f);
                    led.transform.position = new Vector3(c.x, TrayTop + 0.5f, c.z) + side;
                    led.transform.localScale = Vector3.one * (cs * 0.16f);
                    led.AddComponent<MeshFilter>().sharedMesh = Prims.Sphere;
                    gv.led = led.AddComponent<MeshRenderer>();
                }
                if (gs.isPadlocked) gv.lockVisual = BuildPadlock(go.transform, c, horiz, gw, gd);
                if (gs.isPiercingGate) BuildPiercingGears(go.transform, c, horiz, gw, gd);
                gateViews.Add(gv);
            }
        }

        /// <summary>Heavy iron chains and a brass padlock sealing a gate until its key has left.</summary>
        private GameObject BuildPadlock(Transform parent, Vector3 c, bool horiz, float gw, float gd)
        {
            GameObject root = new GameObject("Padlock");
            root.transform.SetParent(parent, false);
            float y = TrayTop + 0.45f;
            // chains: three iron bars across the gate
            for (int k = 0; k < 3; k++)
            {
                float f = (k - 1) * 0.3f;
                Vector3 pos = horiz ? new Vector3(c.x + f * gw, y, c.z) : new Vector3(c.x, y, c.z + f * gd);
                Vector3 scale = horiz ? new Vector3(0.1f, 0.1f, gd * 1.2f) : new Vector3(gw * 1.2f, 0.1f, 0.1f);
                Prims.Make("Chain" + k, root.transform, Prims.Cube, GameMaterials.Gunmetal, pos, scale);
                Prims.Make("Link" + k, root.transform, Prims.Sphere, GameMaterials.Steel, pos + new Vector3(0f, 0.08f, 0f), Vector3.one * 0.14f);
            }
            // brass padlock body + shackle
            Prims.Make("PadlockBody", root.transform, Prims.Cube, GameMaterials.Brass, new Vector3(c.x, y + 0.18f, c.z), new Vector3(0.5f, 0.4f, 0.28f));
            Prims.Make("ShackleL", root.transform, Prims.Cylinder, GameMaterials.Steel, new Vector3(c.x - 0.14f, y + 0.5f, c.z), new Vector3(0.06f, 0.16f, 0.06f));
            Prims.Make("ShackleR", root.transform, Prims.Cylinder, GameMaterials.Steel, new Vector3(c.x + 0.14f, y + 0.5f, c.z), new Vector3(0.06f, 0.16f, 0.06f));
            Prims.Make("ShackleTop", root.transform, Prims.Cube, GameMaterials.Steel, new Vector3(c.x, y + 0.66f, c.z), new Vector3(0.34f, 0.06f, 0.06f));
            return root;
        }

        /// <summary>Spinning brass watch gears framing a Clockwork Piercing Gate.</summary>
        private void BuildPiercingGears(Transform parent, Vector3 c, bool horiz, float gw, float gd)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Vector3 off = horiz ? new Vector3(side * (gw * 0.5f + 0.12f), 0f, 0f) : new Vector3(0f, 0f, side * (gd * 0.5f + 0.12f));
                GameObject gear = new GameObject("PiercingGear");
                gear.transform.SetParent(parent, false);
                gear.transform.position = new Vector3(c.x, TrayTop + 0.35f, c.z) + off;
                Prims.Make("Disc", gear.transform, Prims.Cylinder, GameMaterials.Brass, Vector3.zero, new Vector3(0.5f, 0.05f, 0.5f));
                for (int k = 0; k < 8; k++)
                {
                    float a = k * 45f * Mathf.Deg2Rad;
                    Prims.Make("Tooth" + k, gear.transform, Prims.Cube, GameMaterials.Brass, new Vector3(Mathf.Cos(a) * 0.27f, 0f, Mathf.Sin(a) * 0.27f), new Vector3(0.1f, 0.06f, 0.1f), new Vector3(0f, -k * 45f, 0f));
                }
                spinners.Add(gear.transform);
            }
        }

        // ------------------------------------------------------------------ portals + rails

        private void BuildPortals()
        {
            for (int k = 0; k < def.portals.Count; k++)
            {
                PortalSpec ps = def.portals[k];
                bool cyan = ps.pair == 0;
                Int2[] ends = { ps.a, ps.b };
                Int2[] others = { ps.b, ps.a };
                for (int e = 0; e < 2; e++)
                {
                    Vector3 c = CellWorld(ends[e]);
                    GameObject go = new GameObject("Portal" + k + (e == 0 ? "A" : "B"));
                    go.transform.SetParent(root, false);
                    go.transform.position = new Vector3(c.x, TrayTop + 0.02f, c.z);
                    Prims.Make("Rim", go.transform, Prims.Cylinder, GameMaterials.Brass, Vector3.zero, new Vector3(cs * 0.86f, 0.03f, cs * 0.86f));
                    Prims.Make("Well", go.transform, Prims.Cylinder, GameMaterials.Rubber, new Vector3(0f, 0.02f, 0f), new Vector3(cs * 0.7f, 0.03f, cs * 0.7f));
                    GameObject neon = Prims.Make("Neon", go.transform, Prims.Cylinder, cyan ? GameMaterials.PortalCyan : GameMaterials.PortalViolet, new Vector3(0f, 0.045f, 0f), new Vector3(cs * 0.6f, 0.012f, cs * 0.6f));
                    PortalView pv = new PortalView();
                    pv.cell = ends[e];
                    pv.partner = others[e];
                    pv.cyan = cyan;
                    pv.center = c;
                    pv.ring = neon.GetComponent<Renderer>();
                    pv.orbiters = new Transform[3];
                    for (int o = 0; o < 3; o++)
                    {
                        GameObject orb = Prims.Make("Swirl" + o, go.transform, Prims.Sphere, cyan ? GameMaterials.PortalCyan : GameMaterials.PortalViolet, Vector3.zero, Vector3.one * cs * 0.1f);
                        pv.orbiters[o] = orb.transform;
                    }
                    portalViews.Add(pv);
                }
            }
        }

        /// <summary>Recessed steel guide rails on the tray floor under every direction-locked block.</summary>
        private void BuildRails()
        {
            MeshBuilder rb = new MeshBuilder();
            bool any = false;
            for (int i = 0; i < board.BlockCount; i++)
            {
                BlockInfo b = board.Blocks[i];
                if (b.axis == MoveAxis.Free || b.isCompanion) continue;
                any = true;
                List<int> lines = new List<int>();
                for (int c = 0; c < b.cells.Length; c++)
                {
                    int ln = b.axis == MoveAxis.Horizontal ? b.origin.y + b.cells[c].y : b.origin.x + b.cells[c].x;
                    if (!lines.Contains(ln)) lines.Add(ln);
                }
                for (int k = 0; k < lines.Count; k++)
                {
                    float len = (b.axis == MoveAxis.Horizontal ? def.GridWidth : def.GridHeight) * cs * 0.96f;
                    for (int side = -1; side <= 1; side += 2)
                    {
                        float off = side * cs * 0.3f;
                        if (b.axis == MoveAxis.Horizontal)
                        {
                            Vector3 c0 = CellWorld(0, lines[k]);
                            rb.AddSlab(originBL.x + def.GridWidth * cs * 0.5f, c0.z + off, len, cs * 0.07f, 0.02f, cs * 0.03f, cs * 0.01f, TrayTop - 0.005f, 1);
                        }
                        else
                        {
                            Vector3 c0 = CellWorld(lines[k], 0);
                            rb.AddSlab(c0.x + off, originBL.z + def.GridHeight * cs * 0.5f, cs * 0.07f, len, 0.02f, cs * 0.03f, cs * 0.01f, TrayTop - 0.005f, 1);
                        }
                    }
                }
            }
            if (!any) return;
            GameObject go = new GameObject("GuideRails");
            go.transform.SetParent(root, false);
            go.AddComponent<MeshFilter>().sharedMesh = rb.ToMesh("GuideRails");
            go.AddComponent<MeshRenderer>().sharedMaterial = GameMaterials.Steel;
        }

        private void AnimateExtras()
        {
            for (int i = 0; i < spinners.Count; i++) if (spinners[i] != null) spinners[i].Rotate(0f, 70f * Time.deltaTime, 0f, Space.Self);
            for (int i = 0; i < portalViews.Count; i++)
            {
                PortalView pv = portalViews[i];
                bool blocked = board.At(pv.partner.x, pv.partner.y) >= 0;
                float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * 6f);
                if (pv.ring != null)
                    pv.ring.sharedMaterial = blocked && pulse > 0.4f ? GameMaterials.PortalAmber : (pv.cyan ? GameMaterials.PortalCyan : GameMaterials.PortalViolet);
                for (int o = 0; o < pv.orbiters.Length; o++)
                {
                    float a = Time.time * (blocked ? 1.2f : 4f) + o * 2.094f;
                    float r = cs * (0.12f + 0.16f * (0.5f + 0.5f * Mathf.Sin(Time.time * 2f + o)));
                    pv.orbiters[o].position = pv.center + new Vector3(Mathf.Cos(a) * r, 0.12f + 0.1f * Mathf.Sin(a * 1.5f), Mathf.Sin(a) * r);
                }
            }
        }

        private void BuildBlocks()
        {
            views = new BlockView[board.BlockCount];
            for (int i = 0; i < board.BlockCount; i++)
            {
                GameObject go = new GameObject("Block_" + i + "_" + board.Blocks[i].color);
                go.transform.SetParent(root, false);
                BlockView v = go.AddComponent<BlockView>();
                v.Init(i, board.Blocks[i], cs, CellWorld(board.Blocks[i].isCompanion ? new Int2(0, 0) : board.Blocks[i].origin));
                if (board.Dormant[i]) v.SetHidden();
                views[i] = v;
            }
        }

        private void BuildPreviewLines()
        {
            for (int i = 0; i < preview.Length; i++)
            {
                GameObject go = new GameObject("PreviewLine" + i);
                go.transform.SetParent(root, false);
                LineRenderer lr = go.AddComponent<LineRenderer>();
                lr.positionCount = 2;
                lr.startWidth = cs * 0.1f;
                lr.endWidth = cs * 0.1f;
                lr.sharedMaterial = GameMaterials.UnlitGlow(Color.white, 1.2f);
                lr.enabled = false;
                preview[i] = lr;
            }
        }

        // ------------------------------------------------------------------ shutters

        public void UpdateShutters(bool animate)
        {
            for (int i = 0; i < gateViews.Count; i++)
            {
                GateView gv = gateViews[i];
                if (gv.door == null) continue;
                bool open = board.IsGateOpen(gv.index);
                bool changed = animate && open != gv.lastOpen;
                gv.lastOpen = open;
                gv.led.sharedMaterial = open ? GameMaterials.LedGreen : GameMaterials.LedAmber;
                Vector3 p = open ? gv.openPos : gv.closedPos;
                Vector3 s = open ? gv.openScale : gv.closedScale;
                if (animate) StartCoroutine(MoveDoor(gv.door.transform, p, s, open));
                else { gv.door.transform.position = p; gv.door.transform.localScale = s; }
                if (changed && SoundSynth.Instance != null) SoundSynth.Instance.Play(Sfx.ShutterClank);
            }
        }

        private IEnumerator MoveDoor(Transform door, Vector3 pos, Vector3 scale, bool open)
        {
            Vector3 p0 = door.position, s0 = door.localScale;
            float t = 0f, dur = open ? 0.35f : 0.16f; // opens smoothly, slams shut
            while (t < 1f)
            {
                if (door == null) yield break;
                t += Time.deltaTime / dur;
                float e = open ? 1f - (1f - t) * (1f - t) : t * t * t;
                door.position = Vector3.Lerp(p0, pos, e);
                door.localScale = Vector3.Lerp(s0, scale, e);
                yield return null;
            }
            if (door == null) yield break;
            door.position = pos;
            door.localScale = scale;
            if (!open && FXManager.Instance != null) FXManager.Instance.Sparks(pos + new Vector3(0f, 0.3f, 0f), 6);
        }

        // ------------------------------------------------------------------ input

        private Vector2 ScreenToGrid(Vector2 screen)
        {
            Ray ray = cam.ScreenPointToRay(new Vector3(screen.x, screen.y, 0f));
            Plane plane = new Plane(Vector3.up, new Vector3(0f, TrayTop, 0f));
            float dist;
            if (!plane.Raycast(ray, out dist)) return new Vector2(-999f, -999f);
            Vector3 p = ray.GetPoint(dist);
            return new Vector2((p.x - originBL.x) / cs, (p.z - originBL.z) / cs);
        }

        private void Update()
        {
            if (board == null || cam == null) return;

            // pulsing gate glow
            gatePulse += Time.deltaTime * 3f;
            float pulse = 1.3f + 0.7f * Mathf.Sin(gatePulse);
            for (int i = 0; i < gateViews.Count; i++)
            {
                Color gc = GameMaterials.ColorOf(gateViews[i].spec.color);
                GameMaterials.Glow(gateViews[i].spec.color).SetColor("_EmissionColor", gc * pulse);
            }

            AnimateExtras();

            if (!InputEnabled)
            {
                if (dragging >= 0) EndDrag();
                HidePreview();
                return;
            }

            if (dragging < 0)
            {
                if (InputHelper.PressedThisFrame && !InputHelper.OverUi) BeginDrag(InputHelper.PointerPosition);
                return;
            }

            DragUpdate(InputHelper.PointerPosition);
            if (dragging >= 0)
            {
                UpdatePreview();
                if (!InputHelper.IsPressed || InputHelper.ReleasedThisFrame) EndDrag();
            }
            else HidePreview();
        }

        private void BeginDrag(Vector2 screen)
        {
            Vector2 g = ScreenToGrid(screen);
            int idx = board.At(Mathf.FloorToInt(g.x), Mathf.FloorToInt(g.y));
            if (idx < 0) return;

            if (board.Blocks[idx].IsObstacle)
            {
                views[idx].Shake();
                if (SoundSynth.Instance != null) SoundSynth.Instance.Play(Sfx.MetalPing);
                if (Message != null) Message("Scrap iron - it never moves");
                return;
            }
            if (board.IsFrozen(idx))
            {
                views[idx].Shake();
                if (SoundSynth.Instance != null) SoundSynth.Instance.Play(Sfx.ErrorBuzz);
                if (Message != null) Message("Frozen! Send a neighbouring block out to thaw it");
                return;
            }

            dragging = idx;
            grabGrid = g;
            startPos = board.Pos[idx];
            dragExited = false;
            views[idx].SetLifted(true);
            if (SoundSynth.Instance != null) SoundSynth.Instance.Play(Sfx.BlockPick);
            if (DragStarted != null) DragStarted();
        }

        private static int Sign(int v) { return v > 0 ? 1 : (v < 0 ? -1 : 0); }

        private void DragUpdate(Vector2 screen)
        {
            Vector2 now = ScreenToGrid(screen);
            if (now.x < -900f) return;
            Vector2 delta = now - grabGrid;
            int tx = Mathf.RoundToInt(startPos.x + delta.x);
            int ty = Mathf.RoundToInt(startPos.y + delta.y);
            Int2 cur = board.Pos[dragging];

            // rail blocks resist sideways drags with a tactile spring nudge
            MoveAxis axis = board.Blocks[dragging].axis;
            if (axis != MoveAxis.Free && Time.time > nextNudge)
            {
                float perp = axis == MoveAxis.Horizontal ? Mathf.Abs(delta.y) : Mathf.Abs(delta.x);
                float along = axis == MoveAxis.Horizontal ? Mathf.Abs(delta.x) : Mathf.Abs(delta.y);
                if (perp > 0.7f && perp > along)
                {
                    nextNudge = Time.time + 0.4f;
                    views[dragging].Shake();
                    if (SoundSynth.Instance != null) SoundSynth.Instance.Play(Sfx.MetalPing, 0.7f, 0.35f);
                    if (FXManager.Instance != null) FXManager.Instance.Haptic();
                }
            }

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
                        Int2 landing = board.Land(dragging, cur, dir);
                        bool warped = landing != cur + dir;
                        Int2 before = cur;
                        cur = landing;
                        board.Commit(dragging, cur);
                        if (warped) WarpEffect(dragging, before + dir, cur);
                        else views[dragging].SetTarget(CellWorld(cur));
                        stepped = true;
                        if (SoundSynth.Instance != null) SoundSynth.Instance.Play(Sfx.BlockSnap, UnityEngine.Random.Range(0.95f, 1.1f), 0.5f);
                        if (FXManager.Instance != null) FXManager.Instance.Haptic();
                    }
                    else if (r == StepResult.Exit)
                    {
                        if (CanExit == null || CanExit(board.Blocks[dragging].color))
                        {
                            DoExit(dragging, dir);
                            return;
                        }
                        if (Message != null) Message("Track full! Let the turrets finish first");
                        if (SoundSynth.Instance != null) SoundSynth.Instance.Play(Sfx.ErrorBuzz);
                        views[dragging].Shake();
                    }
                }
                if (!stepped) break;
            }
        }

        /// <summary>
        /// Performs a move exactly like a finished drag would (cell-by-cell stepping, warps, exits, move count).
        /// Used by the autoplay test bot; returns false if the move could not be completed.
        /// </summary>
        public bool AutoMove(Move m)
        {
            if (board == null || views == null || m.block < 0 || m.block >= views.Length) return false;
            int i = m.block;
            if (views[i] == null || board.Exited[i] || board.Dormant[i] || board.Blocks[i].IsObstacle || board.Ice[i] > 0) return false;

            dragging = i;
            startPos = board.Pos[i];
            dragExited = false;
            Int2 dir = Dirs.Of(m.dir);
            Int2 cur = board.Pos[i];
            for (int guard = 0; guard < 64; guard++)
            {
                if (!m.exit && cur == m.to) break;
                StepResult r = board.Probe(i, cur, dir);
                if (r == StepResult.Blocked) break;
                if (r == StepResult.Exit)
                {
                    if (!m.exit || (CanExit != null && !CanExit(board.Blocks[i].color))) { dragging = -1; HidePreview(); return false; }
                    DoExit(i, dir);
                    return true;
                }
                Int2 landing = board.Land(i, cur, dir);
                bool warped = landing != cur + dir;
                Int2 before = cur;
                cur = landing;
                board.Commit(i, cur);
                if (warped) WarpEffect(i, before + dir, cur);
                else views[i].SetTarget(CellWorld(cur));
            }
            bool reached = board.Pos[i] == m.to;
            EndDrag();
            return reached;
        }

        private void DoExit(int i, Int2 dir)
        {
            BlockView view = views[i];
            Vector3 center = view.CentroidWorld;
            GateDirection gd = Dirs.From(dir);

            ExitEffects fx = new ExitEffects();
            board.ExitBlock(i, fx, gd, startPos, board.Pos[i]);
            views[i] = null;
            dragging = -1;
            dragExited = true;
            HidePreview();

            view.SetLifted(false);
            float travel = (Mathf.Max(board.Blocks[i].ExtentX(), board.Blocks[i].ExtentY()) + 1.4f) * cs;
            view.PlayExit(new Vector3(dir.x, 0f, dir.y), travel, null);

            if (SoundSynth.Instance != null) SoundSynth.Instance.Play(Sfx.GateExit);
            if (FXManager.Instance != null)
            {
                FXManager.Instance.Sparks(center, 12);
                FXManager.Instance.Haptic();
            }

            for (int k = 0; k < fx.thawed.Count; k++)
            {
                int j = fx.thawed[k];
                if (views[j] != null) views[j].SetIce(board.Ice[j], true);
            }
            if (fx.spawned >= 0 && views[fx.spawned] != null) views[fx.spawned].Appear(CellWorld(fx.spawnCell));
            if (fx.shutterToggled) UpdateShutters(true);

            if (fx.unlockedLock >= 0) UnlockPadlocks(fx.unlockedLock, center);
            if (BlockExited != null) BlockExited(i, center, gd, fx.piercing);
            if (MoveCommitted != null) MoveCommitted();
        }

        private void EndDrag()
        {
            int idx = dragging;
            dragging = -1;
            HidePreview();
            if (idx < 0 || views == null || views[idx] == null) return;
            views[idx].SetLifted(false);
            if (!dragExited && board.Pos[idx] != startPos && MoveCommitted != null) MoveCommitted();
        }

        // ------------------------------------------------------------------ portals, padlocks (visual hooks)

        /// <summary>A 1x1 block stepped onto a pad: zap out of the entry pad and appear at the partner pad.</summary>
        private void WarpEffect(int blockIndex, Int2 entryPad, Int2 exitPad)
        {
            Vector3 a = CellWorld(entryPad), b = CellWorld(exitPad);
            if (FXManager.Instance != null)
            {
                FXManager.Instance.Burst(FxKind.Spark, a + new Vector3(0f, 0.4f, 0f), 14, new Color(0.4f, 0.9f, 1f), 4f, 1f);
                FXManager.Instance.Burst(FxKind.Spark, b + new Vector3(0f, 0.4f, 0f), 14, new Color(0.8f, 0.5f, 1f), 4f, 1f);
                FXManager.Instance.Haptic();
            }
            if (SoundSynth.Instance != null) SoundSynth.Instance.Play(Sfx.PortalZap);
            views[blockIndex].TeleportTo(b);
        }

        /// <summary>Called when a brass key block leaves: opens the padlocked gates with that lock id.</summary>
        private void UnlockPadlocks(int lockId, Vector3 keyFrom)
        {
            for (int g = 0; g < gateViews.Count; g++)
            {
                GateView gv = gateViews[g];
                if (!gv.spec.isPadlocked || gv.spec.lockId != lockId) continue;
                StartCoroutine(KeyFlight(gv, keyFrom));
            }
        }

        private IEnumerator KeyFlight(GateView gv, Vector3 from)
        {
            GameObject key = Prims.Make("FlyingKey", root, Prims.Sphere, GameMaterials.Brass, from + new Vector3(0f, 0.8f, 0f), Vector3.one * 0.3f);
            Prims.Make("Shaft", key.transform, Prims.Cylinder, GameMaterials.Brass, new Vector3(0f, -1.6f, 0f), new Vector3(0.35f, 1.4f, 0.35f));
            Vector3 to = gv.center + new Vector3(0f, 0.5f, 0f);
            Vector3 start = key.transform.position;
            float t = 0f;
            while (t < 1f)
            {
                if (key == null) yield break;
                t += Time.deltaTime / 0.7f;
                float e = Mathf.Clamp01(t);
                Vector3 p = Vector3.Lerp(start, to, e);
                p.y += Mathf.Sin(e * Mathf.PI) * 3.2f; // 3D arc
                key.transform.position = p;
                key.transform.Rotate(0f, 540f * Time.deltaTime, 0f, Space.World);
                yield return null;
            }
            if (key != null) Destroy(key);
            if (FXManager.Instance != null) FXManager.Instance.GoldSparks(to, 28);
            if (SoundSynth.Instance != null) SoundSynth.Instance.Play(Sfx.PadlockSnap);
            if (gv.lockVisual != null)
            {
                // the padlock snaps open and the chains drop with gravity
                for (int c = 0; c < gv.lockVisual.transform.childCount; c++)
                {
                    Transform ch = gv.lockVisual.transform.GetChild(c);
                    Rigidbody rb = ch.gameObject.AddComponent<Rigidbody>();
                    rb.mass = 0.2f;
                    ch.gameObject.AddComponent<BoxCollider>();
                    rb.AddForce(new Vector3(UnityEngine.Random.Range(-1f, 1f), 2f, UnityEngine.Random.Range(-1f, 0.5f)), ForceMode.Impulse);
                }
                gv.lockVisual.transform.DetachChildren();
                Destroy(gv.lockVisual, 0.1f);
                gv.lockVisual = null;
            }
            if (Message != null) Message("Padlock unlocked!");
        }

        // ------------------------------------------------------------------ trajectory preview

        private void UpdatePreview()
        {
            if (dragging < 0) { HidePreview(); return; }
            BlockInfo b = board.Blocks[dragging];
            Vector3 start = views[dragging].CentroidWorld;
            start.y = TrayTop + 0.04f;
            for (int d = 0; d < 4; d++)
            {
                Int2 dir = Dirs.Of(Dirs.All[d]);
                Int2 o = board.Pos[dragging];
                int steps = 0;
                bool exit = false;
                while (true)
                {
                    StepResult r = board.Probe(dragging, o, dir);
                    if (r == StepResult.Blocked) break;
                    if (r == StepResult.Exit) { exit = true; steps++; break; }
                    o = o + dir;
                    steps++;
                }
                LineRenderer lr = preview[d];
                if (steps == 0) { lr.enabled = false; continue; }
                Vector3 end = start + new Vector3(dir.x, 0f, dir.y) * (steps * cs);
                lr.enabled = true;
                lr.SetPosition(0, start);
                lr.SetPosition(1, end);
                Color c = exit ? GameMaterials.ColorOf(b.color) : new Color(0.8f, 0.85f, 0.95f);
                lr.sharedMaterial = GameMaterials.UnlitGlow(c, exit ? 2.2f : 0.7f);
                lr.startWidth = cs * (exit ? 0.16f : 0.07f);
                lr.endWidth = lr.startWidth;
            }
        }

        private void HidePreview()
        {
            for (int i = 0; i < preview.Length; i++) if (preview[i] != null) preview[i].enabled = false;
        }
    }

    /// <summary>Small helper so slab building can be chained inline.</summary>
    public static class MeshBuilderExt
    {
        public static MeshBuilder AddSlabReturn(this MeshBuilder b, float cx, float cz, float w, float d, float height, float radius, float bevel, float y0, int seg)
        {
            b.AddSlab(cx, cz, w, d, height, radius, bevel, y0, seg);
            return b;
        }
    }
}
