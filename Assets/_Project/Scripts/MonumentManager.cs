using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorGateBlast
{
    /// <summary>
    /// The destructible 3D monument ("ASMR disassembly" target): Retro Alarm Clock (levels 1-10) and the multi-stage
    /// Retro Cassette Player (levels 11-20). Every part is a kinematic Rigidbody with HP and a colour affinity; at 0 HP
    /// it is released with an impact force and falls under gravity, shedding screws, springs and fragments that bounce
    /// off the conveyor. Destroying a TNT blasts neighbouring parts (hit-stop + shake + explosion force).
    /// The monument stands on a turntable and can be rotated 360 degrees by dragging in the upper part of the screen.
    /// </summary>
    public sealed class MonumentManager : MonoBehaviour
    {
        private sealed class PartView
        {
            public GameObject go;
            public Rigidbody rb;
            public TextMesh label;
            public bool broken;
            public Transform spin;      // optional child that rotates (gear / reel)
            public float spinSpeed;
            public Transform hand;      // alarm-clock minute hand
            public Vector3 aim;         // local point turrets aim at (parts built around the monument origin)
        }

        public MonumentModel Model { get; private set; }
        public int InFlight { get; private set; }
        public int AliveCount { get { return Model == null ? 0 : Model.AliveCount; } }
        public MonumentKind Kind { get; private set; }

        /// <summary>Raised for each destroyed part (index) after its pellet landed.</summary>
        public event Action<int> PartDestroyed;
        public event Action<int, int> PartDamaged; // (part, hp left)
        public event Action Detonated;

        private LevelDefinition def;
        private Transform pivot;
        private Transform root;
        private Transform debrisRoot;
        private PartView[] views;
        private Vector3 anchor;
        private float handTimer;
        private float labelTimer;
        private float firingUntil;
        private int debrisAlive;
        private float rotVelocity;
        private bool rotating;
        private float lastPointerX;
        private Camera cam;

        public void SetCamera(Camera c) { cam = c; }

        public Vector3 PartWorld(int i)
        {
            if (views == null || i < 0 || i >= views.Length || views[i].go == null) return anchor;
            return views[i].go.transform.TransformPoint(views[i].aim);
        }

        public int FindTarget(int color) { return Model == null ? -1 : Model.FindTarget(color); }
        public int AliveOfColor(int color) { return Model == null ? 0 : Model.AliveOfColor(color); }

        public void Clear()
        {
            StopAllCoroutines();
            InFlight = 0;
            debrisAlive = 0;
            if (root != null) Destroy(root.gameObject);
            root = null;
            views = null;
            Model = null;
        }

        // ================================================================== construction

        public void Build(LevelDefinition level, Vector3 anchorWorld)
        {
            Clear();
            def = level;
            Kind = level.monument;
            anchor = anchorWorld;
            Model = new MonumentModel(level);

            root = new GameObject("MonumentRoot").transform;
            root.SetParent(transform, false);
            debrisRoot = new GameObject("Debris").transform;
            debrisRoot.SetParent(root, false);

            // brass turntable the monument rotates on
            GameObject turntable = Prims.Make("Turntable", root, Prims.Cylinder, GameMaterials.Brass, anchor + new Vector3(0f, 0.12f, 0f), new Vector3(4.4f, 0.12f, 4.4f));
            Prims.Make("TurntableRim", turntable.transform, Prims.Cylinder, GameMaterials.Steel, new Vector3(0f, 0.55f, 0f), new Vector3(0.88f, 0.4f, 0.88f));
            BoxCollider tc = turntable.AddComponent<BoxCollider>();
            tc.size = new Vector3(1f, 2f, 1f);

            // the table surface debris falls onto
            GameObject ground = new GameObject("TableCollider");
            ground.transform.SetParent(root, false);
            ground.transform.position = new Vector3(0f, -0.5f, 0f);
            ground.AddComponent<BoxCollider>().size = new Vector3(60f, 1f, 60f);

            pivot = new GameObject("Pivot").transform;
            pivot.SetParent(root, false);
            pivot.position = anchor + new Vector3(0f, 0.24f, 0f);

            views = new PartView[Model.Parts.Length];
            for (int i = 0; i < views.Length; i++)
            {
                views[i] = new PartView();
                BuildPart(i);
            }
            BuildStaticDecor();
        }

        private PartView NewPart(int i, string name, Vector3 localPos)
        {
            PartView pv = views[i];
            pv.go = new GameObject(name);
            pv.go.transform.SetParent(pivot, false);
            pv.go.transform.localPosition = localPos;
            pv.rb = pv.go.AddComponent<Rigidbody>();
            pv.rb.isKinematic = true;
            pv.rb.mass = 1f;
            return pv;
        }

        private void Marker(PartView pv, BlockColor c, Vector3 labelPos)
        {
            // glowing affinity ring + HP badge
            Prims.Make("Affinity", pv.go.transform, Prims.Sphere, GameMaterials.Glow(c), labelPos + new Vector3(0f, -0.28f, 0f), Vector3.one * 0.16f);
            pv.label = Prims.Label(pv.go.transform, "1", 0.45f, Color.white, labelPos);
        }

        private static void Box(GameObject go, Vector3 center, Vector3 size)
        {
            BoxCollider bc = go.AddComponent<BoxCollider>();
            bc.center = center;
            bc.size = size;
        }

        private static void Sphere(GameObject go, Vector3 center, float r)
        {
            SphereCollider sc = go.AddComponent<SphereCollider>();
            sc.center = center;
            sc.radius = r;
        }

        private void BuildPart(int i)
        {
            PartSpec spec = Model.Parts[i];
            float lx, ly;
            MonumentLayout.Position(Kind, spec.type, spec.slot, out lx, out ly);
            if (Kind == MonumentKind.AlarmClock) BuildClockPart(i, spec, lx, ly);
            else BuildCassettePart(i, spec, lx, ly);
            if (views[i].label != null) views[i].label.text = Model.Hp[i].ToString();
        }

        // ------------------------------------------------------------------ alarm clock

        private void BuildClockPart(int i, PartSpec spec, float lx, float ly)
        {
            Material mat = GameMaterials.ForBlock(spec.color);
            switch (spec.type)
            {
                case PartType.Casing:
                {
                    PartView pv = NewPart(i, "ClockCasing", new Vector3(lx, ly, 0f));
                    Transform t = pv.go.transform;
                    Prims.Make("Body", t, Prims.Cylinder, mat, Vector3.zero, new Vector3(2.6f, 0.5f, 2.6f), new Vector3(90f, 0f, 0f));
                    Prims.Make("Bezel", t, Prims.Cylinder, GameMaterials.Brass, new Vector3(0f, 0f, -0.5f), new Vector3(2.72f, 0.07f, 2.72f), new Vector3(90f, 0f, 0f));
                    Prims.Make("Dial", t, Prims.Cylinder, GameMaterials.Cream, new Vector3(0f, 0f, -0.575f), new Vector3(2.3f, 0.02f, 2.3f), new Vector3(90f, 0f, 0f));
                    for (int k = 0; k < 12; k++)
                    {
                        float a = k * 30f * Mathf.Deg2Rad;
                        Prims.Make("Tick" + k, t, Prims.Cube, GameMaterials.Charcoal, new Vector3(Mathf.Sin(a) * 0.98f, Mathf.Cos(a) * 0.98f, -0.6f),
                            new Vector3(k % 3 == 0 ? 0.1f : 0.05f, k % 3 == 0 ? 0.24f : 0.14f, 0.02f), new Vector3(0f, 0f, -k * 30f));
                    }
                    Prims.Make("FootL", t, Prims.Sphere, GameMaterials.Brass, new Vector3(-0.85f, -1.3f, -0.1f), Vector3.one * 0.38f);
                    Prims.Make("FootR", t, Prims.Sphere, GameMaterials.Brass, new Vector3(0.85f, -1.3f, -0.1f), Vector3.one * 0.38f);
                    Marker(pv, spec.color, new Vector3(0f, -0.55f, -0.8f));
                    Sphere(pv.go, Vector3.zero, 1.3f);
                    break;
                }
                case PartType.Bell:
                {
                    PartView pv = NewPart(i, "Bell" + spec.slot, new Vector3(lx, ly, 0f));
                    pv.go.transform.localEulerAngles = new Vector3(0f, 0f, spec.slot == 0 ? 20f : -20f);
                    Transform t = pv.go.transform;
                    Prims.Make("Dome", t, Prims.Sphere, mat, Vector3.zero, new Vector3(1.0f, 0.62f, 1.0f));
                    Prims.Make("Knob", t, Prims.Sphere, GameMaterials.Brass, new Vector3(0f, 0.34f, 0f), Vector3.one * 0.18f);
                    Prims.Make("Stem", t, Prims.Cylinder, GameMaterials.Steel, new Vector3(0f, -0.4f, 0f), new Vector3(0.07f, 0.2f, 0.07f));
                    Marker(pv, spec.color, new Vector3(0f, 0.75f, -0.2f));
                    Sphere(pv.go, Vector3.zero, 0.5f);
                    break;
                }
                case PartType.Hand:
                {
                    PartView pv = NewPart(i, "ClockHands", new Vector3(lx, ly, -0.62f));
                    Transform t = pv.go.transform;
                    GameObject hour = Prims.Make("Hour", t, Prims.Cube, mat, new Vector3(0.12f, 0.2f, 0f), new Vector3(0.12f, 0.62f, 0.04f), new Vector3(0f, 0f, -50f));
                    GameObject minute = Prims.Make("Minute", t, Prims.Cube, mat, new Vector3(0f, 0.42f, -0.03f), new Vector3(0.08f, 0.95f, 0.03f));
                    Prims.Make("Hub", t, Prims.Sphere, GameMaterials.Brass, new Vector3(0f, 0f, -0.04f), Vector3.one * 0.18f);
                    pv.hand = minute.transform;
                    Marker(pv, spec.color, new Vector3(0f, -0.95f, -0.3f));
                    Box(pv.go, new Vector3(0f, 0.3f, 0f), new Vector3(0.45f, 1.1f, 0.2f));
                    break;
                }
                case PartType.Glass:
                {
                    PartView pv = NewPart(i, "GlassLens", new Vector3(lx, ly, -0.66f));
                    Prims.Make("Lens", pv.go.transform, Prims.Sphere, GameMaterials.Acrylic(spec.color), Vector3.zero, new Vector3(2.35f, 2.35f, 0.32f));
                    Marker(pv, spec.color, new Vector3(0.9f, 1.05f, -0.2f));
                    Sphere(pv.go, Vector3.zero, 1.1f);
                    break;
                }
                case PartType.Gear:
                {
                    PartView pv = NewPart(i, "Gear" + spec.slot, new Vector3(lx, ly, -0.12f));
                    BuildGear(pv, spec.color, 0.9f);
                    Marker(pv, spec.color, new Vector3(0f, 0.72f, -0.15f));
                    Sphere(pv.go, Vector3.zero, 0.45f);
                    break;
                }
                case PartType.Tnt:
                    BuildTnt(i, spec, lx, ly, -0.8f);
                    break;
                default:
                {
                    PartView pv = NewPart(i, "Part" + i, new Vector3(lx, ly, 0f));
                    Prims.Make("Block", pv.go.transform, Prims.Cube, mat, Vector3.zero, Vector3.one * 0.6f);
                    Marker(pv, spec.color, new Vector3(0f, 0.6f, 0f));
                    Box(pv.go, Vector3.zero, Vector3.one * 0.6f);
                    break;
                }
            }
        }

        private void BuildGear(PartView pv, BlockColor c, float scale)
        {
            GameObject spin = new GameObject("GearSpin");
            spin.transform.SetParent(pv.go.transform, false);
            spin.transform.localScale = Vector3.one * scale;
            Prims.Make("Disc", spin.transform, Prims.Cylinder, GameMaterials.Copper, Vector3.zero, new Vector3(0.78f, 0.05f, 0.78f), new Vector3(90f, 0f, 0f));
            for (int k = 0; k < 10; k++)
            {
                float a = k * 36f * Mathf.Deg2Rad;
                Prims.Make("Tooth" + k, spin.transform, Prims.Cube, GameMaterials.Copper, new Vector3(Mathf.Cos(a) * 0.42f, Mathf.Sin(a) * 0.42f, 0f),
                    new Vector3(0.16f, 0.12f, 0.08f), new Vector3(0f, 0f, k * 36f));
            }
            Prims.Make("Hub", spin.transform, Prims.Sphere, GameMaterials.Glow(c), new Vector3(0f, 0f, -0.04f), Vector3.one * 0.2f);
            pv.spin = spin.transform;
            pv.spinSpeed = 40f + UnityEngine.Random.value * 30f;
        }

        private void BuildTnt(int i, PartSpec spec, float lx, float ly, float z)
        {
            PartView pv = NewPart(i, "TNT" + spec.slot, new Vector3(lx, ly, z));
            Transform t = pv.go.transform;
            Prims.Make("Box", t, Prims.Cube, GameMaterials.TntRed, Vector3.zero, new Vector3(0.9f, 0.6f, 0.55f));
            Prims.Make("BandA", t, Prims.Cube, GameMaterials.Charcoal, new Vector3(-0.28f, 0f, 0f), new Vector3(0.07f, 0.62f, 0.57f));
            Prims.Make("BandB", t, Prims.Cube, GameMaterials.Charcoal, new Vector3(0.28f, 0f, 0f), new Vector3(0.07f, 0.62f, 0.57f));
            Prims.Make("Fuse", t, Prims.Cylinder, GameMaterials.Rubber, new Vector3(0f, 0.42f, 0f), new Vector3(0.05f, 0.14f, 0.05f), new Vector3(0f, 0f, 20f));
            Prims.Make("Spark", t, Prims.Sphere, GameMaterials.Glow(BlockColor.Yellow), new Vector3(0.05f, 0.58f, 0f), Vector3.one * 0.1f);
            Prims.Label(t, "TNT", 0.26f, Color.white, new Vector3(0f, 0f, -0.3f));
            Marker(pv, spec.color, new Vector3(0f, 0.7f, -0.2f));
            Box(pv.go, Vector3.zero, new Vector3(0.9f, 0.6f, 0.55f));
        }

        private void BuildStaticDecor()
        {
            if (Kind == MonumentKind.AlarmClock)
            {
                // bell hammer + back plate so the clock is never a hollow shell once parts fall off
                Prims.Make("BackPlate", pivot, Prims.Cylinder, GameMaterials.Gunmetal, new Vector3(0f, 1.5f, 0.52f), new Vector3(2.5f, 0.04f, 2.5f), new Vector3(90f, 0f, 0f));
                Prims.Make("HammerStem", pivot, Prims.Cylinder, GameMaterials.Steel, new Vector3(0f, 2.62f, 0f), new Vector3(0.05f, 0.35f, 0.05f));
                Prims.Make("HammerBall", pivot, Prims.Sphere, GameMaterials.Steel, new Vector3(0f, 2.98f, 0f), Vector3.one * 0.16f);
            }
            else
            {
                Prims.Make("DeckBack", pivot, Prims.Cube, GameMaterials.Charcoal, new Vector3(0f, 1.3f, 0.5f), new Vector3(3.5f, 1.9f, 0.08f));
                Prims.Make("SpeakerGrille", pivot, Prims.Cube, GameMaterials.Rubber, new Vector3(1.43f, 1.2f, -0.62f), new Vector3(0.6f, 0.9f, 0.02f));
            }
        }

        // ------------------------------------------------------------------ cassette player

        private void BuildCassettePart(int i, PartSpec spec, float lx, float ly)
        {
            Material mat = GameMaterials.ForBlock(spec.color);
            switch (spec.type)
            {
                case PartType.Casing:
                {
                    bool silver = spec.slot == 0;
                    PartView pv = NewPart(i, silver ? "CasingSilver" : "CasingCharcoal", Vector3.zero);
                    Transform t = pv.go.transform;
                    if (silver)
                    {
                        // brushed-silver frame around the open cassette window
                        Prims.Make("Top", t, Prims.Cube, GameMaterials.Steel, new Vector3(0f, 2.0f, 0f), new Vector3(3.6f, 0.4f, 1.2f));
                        Prims.Make("Bottom", t, Prims.Cube, GameMaterials.Steel, new Vector3(0f, 0.62f, 0f), new Vector3(3.6f, 0.24f, 1.2f));
                        Prims.Make("Left", t, Prims.Cube, GameMaterials.Steel, new Vector3(-1.425f, 1.2f, 0f), new Vector3(0.75f, 1.4f, 1.2f));
                        Prims.Make("Right", t, Prims.Cube, GameMaterials.Steel, new Vector3(1.425f, 1.2f, 0f), new Vector3(0.75f, 1.4f, 1.2f));
                        Marker(pv, spec.color, new Vector3(-1.4f, 1.95f, -0.75f));
                        pv.aim = new Vector3(0f, 1.3f, -0.4f);
                        Box(pv.go, new Vector3(0f, 1.3f, 0f), new Vector3(3.6f, 1.8f, 1.2f));
                    }
                    else
                    {
                        Prims.Make("Base", t, Prims.Cube, GameMaterials.Charcoal, new Vector3(0f, 0.25f, 0f), new Vector3(3.7f, 0.5f, 1.3f));
                        Prims.Make("FootL", t, Prims.Cube, GameMaterials.Rubber, new Vector3(-1.6f, 0.02f, -0.45f), new Vector3(0.4f, 0.08f, 0.3f));
                        Prims.Make("FootR", t, Prims.Cube, GameMaterials.Rubber, new Vector3(1.6f, 0.02f, -0.45f), new Vector3(0.4f, 0.08f, 0.3f));
                        Marker(pv, spec.color, new Vector3(1.4f, 0.5f, -0.75f));
                        pv.aim = new Vector3(0f, 0.3f, -0.4f);
                        Box(pv.go, new Vector3(0f, 0.25f, 0f), new Vector3(3.7f, 0.5f, 1.3f));
                    }
                    break;
                }
                case PartType.Button:
                {
                    PartView pv = NewPart(i, "Button" + spec.slot, new Vector3(lx, 2.28f, -0.2f));
                    Prims.Make("Key", pv.go.transform, Prims.Cube, mat, Vector3.zero, new Vector3(0.55f, 0.2f, 0.4f));
                    Prims.Label(pv.go.transform, spec.slot == 0 ? ">" : "^", 0.2f, Color.white, new Vector3(0f, 0.12f, -0.01f));
                    Marker(pv, spec.color, new Vector3(0f, 0.45f, 0f));
                    Box(pv.go, Vector3.zero, new Vector3(0.55f, 0.2f, 0.4f));
                    break;
                }
                case PartType.Door:
                {
                    PartView pv = NewPart(i, "AcrylicDoor", new Vector3(0f, 1.25f, -0.74f));
                    Transform t = pv.go.transform;
                    Prims.Make("Pane", t, Prims.Cube, GameMaterials.Acrylic(spec.color), Vector3.zero, new Vector3(2.15f, 1.25f, 0.06f));
                    Prims.Make("HingeL", t, Prims.Cylinder, GameMaterials.Brass, new Vector3(-1.1f, 0.5f, 0f), new Vector3(0.08f, 0.1f, 0.08f));
                    Prims.Make("HingeR", t, Prims.Cylinder, GameMaterials.Brass, new Vector3(-1.1f, -0.5f, 0f), new Vector3(0.08f, 0.1f, 0.08f));
                    Marker(pv, spec.color, new Vector3(0.75f, 0.5f, -0.2f));
                    Box(pv.go, Vector3.zero, new Vector3(2.15f, 1.25f, 0.12f));
                    break;
                }
                case PartType.Cassette:
                {
                    PartView pv = NewPart(i, "CassetteTape", new Vector3(0f, 1.25f, -0.46f));
                    Transform t = pv.go.transform;
                    Prims.Make("Shell", t, Prims.Cube, mat, Vector3.zero, new Vector3(1.95f, 1.15f, 0.13f));
                    Prims.Make("Label", t, Prims.Cube, GameMaterials.Cream, new Vector3(0f, 0.3f, -0.075f), new Vector3(1.4f, 0.38f, 0.01f));
                    Prims.Make("Window", t, Prims.Cube, GameMaterials.Rubber, new Vector3(0f, -0.15f, -0.07f), new Vector3(1.2f, 0.35f, 0.02f));
                    GameObject reels = new GameObject("Reels");
                    reels.transform.SetParent(t, false);
                    Prims.Make("ReelL", reels.transform, Prims.Cylinder, GameMaterials.Cream, new Vector3(-0.42f, -0.15f, -0.09f), new Vector3(0.34f, 0.02f, 0.34f), new Vector3(90f, 0f, 0f));
                    Prims.Make("ReelR", reels.transform, Prims.Cylinder, GameMaterials.Cream, new Vector3(0.42f, -0.15f, -0.09f), new Vector3(0.34f, 0.02f, 0.34f), new Vector3(90f, 0f, 0f));
                    Prims.Make("SpokeL", reels.transform, Prims.Cube, GameMaterials.Charcoal, new Vector3(-0.42f, -0.15f, -0.105f), new Vector3(0.28f, 0.04f, 0.01f));
                    Prims.Make("SpokeR", reels.transform, Prims.Cube, GameMaterials.Charcoal, new Vector3(0.42f, -0.15f, -0.105f), new Vector3(0.28f, 0.04f, 0.01f));
                    pv.spin = reels.transform; // the whole reel group wobbles; individual spokes rotate in Update
                    pv.spinSpeed = 120f;
                    Marker(pv, spec.color, new Vector3(0f, 0.75f, -0.2f));
                    Box(pv.go, Vector3.zero, new Vector3(1.95f, 1.15f, 0.2f));
                    break;
                }
                case PartType.Gear:
                {
                    PartView pv = NewPart(i, "Gear" + spec.slot, new Vector3(lx, ly, -0.22f));
                    BuildGear(pv, spec.color, 0.85f);
                    Marker(pv, spec.color, new Vector3(0f, 0.62f, -0.15f));
                    Sphere(pv.go, Vector3.zero, 0.42f);
                    break;
                }
                case PartType.Pcb:
                {
                    PartView pv = NewPart(i, "CircuitBoard", new Vector3(0f, 0.95f, -0.12f));
                    Transform t = pv.go.transform;
                    Prims.Make("Board", t, Prims.Cube, GameMaterials.PcbGreen, Vector3.zero, new Vector3(2.0f, 0.9f, 0.04f));
                    for (int k = 0; k < 5; k++)
                        Prims.Make("Chip" + k, t, Prims.Cube, GameMaterials.Charcoal, new Vector3(-0.7f + k * 0.35f, 0.15f * (k % 2 == 0 ? 1 : -1), -0.035f), new Vector3(0.22f, 0.16f, 0.04f));
                    for (int k = 0; k < 4; k++)
                        Prims.Make("Trace" + k, t, Prims.Cube, GameMaterials.Brass, new Vector3(0f, -0.3f + k * 0.17f, -0.026f), new Vector3(1.7f, 0.018f, 0.01f));
                    Marker(pv, spec.color, new Vector3(0.7f, 0.55f, -0.15f));
                    Box(pv.go, Vector3.zero, new Vector3(2.0f, 0.9f, 0.1f));
                    break;
                }
                case PartType.Tnt:
                {
                    // TNT capacitor: red can with brass cap and a glowing band
                    PartView pv = NewPart(i, "TNTCapacitor" + spec.slot, new Vector3(lx, ly, -0.35f));
                    Transform t = pv.go.transform;
                    Prims.Make("Can", t, Prims.Cylinder, GameMaterials.TntRed, Vector3.zero, new Vector3(0.62f, 0.42f, 0.62f));
                    Prims.Make("Cap", t, Prims.Cylinder, GameMaterials.Brass, new Vector3(0f, 0.43f, 0f), new Vector3(0.66f, 0.05f, 0.66f));
                    Prims.Make("Band", t, Prims.Cylinder, GameMaterials.Glow(BlockColor.Yellow), new Vector3(0f, 0.05f, 0f), new Vector3(0.64f, 0.05f, 0.64f));
                    Prims.Label(t, "TNT", 0.2f, Color.white, new Vector3(0f, -0.1f, -0.34f));
                    Marker(pv, spec.color, new Vector3(0f, 0.85f, -0.1f));
                    Sphere(pv.go, Vector3.zero, 0.42f);
                    break;
                }
                default:
                {
                    PartView pv = NewPart(i, "Part" + i, new Vector3(lx, ly, 0f));
                    Prims.Make("Block", pv.go.transform, Prims.Cube, mat, Vector3.zero, Vector3.one * 0.6f);
                    Marker(pv, spec.color, new Vector3(0f, 0.6f, 0f));
                    Box(pv.go, Vector3.zero, Vector3.one * 0.6f);
                    break;
                }
            }
        }

        // ================================================================== combat

        /// <summary>
        /// A turret fires at <paramref name="part"/>. The hit is applied to the model immediately (so every observer sees
        /// consistent HP); the pellet / laser animation and the physical break-off follow when it lands.
        /// </summary>
        public bool Shoot(int part, Vector3 from, BlockColor color, bool mega, bool pierce)
        {
            if (Model == null || part < 0 || !Model.IsTargetable(part) || (int)Model.Parts[part].color != (int)color) return false;

            List<int> destroyed = new List<int>();
            List<int> detonated = new List<int>();
            Model.Hit(part, destroyed, detonated);

            // Clockwork piercing shot: also damages one part hidden directly behind the target.
            int pierced = -1;
            if (pierce)
            {
                for (int j = 0; j < Model.Parts.Length; j++)
                {
                    if (Model.Hp[j] > 0 && ((Model.Parts[j].hiddenByMask >> part) & 1) != 0) { pierced = j; break; }
                }
                if (pierced >= 0) Model.Hit(pierced, destroyed, detonated);
            }

            firingUntil = Time.time + 0.5f;
            StartCoroutine(Pellet(from, part, pierced, color, mega, destroyed, detonated));
            return true;
        }

        private IEnumerator Pellet(Vector3 from, int part, int pierced, BlockColor color, bool mega, List<int> destroyed, List<int> detonated)
        {
            InFlight++;
            Vector3 to = PartWorld(part);
            GameObject shot = null;
            if (!mega)
            {
                shot = Prims.Make("Pellet", root, Prims.Sphere, GameMaterials.UnlitGlow(GameMaterials.ColorOf(color), 3f), from, Vector3.one * 0.2f);
                float dist = Vector3.Distance(from, to);
                float dur = Mathf.Clamp(dist / 26f, 0.05f, 0.22f);
                float t = 0f;
                while (t < 1f)
                {
                    t += Time.deltaTime / dur;
                    shot.transform.position = Vector3.Lerp(from, to, Mathf.Clamp01(t));
                    yield return null;
                }
                Destroy(shot);
            }
            else
            {
                yield return null;
            }

            to = PartWorld(part);
            if (FXManager.Instance != null)
            {
                if (destroyed.Count == 0) FXManager.Instance.Sparks(to, 6);
                FXManager.Instance.Shake(0.04f, 0.1f);
            }
            if (SoundSynth.Instance != null && destroyed.Count == 0) SoundSynth.Instance.Play(Sfx.DebrisClink, UnityEngine.Random.Range(0.9f, 1.2f), 0.45f);

            if (views != null)
            {
                if (part < views.Length) RefreshLabel(part);
                if (pierced >= 0 && pierced < views.Length) RefreshLabel(pierced);
            }
            if (PartDamaged != null && Model != null) PartDamaged(part, Model.Hp[part]);

            if (destroyed.Count > 0) yield return StartCoroutine(BreakSequence(destroyed, detonated, to));
            InFlight--;
        }

        private IEnumerator BreakSequence(List<int> destroyed, List<int> detonated, Vector3 impactPoint)
        {
            for (int k = 0; k < destroyed.Count; k++)
            {
                int idx = destroyed[k];
                bool isTnt = detonated.Contains(idx);
                BreakPart(idx, isTnt);
                if (PartDestroyed != null) PartDestroyed(idx);
                if (isTnt) Explode(PartWorld(idx));
                if (destroyed.Count > 1) yield return new WaitForSeconds(0.07f);
            }
        }

        private void RefreshLabel(int i)
        {
            PartView pv = views[i];
            if (pv.label == null || Model == null) return;
            pv.label.text = Model.Hp[i].ToString();
        }

        private void BreakPart(int i, bool explosive)
        {
            PartView pv = views[i];
            if (pv.broken || pv.go == null) return;
            pv.broken = true;
            PartSpec spec = Model.Parts[i];
            Vector3 pos = pv.go.transform.position;

            if (pv.label != null) Destroy(pv.label.gameObject);
            pv.go.transform.SetParent(debrisRoot, true);
            pv.rb.isKinematic = false;
            pv.rb.mass = spec.type == PartType.Casing ? 3f : 1f;
            Vector3 push = (pos - (anchor + new Vector3(0f, 1.5f, 0f)));
            push.z -= 0.6f; // pop towards the camera
            if (push.sqrMagnitude < 0.01f) push = new Vector3(0f, 0.5f, -1f);
            push = push.normalized;
            float impulse = explosive ? 7f : 4.5f;
            pv.rb.AddForce((push + Vector3.up * 0.7f) * impulse * pv.rb.mass, ForceMode.Impulse);
            pv.rb.AddTorque(UnityEngine.Random.insideUnitSphere * 5f * pv.rb.mass, ForceMode.Impulse);

            FXManager fx = FXManager.Instance;
            SoundSynth snd = SoundSynth.Instance;
            bool glass = spec.type == PartType.Glass || spec.type == PartType.Door;
            if (fx != null)
            {
                if (glass) fx.GlassBreak(pos);
                else fx.MetalBreak(pos, GameMaterials.ColorOf(spec.color));
                fx.Shake(0.14f, 0.2f);
                fx.Haptic();
            }
            if (snd != null) snd.Play(glass ? Sfx.GlassSnap : Sfx.MonumentBreak, UnityEngine.Random.Range(0.92f, 1.08f), 0.9f);
            if (spec.type == PartType.Door && snd != null) snd.Play(Sfx.PlasticPop);

            SpawnDebris(spec.type, pos, spec.color, explosive);
            StartCoroutine(RemoveLater(pv.go, 4.2f));
        }

        private void SpawnDebris(PartType type, Vector3 pos, BlockColor color, bool explosive)
        {
            float rate = def != null ? def.debrisSpawnRate : 1f;
            int screws = Mathf.RoundToInt((type == PartType.Casing ? 5 : 2) * rate);
            int springs = Mathf.RoundToInt((type == PartType.Gear || type == PartType.Cassette || type == PartType.Casing ? 2 : 1) * rate);
            int frags = Mathf.RoundToInt((type == PartType.Casing ? 8 : 4) * rate);
            for (int i = 0; i < screws; i++) Debris(Prims.Cylinder, GameMaterials.Brass, pos, new Vector3(0.05f, 0.07f, 0.05f), explosive);
            for (int i = 0; i < springs; i++) Debris(Prims.Capsule, GameMaterials.Steel, pos, new Vector3(0.07f, 0.13f, 0.07f), explosive);
            for (int i = 0; i < frags; i++) Debris(Prims.Cube, GameMaterials.ForBlock(color), pos, new Vector3(0.12f, 0.06f, 0.1f), explosive);
        }

        private void Debris(Mesh mesh, Material mat, Vector3 pos, Vector3 size, bool explosive)
        {
            if (debrisAlive > 90) return;
            GameObject go = Prims.Make("Debris", debrisRoot, mesh, mat, pos + UnityEngine.Random.insideUnitSphere * 0.3f, size);
            go.transform.rotation = UnityEngine.Random.rotation;
            BoxCollider bc = go.AddComponent<BoxCollider>();
            bc.size = Vector3.one;
            Rigidbody rb = go.AddComponent<Rigidbody>();
            rb.mass = 0.05f;
            Vector3 v = (UnityEngine.Random.insideUnitSphere + Vector3.up * 1.2f + new Vector3(0f, 0f, -0.7f)) * (explosive ? 5.5f : 3f);
            rb.AddForce(v, ForceMode.VelocityChange);
            rb.AddTorque(UnityEngine.Random.insideUnitSphere * 6f, ForceMode.VelocityChange);
            debrisAlive++;
            StartCoroutine(DebrisLife(go, 2.2f + UnityEngine.Random.value * 1.6f));
        }

        private IEnumerator DebrisLife(GameObject go, float life)
        {
            float t = 0f;
            bool clinked = false;
            while (go != null && t < life)
            {
                t += Time.deltaTime;
                if (!clinked && t > 0.45f && SoundSynth.Instance != null && UnityEngine.Random.value < 0.5f)
                {
                    clinked = true;
                    SoundSynth.Instance.Play(Sfx.DebrisClink, UnityEngine.Random.Range(0.8f, 1.5f), 0.25f);
                }
                yield return null;
            }
            if (go != null)
            {
                float s = 0f;
                Vector3 sc = go.transform.localScale;
                while (go != null && s < 1f)
                {
                    s += Time.deltaTime / 0.3f;
                    go.transform.localScale = sc * (1f - s);
                    yield return null;
                }
                if (go != null) Destroy(go);
            }
            debrisAlive = Mathf.Max(0, debrisAlive - 1);
        }

        private IEnumerator RemoveLater(GameObject go, float t)
        {
            yield return new WaitForSeconds(t);
            if (go != null) Destroy(go);
        }

        /// <summary>Massive TNT blast: 0.06 s hit-stop, slow-motion tail, camera shake and a physical shockwave.</summary>
        private void Explode(Vector3 pos)
        {
            FXManager fx = FXManager.Instance;
            if (fx != null)
            {
                fx.Explosion(pos);
                fx.HitStop(0.06f, 0.3f, 0.55f);
                fx.Shake(0.55f, 0.6f);
                fx.Haptic();
            }
            if (SoundSynth.Instance != null) SoundSynth.Instance.Play(Sfx.Tnt, 1f, 1f);
            Collider[] hits = Physics.OverlapSphere(pos, 5.5f);
            for (int i = 0; i < hits.Length; i++)
            {
                Rigidbody rb = hits[i].attachedRigidbody;
                if (rb != null && !rb.isKinematic) rb.AddExplosionForce(14f * Mathf.Max(0.3f, rb.mass), pos, 5.5f, 0.8f, ForceMode.Impulse);
            }
            if (Detonated != null) Detonated();
        }

        // ================================================================== animation + rotation

        private void Update()
        {
            if (views == null || Model == null) return;
            float dt = Time.deltaTime;

            // 360-degree rotation: drag horizontally in the upper part of the screen
            HandleRotation(dt);

            handTimer += dt;
            bool tick = false;
            if (handTimer >= 1f) { handTimer -= 1f; tick = true; }
            labelTimer += dt;
            bool refreshLabels = labelTimer > 0.15f;
            if (refreshLabels) labelTimer = 0f;
            bool busy = Time.time < firingUntil;

            for (int i = 0; i < views.Length; i++)
            {
                PartView pv = views[i];
                if (pv.go == null) continue;
                if (pv.hand != null && !pv.broken && tick)
                {
                    pv.hand.parent.localEulerAngles = new Vector3(0f, 0f, pv.hand.parent.localEulerAngles.z - 6f);
                    if (SoundSynth.Instance != null && i == 0) SoundSynth.Instance.Play(Sfx.TickTock, 1f, 0.18f);
                }
                if (pv.spin != null && !pv.broken)
                {
                    float speed = pv.spinSpeed * (busy ? 3f : 1f);
                    if (pv.go.name == "CassetteTape")
                    {
                        for (int c = 0; c < pv.spin.childCount; c++)
                        {
                            Transform ch = pv.spin.GetChild(c);
                            if (ch.name.StartsWith("Spoke")) ch.Rotate(0f, 0f, speed * dt, Space.Self);
                        }
                    }
                    else pv.spin.Rotate(0f, 0f, speed * dt, Space.Self);
                }
                if (refreshLabels && pv.label != null && !pv.broken)
                {
                    bool visible = Model.IsTargetable(i);
                    if (pv.label.gameObject.activeSelf != visible) pv.label.gameObject.SetActive(visible);
                    if (visible && cam != null) pv.label.transform.rotation = cam.transform.rotation;
                }
                else if (pv.label != null && !pv.broken && cam != null)
                {
                    pv.label.transform.rotation = cam.transform.rotation;
                }
            }
        }

        private void HandleRotation(float dt)
        {
            if (cam == null || pivot == null) return;
            Vector2 p = InputHelper.PointerPosition;
            if (InputHelper.PressedThisFrame && !InputHelper.OverUi && p.y > Screen.height * 0.55f)
            {
                rotating = true;
                lastPointerX = p.x;
                rotVelocity = 0f;
            }
            if (rotating)
            {
                if (InputHelper.IsPressed)
                {
                    float d = p.x - lastPointerX;
                    lastPointerX = p.x;
                    float deg = -d * 0.35f;
                    pivot.Rotate(0f, deg, 0f, Space.World);
                    rotVelocity = deg / Mathf.Max(0.005f, dt);
                }
                else rotating = false;
            }
            else if (Mathf.Abs(rotVelocity) > 1f)
            {
                pivot.Rotate(0f, rotVelocity * dt, 0f, Space.World);
                rotVelocity = Mathf.Lerp(rotVelocity, 0f, 1f - Mathf.Exp(-4f * dt));
            }
        }

        /// <summary>Current rotation of the monument around Y in degrees (used to know whether the rear has been inspected).</summary>
        public float RotationY { get { return pivot == null ? 0f : pivot.eulerAngles.y; } }
    }
}
