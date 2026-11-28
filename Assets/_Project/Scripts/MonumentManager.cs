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
            public Transform[] screws = new Transform[3]; // up to three coupled brass screws pinning this part (null when none / removed)
            public Transform aimT;      // moving aim point (an orbiting planet): overrides aim
        }

        /// <summary>The monument is built at a modelling scale of about 3 units; this makes it fill the upper half of the screen.</summary>
        public const float MonumentScale = 1.25f;

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

        /// <summary>Highest point of the freshly built monument (used so the camera rig keeps its top inside the HUD-safe area).</summary>
        /// <summary>Top of the visible silhouette (centre of the turntable, highest point): used to keep the belt behind it clear.</summary>
        public Vector3 SilhouetteTop()
        {
            Vector3 t = TopWorldPoint();
            return new Vector3(anchor.x, t.y, anchor.z + 0.3f);
        }

        public Vector3 TopWorldPoint()
        {
            Vector3 p = anchor;
            if (pivot == null) return p;
            Renderer[] rs = pivot.GetComponentsInChildren<Renderer>();
            float maxY = 0f, maxZ = anchor.z;
            for (int i = 0; i < rs.Length; i++)
            {
                maxY = Mathf.Max(maxY, rs[i].bounds.max.y);
                maxZ = Mathf.Max(maxZ, rs[i].bounds.max.z);
            }
            p.y = maxY;
            p.z = maxZ;
            return p;
        }

        /// <summary>World position of a part, or of its screw when <paramref name="code"/> is a screw target.</summary>
        public Vector3 PartWorld(int code)
        {
            int i = MonumentModel.PartOf(code);
            if (views == null || i < 0 || i >= views.Length || views[i].go == null) return anchor;
            if (MonumentModel.IsScrewCode(code) && views[i].screws[MonumentModel.ScrewIndexOf(code)] != null) return views[i].screws[MonumentModel.ScrewIndexOf(code)].position;
            if (views[i].aimT != null) return views[i].aimT.position;
            return views[i].go.transform.TransformPoint(views[i].aim);
        }

        public int FindTarget(int color) { return Model == null ? -1 : Model.FindTarget(color); }
        public int FindDeflectedPlate(int color) { return Model == null ? -1 : Model.FindDeflectedPlate(color); }
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
            GameObject turntable = Prims.Make("Turntable", root, Prims.Cylinder, GameMaterials.Brass, anchor + new Vector3(0f, 0.1f, 0f), new Vector3(3.6f * MonumentScale, 0.1f, 3.6f * MonumentScale));
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
            pivot.localScale = Vector3.one * (Kind == MonumentKind.GrandfatherClock ? 0.92f : (Kind == MonumentKind.PipeOrgan ? 1.0f : MonumentScale));

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
            switch (Kind)
            {
                case MonumentKind.AlarmClock: BuildClockPart(i, spec, lx, ly); break;
                case MonumentKind.CassettePlayer: BuildCassettePart(i, spec, lx, ly); break;
                case MonumentKind.RotaryPhone: BuildPhonePart(i, spec, lx, ly); break;
                case MonumentKind.EspressoMachine: BuildEspressoPart(i, spec, lx, ly); break;
                case MonumentKind.TlrCamera: BuildCameraPart(i, spec, lx, ly); break;
                case MonumentKind.Typewriter: BuildTypewriterPart(i, spec, lx, ly); break;
                case MonumentKind.Gramophone: BuildGramophonePart(i, spec, lx, ly); break;
                case MonumentKind.Orrery: BuildOrreryPart(i, spec, lx, ly); break;
                case MonumentKind.Tourbillon: BuildTourbillonPart(i, spec, lx, ly); break;
                case MonumentKind.GrandfatherClock: BuildGrandfatherPart(i, spec, lx, ly); break;
                case MonumentKind.MusicBox: BuildMusicBoxPart(i, spec, lx, ly); break;
                case MonumentKind.PipeOrgan: BuildOrganPart(i, spec, lx, ly); break;
                default: BuildWatchPart(i, spec, lx, ly); break;
            }
            if (views[i].go != null && spec.hasScrew) AddScrew(i, spec);
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
            else if (Kind == MonumentKind.RotaryPhone)
            {
                BuildPhoneDecor();
            }
            else if (Kind == MonumentKind.PocketWatch)
            {
                BuildWatchDecor();
            }
            else if (Kind == MonumentKind.EspressoMachine)
            {
                BuildEspressoDecor();
            }
            else if (Kind == MonumentKind.TlrCamera)
            {
                BuildCameraDecor();
            }
            else if (Kind == MonumentKind.Typewriter)
            {
                BuildTypewriterDecor();
            }
            else if (Kind == MonumentKind.Gramophone)
            {
                BuildGramophoneDecor();
            }
            else if (Kind == MonumentKind.Orrery)
            {
                BuildOrreryDecor();
            }
            else if (Kind == MonumentKind.Tourbillon)
            {
                BuildTourbillonDecor();
            }
            else if (Kind == MonumentKind.GrandfatherClock)
            {
                BuildGrandfatherDecor();
            }
            else if (Kind == MonumentKind.MusicBox)
            {
                BuildMusicBoxDecor();
            }
            else if (Kind == MonumentKind.PipeOrgan)
            {
                BuildOrganDecor();
            }
            else
            {
                Prims.Make("DeckBack", pivot, Prims.Cube, GameMaterials.Charcoal, new Vector3(0f, 1.3f, 0.5f), new Vector3(3.5f, 1.9f, 0.08f));
                Prims.Make("SpeakerGrille", pivot, Prims.Cube, GameMaterials.Rubber, new Vector3(1.43f, 1.2f, -0.62f), new Vector3(0.6f, 0.9f, 0.02f));
            }
        }

        // ------------------------------------------------------------------ chapter 5: vintage espresso machine

        private void BuildEspressoDecor()
        {
            Prims.Make("BaseTray", pivot, Prims.Cube, GameMaterials.Brass, new Vector3(0f, 0.12f, -0.2f), new Vector3(3.3f, 0.24f, 1.7f));
            Prims.Make("BaseRim", pivot, Prims.Cube, GameMaterials.Steel, new Vector3(0f, 0.26f, -1.02f), new Vector3(3.3f, 0.08f, 0.1f));
            Prims.Make("BoilerBody", pivot, Prims.Cylinder, GameMaterials.Copper, new Vector3(0f, 1.15f, 0.1f), new Vector3(1.7f, 0.85f, 1.7f));
            Prims.Make("BoilerBand", pivot, Prims.Cylinder, GameMaterials.Brass, new Vector3(0f, 1.1f, 0.1f), new Vector3(1.78f, 0.06f, 1.78f));
            Prims.Make("Pipe", pivot, Prims.Cylinder, GameMaterials.Copper, new Vector3(0.95f, 0.9f, 0.3f), new Vector3(0.14f, 0.55f, 0.14f));
            Prims.Make("PipeElbow", pivot, Prims.Sphere, GameMaterials.Brass, new Vector3(0.95f, 1.5f, 0.3f), Vector3.one * 0.22f);
            for (int k = 0; k < 2; k++)
                Prims.Make("Foot" + k, pivot, Prims.Sphere, GameMaterials.Brass, new Vector3(k == 0 ? -1.4f : 1.4f, 0.12f, -0.9f), Vector3.one * 0.3f);
        }

        private void BuildEspressoPart(int i, PartSpec spec, float lx, float ly)
        {
            Material mat = GameMaterials.ForBlock(spec.color);
            switch (spec.type)
            {
                case PartType.Boiler:
                {
                    bool dome = spec.slot == 0;
                    PartView pv = NewPart(i, dome ? "CopperBoilerDome" : "BrassEagleFinial", new Vector3(lx, ly, 0.1f));
                    Transform t = pv.go.transform;
                    if (dome)
                    {
                        Prims.Make("Dome", t, Prims.Sphere, GameMaterials.Copper, Vector3.zero, new Vector3(1.85f, 1.15f, 1.85f));
                        Prims.Make("Tint", t, Prims.Sphere, GameMaterials.Glow(spec.color), new Vector3(0f, 0.05f, -0.86f), new Vector3(0.45f, 0.45f, 0.08f));
                        for (int k = 0; k < 8; k++)
                        {
                            float a = k * 45f * Mathf.Deg2Rad;
                            Prims.Make("Rivet" + k, t, Prims.Sphere, GameMaterials.Brass, new Vector3(Mathf.Sin(a) * 0.9f, -0.18f, Mathf.Cos(a) * 0.9f), Vector3.one * 0.09f);
                        }
                        Marker(pv, spec.color, new Vector3(0.55f, 0.55f, -0.7f));
                        Sphere(pv.go, Vector3.zero, 0.95f);
                        pv.aim = new Vector3(0f, 0f, -0.4f);
                    }
                    else
                    {
                        Prims.Make("Stem", t, Prims.Cylinder, GameMaterials.Brass, new Vector3(0f, -0.2f, 0f), new Vector3(0.12f, 0.2f, 0.12f));
                        Prims.Make("Body", t, Prims.Sphere, GameMaterials.Brass, new Vector3(0f, 0.08f, 0f), new Vector3(0.34f, 0.46f, 0.3f));
                        Prims.Make("WingL", t, Prims.Cube, GameMaterials.Brass, new Vector3(-0.3f, 0.12f, 0f), new Vector3(0.45f, 0.1f, 0.06f), new Vector3(0f, 0f, 25f));
                        Prims.Make("WingR", t, Prims.Cube, GameMaterials.Brass, new Vector3(0.3f, 0.12f, 0f), new Vector3(0.45f, 0.1f, 0.06f), new Vector3(0f, 0f, -25f));
                        Prims.Make("Head", t, Prims.Sphere, mat, new Vector3(0f, 0.38f, -0.02f), Vector3.one * 0.18f);
                        Marker(pv, spec.color, new Vector3(0.5f, 0.35f, -0.1f));
                        Sphere(pv.go, Vector3.zero, 0.4f);
                    }
                    break;
                }
                case PartType.Gauge:
                {
                    PartView pv = NewPart(i, "BrassPressureGauge", new Vector3(lx, ly, -0.85f));
                    Transform t = pv.go.transform;
                    Prims.Make("Ring", t, Prims.Cylinder, GameMaterials.Brass, Vector3.zero, new Vector3(0.62f, 0.09f, 0.62f), new Vector3(90f, 0f, 0f));
                    Prims.Make("Face", t, Prims.Cylinder, GameMaterials.Cream, new Vector3(0f, 0f, -0.05f), new Vector3(0.5f, 0.02f, 0.5f), new Vector3(90f, 0f, 0f));
                    GameObject needle = Prims.Make("Needle", t, Prims.Cube, GameMaterials.TntRed, new Vector3(0.05f, 0.08f, -0.08f), new Vector3(0.03f, 0.22f, 0.01f), new Vector3(0f, 0f, -30f));
                    pv.hand = needle.transform;
                    Marker(pv, spec.color, new Vector3(0f, 0.45f, -0.15f));
                    Sphere(pv.go, Vector3.zero, 0.34f);
                    break;
                }
                case PartType.Lever:
                {
                    PartView pv = NewPart(i, "SpringLever", new Vector3(lx, ly, 0.05f));
                    Transform t = pv.go.transform;
                    Prims.Make("Pivot", t, Prims.Sphere, GameMaterials.Brass, Vector3.zero, Vector3.one * 0.3f);
                    Prims.Make("Arm", t, Prims.Cube, GameMaterials.Steel, new Vector3(0.38f, 0.32f, 0f), new Vector3(0.12f, 0.95f, 0.1f), new Vector3(0f, 0f, -32f));
                    Prims.Make("Grip", t, Prims.Sphere, GameMaterials.Bakelite, new Vector3(0.7f, 0.74f, 0f), new Vector3(0.28f, 0.28f, 0.28f));
                    Prims.Make("Tint", t, Prims.Sphere, GameMaterials.Glow(spec.color), new Vector3(0.7f, 0.74f, -0.14f), Vector3.one * 0.1f);
                    Marker(pv, spec.color, new Vector3(0.35f, 1.1f, -0.1f));
                    Sphere(pv.go, new Vector3(0.3f, 0.35f, 0f), 0.5f);
                    pv.aim = new Vector3(0.3f, 0.3f, -0.2f);
                    break;
                }
                case PartType.GroupHead:
                {
                    PartView pv = NewPart(i, "ChromeGroupHead", new Vector3(lx, ly, -0.8f));
                    Transform t = pv.go.transform;
                    Prims.Make("Head", t, Prims.Cylinder, GameMaterials.Steel, Vector3.zero, new Vector3(1.0f, 0.34f, 1.0f));
                    Prims.Make("Collar", t, Prims.Cylinder, GameMaterials.Steel, new Vector3(0f, -0.38f, 0f), new Vector3(0.78f, 0.08f, 0.78f));
                    Prims.Make("Portafilter", t, Prims.Cylinder, GameMaterials.Bakelite, new Vector3(0f, -0.5f, -0.45f), new Vector3(0.14f, 0.55f, 0.14f), new Vector3(90f, 0f, 0f));
                    Prims.Make("Tint", t, Prims.Cylinder, GameMaterials.Glow(spec.color), new Vector3(0f, 0.36f, 0f), new Vector3(0.5f, 0.02f, 0.5f));
                    Marker(pv, spec.color, new Vector3(0.65f, 0.4f, -0.2f));
                    Box(pv.go, Vector3.zero, new Vector3(1.0f, 0.8f, 1.0f));
                    pv.aim = new Vector3(0f, 0f, -0.45f);
                    break;
                }
                case PartType.SightTube:
                {
                    PartView pv = NewPart(i, "GlassSightTube", new Vector3(lx, ly, -0.3f));
                    Transform t = pv.go.transform;
                    Prims.Make("Glass", t, Prims.Cylinder, GameMaterials.Acrylic(spec.color), Vector3.zero, new Vector3(0.28f, 0.7f, 0.28f));
                    Prims.Make("Water", t, Prims.Cylinder, GameMaterials.Glow(BlockColor.Blue), new Vector3(0f, -0.2f, 0f), new Vector3(0.16f, 0.4f, 0.16f));
                    Prims.Make("CapTop", t, Prims.Cylinder, GameMaterials.Brass, new Vector3(0f, 0.72f, 0f), new Vector3(0.38f, 0.07f, 0.38f));
                    Prims.Make("CapBot", t, Prims.Cylinder, GameMaterials.Brass, new Vector3(0f, -0.72f, 0f), new Vector3(0.38f, 0.07f, 0.38f));
                    Marker(pv, spec.color, new Vector3(0.45f, 0.4f, -0.1f));
                    Box(pv.go, Vector3.zero, new Vector3(0.4f, 1.5f, 0.4f));
                    break;
                }
                case PartType.DripGrid:
                {
                    PartView pv = NewPart(i, "StainlessDripGrid", new Vector3(lx, ly, -0.85f));
                    Transform t = pv.go.transform;
                    Prims.Make("Plate", t, Prims.Cube, GameMaterials.Steel, Vector3.zero, new Vector3(1.9f, 0.08f, 0.8f));
                    for (int k = 0; k < 9; k++)
                        Prims.Make("Slot" + k, t, Prims.Cube, GameMaterials.Rubber, new Vector3(-0.8f + k * 0.2f, 0.05f, 0f), new Vector3(0.07f, 0.02f, 0.6f));
                    Prims.Make("Tint", t, Prims.Cube, GameMaterials.Glow(spec.color), new Vector3(0f, 0.01f, -0.42f), new Vector3(1.9f, 0.05f, 0.04f));
                    Marker(pv, spec.color, new Vector3(0f, 0.4f, -0.2f));
                    Box(pv.go, Vector3.zero, new Vector3(1.9f, 0.3f, 0.8f));
                    break;
                }
                case PartType.Coil:
                {
                    PartView pv = NewPart(i, "CopperHeatingCoil", new Vector3(lx, ly, -0.1f));
                    Transform t = pv.go.transform;
                    for (int k = 0; k < 4; k++)
                        Prims.Make("Ring" + k, t, Prims.Cylinder, GameMaterials.Copper, new Vector3(0f, -0.35f + k * 0.24f, 0f), new Vector3(1.0f - k * 0.06f, 0.045f, 1.0f - k * 0.06f));
                    Prims.Make("Glow", t, Prims.Cylinder, GameMaterials.Glow(BlockColor.Yellow), new Vector3(0f, 0.1f, 0f), new Vector3(0.4f, 0.5f, 0.4f));
                    Marker(pv, spec.color, new Vector3(0.7f, 0.5f, -0.3f));
                    Sphere(pv.go, Vector3.zero, 0.6f);
                    break;
                }
                case PartType.Valve:
                {
                    PartView pv = NewPart(i, "BrassGearValve" + spec.slot, new Vector3(lx, ly, -0.3f));
                    BuildGear(pv, spec.color, 0.62f);
                    Marker(pv, spec.color, new Vector3(0f, 0.5f, -0.15f));
                    Sphere(pv.go, Vector3.zero, 0.36f);
                    break;
                }
                case PartType.Tnt:
                    BuildDynamiteBundle(i, spec, lx, ly, -0.55f);
                    break;
                default:
                {
                    PartView pv = NewPart(i, "Part" + i, new Vector3(lx, ly, 0f));
                    Prims.Make("Block", pv.go.transform, Prims.Cube, mat, Vector3.zero, Vector3.one * 0.5f);
                    Marker(pv, spec.color, new Vector3(0f, 0.5f, 0f));
                    Box(pv.go, Vector3.zero, Vector3.one * 0.5f);
                    break;
                }
            }
        }

        /// <summary>A strapped bundle of three dynamite sticks (used by the chapter 5 / 6 monuments).</summary>
        private void BuildDynamiteBundle(int i, PartSpec spec, float lx, float ly, float z)
        {
            PartView pv = NewPart(i, "DynamiteBundle" + spec.slot, new Vector3(lx, ly, z));
            Transform t = pv.go.transform;
            for (int k = 0; k < 3; k++)
                Prims.Make("Stick" + k, t, Prims.Cylinder, GameMaterials.TntRed, new Vector3((k - 1) * 0.22f, k == 1 ? 0.06f : 0f, 0f), new Vector3(0.2f, 0.34f, 0.2f));
            Prims.Make("StrapA", t, Prims.Cylinder, GameMaterials.Charcoal, new Vector3(0f, -0.14f, 0f), new Vector3(0.7f, 0.04f, 0.4f));
            Prims.Make("StrapB", t, Prims.Cylinder, GameMaterials.Charcoal, new Vector3(0f, 0.18f, 0f), new Vector3(0.7f, 0.04f, 0.4f));
            Prims.Make("Fuse", t, Prims.Cylinder, GameMaterials.Rubber, new Vector3(0.05f, 0.5f, 0f), new Vector3(0.04f, 0.16f, 0.04f), new Vector3(0f, 0f, 18f));
            Prims.Make("Spark", t, Prims.Sphere, GameMaterials.Glow(BlockColor.Yellow), new Vector3(0.1f, 0.68f, 0f), Vector3.one * 0.1f);
            Prims.Label(t, "TNT", 0.2f, Color.white, new Vector3(0f, 0f, -0.25f));
            Marker(pv, spec.color, new Vector3(0f, 0.85f, -0.2f));
            Box(pv.go, Vector3.zero, new Vector3(0.8f, 0.7f, 0.5f));
        }

        // ------------------------------------------------------------------ chapter 13: cathedral pipe organ

        private void BuildOrganDecor()
        {
            Prims.Make("StonePlinth", pivot, Prims.Cube, GameMaterials.Gunmetal, new Vector3(0f, 0.15f, 0.1f), new Vector3(3.3f, 0.3f, 1.9f));
            Prims.Make("PlinthTrim", pivot, Prims.Cube, GameMaterials.Brass, new Vector3(0f, 0.32f, -0.86f), new Vector3(3.2f, 0.05f, 0.05f));
            Prims.Make("ConsoleBody", pivot, Prims.Cube, GameMaterials.Mahogany, new Vector3(0f, 0.68f, 0.25f), new Vector3(2.7f, 0.76f, 1.5f));
            Prims.Make("TowerL", pivot, Prims.Cube, GameMaterials.Mahogany, new Vector3(-1.4f, 1.9f, 0.55f), new Vector3(0.28f, 3.0f, 0.9f));
            Prims.Make("TowerR", pivot, Prims.Cube, GameMaterials.Mahogany, new Vector3(1.4f, 1.9f, 0.55f), new Vector3(0.28f, 3.0f, 0.9f));
            Prims.Make("TowerCapL", pivot, Prims.Sphere, GameMaterials.Gold, new Vector3(-1.4f, 3.45f, 0.55f), Vector3.one * 0.26f);
            Prims.Make("TowerCapR", pivot, Prims.Sphere, GameMaterials.Gold, new Vector3(1.4f, 3.45f, 0.55f), Vector3.one * 0.26f);
            Prims.Make("BackBoard", pivot, Prims.Cube, GameMaterials.Charcoal, new Vector3(0f, 2.0f, 0.95f), new Vector3(2.5f, 2.8f, 0.1f));
            Prims.Make("BenchSeat", pivot, Prims.Cube, GameMaterials.Mahogany, new Vector3(0f, 0.5f, -1.45f), new Vector3(1.3f, 0.12f, 0.5f));
            Prims.Make("BenchLegL", pivot, Prims.Cylinder, GameMaterials.Brass, new Vector3(-0.5f, 0.28f, -1.45f), new Vector3(0.08f, 0.22f, 0.08f));
            Prims.Make("BenchLegR", pivot, Prims.Cylinder, GameMaterials.Brass, new Vector3(0.5f, 0.28f, -1.45f), new Vector3(0.08f, 0.22f, 0.08f));
        }

        private void BuildOrganPart(int i, PartSpec spec, float lx, float ly)
        {
            Material mat = GameMaterials.ForBlock(spec.color);
            switch (spec.type)
            {
                case PartType.Facade:
                {
                    PartView pv = NewPart(i, "OrganFacade" + spec.slot, new Vector3(0f, spec.slot == 0 ? 2.1f : 3.1f, -0.5f));
                    Transform t = pv.go.transform;
                    if (spec.slot == 0)
                    {
                        Prims.Make("Screen", t, Prims.Cube, GameMaterials.Mahogany, Vector3.zero, new Vector3(2.35f, 1.9f, 0.1f));
                        Prims.Make("ArchGold", t, Prims.Cylinder, GameMaterials.Gold, new Vector3(0f, 0.95f, -0.06f), new Vector3(1.5f, 0.05f, 0.5f), new Vector3(90f, 0f, 0f));
                        Prims.Make("FrameL", t, Prims.Cube, GameMaterials.Gold, new Vector3(-1.18f, 0f, -0.02f), new Vector3(0.07f, 1.95f, 0.14f));
                        Prims.Make("FrameR", t, Prims.Cube, GameMaterials.Gold, new Vector3(1.18f, 0f, -0.02f), new Vector3(0.07f, 1.95f, 0.14f));
                        for (int k = -2; k <= 2; k++)
                            Prims.Make("Scroll" + k, t, Prims.Sphere, GameMaterials.Brass, new Vector3(k * 0.38f, -0.2f, -0.08f), new Vector3(0.22f, 0.5f, 0.05f));
                        Prims.Make("Tint", t, Prims.Sphere, GameMaterials.Glow(spec.color), new Vector3(0f, 0.55f, -0.1f), Vector3.one * 0.2f);
                        Marker(pv, spec.color, new Vector3(1.0f, 0.85f, -0.25f));
                        Box(pv.go, Vector3.zero, new Vector3(2.4f, 1.95f, 0.3f));
                        pv.aim = new Vector3(0f, 0f, -0.25f);
                    }
                    else
                    {
                        Prims.Make("Cornice", t, Prims.Cube, GameMaterials.Mahogany, Vector3.zero, new Vector3(3.1f, 0.26f, 1.2f));
                        Prims.Make("CorniceTrim", t, Prims.Cube, GameMaterials.Gold, new Vector3(0f, 0.17f, -0.55f), new Vector3(3.0f, 0.07f, 0.06f));
                        for (int k = -3; k <= 3; k++)
                            Prims.Make("Finial" + k, t, Prims.Cylinder, GameMaterials.Gold, new Vector3(k * 0.42f, 0.28f, -0.5f), new Vector3(0.07f, 0.12f, 0.07f));
                        Prims.Make("Tint", t, Prims.Sphere, GameMaterials.Glow(spec.color), new Vector3(-1.3f, 0.02f, -0.62f), Vector3.one * 0.15f);
                        Marker(pv, spec.color, new Vector3(-1.3f, 0.4f, -0.6f));
                        Box(pv.go, Vector3.zero, new Vector3(3.1f, 0.4f, 1.2f));
                        pv.aim = new Vector3(0f, 0f, 0.65f);
                    }
                    break;
                }
                case PartType.Pipes:
                {
                    PartView pv = NewPart(i, "PipeBank" + spec.slot, new Vector3(spec.slot == 0 ? 0f : 1.0f, spec.slot == 0 ? 2.1f : 2.0f, 0.15f));
                    Transform t = pv.go.transform;
                    int n = spec.slot == 0 ? 7 : 3;
                    float w = spec.slot == 0 ? 0.3f : 0.22f;
                    for (int k = 0; k < n; k++)
                    {
                        float h = (spec.slot == 0 ? 1.7f : 1.2f) - Mathf.Abs(k - (n - 1) * 0.5f) * 0.18f;
                        float x = (k - (n - 1) * 0.5f) * w * 1.05f;
                        Prims.Make("Pipe" + k, t, Prims.Cylinder, k % 2 == 0 ? GameMaterials.Brass : GameMaterials.Gold, new Vector3(x, 0f, 0f), new Vector3(w * 0.8f, h * 0.5f, w * 0.8f));
                        Prims.Make("Mouth" + k, t, Prims.Cube, GameMaterials.Charcoal, new Vector3(x, -h * 0.35f, -w * 0.4f), new Vector3(w * 0.55f, 0.08f, 0.04f));
                    }
                    Prims.Make("Tint", t, Prims.Sphere, GameMaterials.Glow(spec.color), new Vector3(0f, -0.8f, -0.25f), Vector3.one * 0.16f);
                    Marker(pv, spec.color, new Vector3(0.9f, 0.7f, -0.3f));
                    Box(pv.go, Vector3.zero, new Vector3(n * w * 1.1f, 1.7f, 0.6f));
                    pv.aim = new Vector3(0f, 0f, -0.35f);
                    break;
                }
                case PartType.Keyboard:
                {
                    PartView pv = NewPart(i, "OrganKeyboard", new Vector3(0f, 1.2f, -0.7f));
                    Transform t = pv.go.transform;
                    Prims.Make("Shelf", t, Prims.Cube, GameMaterials.Mahogany, new Vector3(0f, -0.04f, 0.1f), new Vector3(2.1f, 0.1f, 0.65f));
                    for (int k = 0; k < 14; k++)
                        Prims.Make("Key" + k, t, Prims.Cube, GameMaterials.Cream, new Vector3(-0.91f + k * 0.14f, 0.03f, -0.1f), new Vector3(0.125f, 0.05f, 0.38f));
                    int[] blacks = { 0, 1, 3, 4, 5, 7, 8, 10, 11, 12 };
                    for (int b = 0; b < blacks.Length; b++)
                        Prims.Make("Black" + b, t, Prims.Cube, GameMaterials.Charcoal, new Vector3(-0.84f + blacks[b] * 0.14f, 0.07f, -0.02f), new Vector3(0.07f, 0.06f, 0.22f));
                    Prims.Make("Tint", t, Prims.Sphere, GameMaterials.Glow(spec.color), new Vector3(1.0f, 0.05f, 0.15f), Vector3.one * 0.14f);
                    Marker(pv, spec.color, new Vector3(1.1f, 0.4f, -0.1f));
                    Box(pv.go, new Vector3(0f, 0f, 0f), new Vector3(2.1f, 0.3f, 0.7f));
                    pv.aim = new Vector3(0f, 0.1f, -0.3f);
                    break;
                }
                case PartType.Bellows:
                {
                    PartView pv = NewPart(i, "WindBellows", new Vector3(0f, 1.0f, 0.75f));
                    Transform t = pv.go.transform;
                    GameObject pump = new GameObject("BellowsPump");
                    pump.transform.SetParent(t, false);
                    for (int k = 0; k < 6; k++)
                        Prims.Make("Fold" + k, pump.transform, Prims.Cylinder, k % 2 == 0 ? GameMaterials.Mahogany : GameMaterials.Copper, new Vector3(0f, -0.35f + k * 0.14f, 0f), new Vector3(0.95f - Mathf.Abs(k - 2.5f) * 0.05f, 0.05f, 0.7f));
                    Prims.Make("Hinge", t, Prims.Cylinder, GameMaterials.Brass, new Vector3(0f, -0.45f, 0f), new Vector3(1.0f, 0.04f, 0.74f));
                    Prims.Make("Tint", t, Prims.Sphere, GameMaterials.Glow(spec.color), new Vector3(0f, 0.15f, -0.4f), Vector3.one * 0.15f);
                    pv.spin = pump.transform;
                    pv.spinSpeed = 1f;
                    Marker(pv, spec.color, new Vector3(0.6f, 0.5f, -0.3f));
                    Box(pv.go, Vector3.zero, new Vector3(1.0f, 0.9f, 0.7f));
                    pv.aim = new Vector3(0f, 0f, -0.45f);
                    break;
                }
                case PartType.Stops:
                {
                    PartView pv = NewPart(i, "StopRail" + spec.slot, new Vector3(spec.slot == 0 ? -0.9f : 0.9f, 1.7f, -0.6f));
                    Transform t = pv.go.transform;
                    Prims.Make("Rail", t, Prims.Cube, GameMaterials.Mahogany, Vector3.zero, new Vector3(0.7f, 0.55f, 0.12f));
                    for (int k = 0; k < 6; k++)
                        Prims.Make("Knob" + k, t, Prims.Cylinder, k % 2 == 0 ? GameMaterials.Cream : GameMaterials.Gold, new Vector3(-0.2f + (k % 3) * 0.2f, k < 3 ? 0.14f : -0.14f, -0.09f), new Vector3(0.12f, 0.05f, 0.12f), new Vector3(90f, 0f, 0f));
                    Prims.Make("Tint", t, Prims.Sphere, GameMaterials.Glow(spec.color), new Vector3(0f, 0f, -0.14f), Vector3.one * 0.12f);
                    Marker(pv, spec.color, new Vector3(0.4f, 0.35f, -0.2f));
                    Box(pv.go, Vector3.zero, new Vector3(0.75f, 0.6f, 0.2f));
                    pv.aim = new Vector3(0f, 0f, -0.25f);
                    break;
                }
                case PartType.Tnt:
                    BuildDynamiteBundle(i, spec, 0f, 0f, 0f);
                    views[i].go.transform.localPosition = new Vector3(spec.slot == 0 ? 0.4f : -0.4f, 1.15f, 0.55f);
                    views[i].go.transform.localScale = Vector3.one * 0.6f;
                    break;
                default:
                {
                    PartView pv = NewPart(i, "Part" + i, new Vector3(0f, 2f, 0f));
                    Prims.Make("Block", pv.go.transform, Prims.Cube, mat, Vector3.zero, Vector3.one * 0.5f);
                    Marker(pv, spec.color, new Vector3(0f, 0.5f, 0f));
                    Box(pv.go, Vector3.zero, Vector3.one * 0.5f);
                    break;
                }
            }
        }

        // ------------------------------------------------------------------ chapter 12: automaton music box

        private void BuildMusicBoxDecor()
        {
            // inlaid mahogany case with brass trim, bun feet and a velvet-dark interior
            Prims.Make("Case", pivot, Prims.Cube, GameMaterials.Mahogany, new Vector3(0f, 0.6f, 0.1f), new Vector3(2.8f, 0.75f, 1.8f));
            Prims.Make("Inlay", pivot, Prims.Cube, GameMaterials.Brass, new Vector3(0f, 0.6f, -0.81f), new Vector3(2.5f, 0.12f, 0.04f));
            Prims.Make("InlayTop", pivot, Prims.Cube, GameMaterials.Brass, new Vector3(0f, 0.99f, -0.78f), new Vector3(2.85f, 0.05f, 0.05f));
            Prims.Make("Interior", pivot, Prims.Cube, GameMaterials.Charcoal, new Vector3(0f, 1.0f, 0.1f), new Vector3(2.5f, 0.05f, 1.55f));
            Prims.Make("BackWall", pivot, Prims.Cube, GameMaterials.Mahogany, new Vector3(0f, 1.55f, 0.88f), new Vector3(2.8f, 1.2f, 0.12f));
            for (int k = 0; k < 4; k++)
                Prims.Make("Foot" + k, pivot, Prims.Sphere, GameMaterials.Brass, new Vector3(k < 2 ? -1.2f : 1.2f, 0.12f, k % 2 == 0 ? -0.7f : 0.9f), Vector3.one * 0.3f);
        }

        private void BuildMusicBoxPart(int i, PartSpec spec, float lx, float ly)
        {
            Material mat = GameMaterials.ForBlock(spec.color);
            switch (spec.type)
            {
                case PartType.Lid:
                {
                    PartView pv = NewPart(i, "MusicLid" + spec.slot, new Vector3(0f, spec.slot == 0 ? 2.1f : 1.15f, 0.05f));
                    Transform t = pv.go.transform;
                    if (spec.slot == 0)
                    {
                        Prims.Make("Dome", t, Prims.Sphere, GameMaterials.Acrylic(spec.color), Vector3.zero, new Vector3(2.55f, 1.9f, 1.75f));
                        Prims.Make("Glint", t, Prims.Sphere, GameMaterials.Glow(spec.color), new Vector3(-0.7f, 0.55f, -0.62f), new Vector3(0.5f, 0.1f, 0.05f), new Vector3(0f, 0f, 30f));
                        Marker(pv, spec.color, new Vector3(0.95f, 0.8f, -0.8f));
                        Sphere(pv.go, Vector3.zero, 1.0f);
                        pv.aim = new Vector3(0f, 0.2f, -0.8f);
                    }
                    else
                    {
                        Prims.Make("Collar", t, Prims.Cylinder, GameMaterials.Brass, Vector3.zero, new Vector3(2.7f, 0.1f, 1.8f));
                        Prims.Make("CollarTrim", t, Prims.Cylinder, GameMaterials.Gold, new Vector3(0f, 0.08f, 0f), new Vector3(2.5f, 0.05f, 1.62f));
                        Prims.Make("Tint", t, Prims.Sphere, GameMaterials.Glow(spec.color), new Vector3(-1.0f, 0.05f, -0.6f), Vector3.one * 0.15f);
                        Marker(pv, spec.color, new Vector3(-1.2f, 0.3f, -0.6f));
                        Box(pv.go, Vector3.zero, new Vector3(2.7f, 0.25f, 1.8f));
                        pv.aim = new Vector3(0f, 0f, 0.6f);
                    }
                    break;
                }
                case PartType.Crank:
                {
                    PartView pv = NewPart(i, "WindingKey", new Vector3(1.55f, 0.9f, -0.1f));
                    Transform t = pv.go.transform;
                    Prims.Make("Shaft", t, Prims.Cylinder, GameMaterials.Steel, Vector3.zero, new Vector3(0.1f, 0.2f, 0.1f), new Vector3(0f, 0f, 90f));
                    GameObject spin = new GameObject("CrankSpin");
                    spin.transform.SetParent(t, false);
                    spin.transform.localPosition = new Vector3(0.22f, 0f, -0.1f);
                    Prims.Make("Arm", spin.transform, Prims.Cube, GameMaterials.Brass, new Vector3(0f, 0.22f, 0f), new Vector3(0.09f, 0.5f, 0.05f));
                    Prims.Make("Knob", spin.transform, Prims.Sphere, GameMaterials.Glow(spec.color), new Vector3(0f, 0.5f, 0f), Vector3.one * 0.2f);
                    Prims.Make("Bow", spin.transform, Prims.Sphere, GameMaterials.Gold, new Vector3(0f, -0.2f, 0f), new Vector3(0.28f, 0.2f, 0.08f));
                    pv.spin = spin.transform;
                    pv.spinSpeed = 55f;
                    Marker(pv, spec.color, new Vector3(0.3f, 0.7f, -0.3f));
                    Sphere(pv.go, new Vector3(0.2f, 0.1f, 0f), 0.5f);
                    break;
                }
                case PartType.Comb:
                {
                    PartView pv = NewPart(i, "SteelComb", new Vector3(0f, 1.25f, -0.45f));
                    Transform t = pv.go.transform;
                    Prims.Make("Bed", t, Prims.Cube, GameMaterials.Brass, new Vector3(0f, 0f, 0.1f), new Vector3(1.9f, 0.1f, 0.18f));
                    for (int k = 0; k < 14; k++)
                        Prims.Make("Tooth" + k, t, Prims.Cube, GameMaterials.Steel, new Vector3(-0.86f + k * 0.132f, 0.03f, -0.1f - (k % 3) * 0.02f), new Vector3(0.05f, 0.025f, 0.34f - (k % 5) * 0.03f));
                    Prims.Make("Tint", t, Prims.Sphere, GameMaterials.Glow(spec.color), new Vector3(0.9f, 0.08f, 0.1f), Vector3.one * 0.13f);
                    Marker(pv, spec.color, new Vector3(1.0f, 0.4f, -0.2f));
                    Box(pv.go, new Vector3(0f, 0f, 0f), new Vector3(2.0f, 0.3f, 0.5f));
                    pv.aim = new Vector3(0f, 0.1f, -0.3f);
                    break;
                }
                case PartType.Dancer:
                {
                    PartView pv = NewPart(i, "BalletDancer", new Vector3(0f, 1.55f, -0.05f));
                    Transform t = pv.go.transform;
                    Prims.Make("Plinth", t, Prims.Cylinder, GameMaterials.Brass, new Vector3(0f, 0f, 0f), new Vector3(0.7f, 0.06f, 0.7f));
                    GameObject spin = new GameObject("GovSpin");
                    spin.transform.SetParent(t, false);
                    Prims.Make("Tutu", spin.transform, Prims.Cylinder, GameMaterials.Cream, new Vector3(0f, 0.38f, 0f), new Vector3(0.62f, 0.05f, 0.62f));
                    Prims.Make("Bodice", spin.transform, Prims.Cylinder, mat, new Vector3(0f, 0.6f, 0f), new Vector3(0.22f, 0.2f, 0.22f));
                    Prims.Make("Head", spin.transform, Prims.Sphere, GameMaterials.Cream, new Vector3(0f, 0.92f, 0f), Vector3.one * 0.2f);
                    Prims.Make("Bun", spin.transform, Prims.Sphere, GameMaterials.Charcoal, new Vector3(0f, 1.04f, 0.05f), Vector3.one * 0.09f);
                    Prims.Make("ArmL", spin.transform, Prims.Cube, GameMaterials.Cream, new Vector3(-0.25f, 0.8f, 0f), new Vector3(0.3f, 0.04f, 0.04f), new Vector3(0f, 0f, 35f));
                    Prims.Make("ArmR", spin.transform, Prims.Cube, GameMaterials.Cream, new Vector3(0.25f, 0.8f, 0f), new Vector3(0.3f, 0.04f, 0.04f), new Vector3(0f, 0f, -35f));
                    Prims.Make("Leg", spin.transform, Prims.Cylinder, GameMaterials.Cream, new Vector3(0f, 0.15f, 0f), new Vector3(0.07f, 0.2f, 0.07f));
                    pv.spin = spin.transform;
                    pv.spinSpeed = 45f;
                    Marker(pv, spec.color, new Vector3(0.45f, 0.9f, -0.3f));
                    Sphere(pv.go, new Vector3(0f, 0.55f, 0f), 0.5f);
                    pv.aim = new Vector3(0f, 0.55f, -0.3f);
                    break;
                }
                case PartType.Drum:
                {
                    PartView pv = NewPart(i, "PinnedDisc" + spec.slot, new Vector3(spec.slot == 0 ? 0f : 0.5f, spec.slot == 0 ? 1.75f : 1.3f, spec.slot == 0 ? 0.5f : 0.2f));
                    Transform t = pv.go.transform;
                    GameObject spin = new GameObject("DiscSpin");
                    spin.transform.SetParent(t, false);
                    float r = spec.slot == 0 ? 0.85f : 0.55f;
                    Prims.Make("Disc", spin.transform, Prims.Cylinder, GameMaterials.Steel, Vector3.zero, new Vector3(r * 2f, 0.03f, r * 2f), new Vector3(90f, 0f, 0f));
                    Prims.Make("Rim", spin.transform, Prims.Cylinder, GameMaterials.Brass, new Vector3(0f, 0f, 0.015f), new Vector3(r * 2.05f, 0.015f, r * 2.05f), new Vector3(90f, 0f, 0f));
                    for (int k = 0; k < 16; k++)
                    {
                        float a = k * 22.5f * Mathf.Deg2Rad;
                        float rr = r * (0.35f + (k % 3) * 0.2f);
                        Prims.Make("Pin" + k, spin.transform, Prims.Sphere, GameMaterials.Gold, new Vector3(Mathf.Cos(a) * rr, Mathf.Sin(a) * rr, -0.03f), Vector3.one * 0.06f);
                    }
                    Prims.Make("Tint", spin.transform, Prims.Cylinder, GameMaterials.Glow(spec.color), new Vector3(0f, 0f, -0.04f), new Vector3(0.22f, 0.02f, 0.22f), new Vector3(90f, 0f, 0f));
                    pv.spin = spin.transform;
                    pv.spinSpeed = spec.slot == 0 ? 22f : -30f;
                    Marker(pv, spec.color, new Vector3(r * 0.8f, r * 0.8f, -0.25f));
                    Sphere(pv.go, Vector3.zero, r * 0.9f);
                    pv.aim = new Vector3(0f, 0f, -0.3f);
                    break;
                }
                case PartType.Tnt:
                    BuildDynamiteBundle(i, spec, 0f, 0f, 0f);
                    views[i].go.transform.localPosition = new Vector3(spec.slot == 0 ? 0.45f : -0.45f, 1.3f, 0.65f);
                    views[i].go.transform.localScale = Vector3.one * 0.62f;
                    break;
                default:
                {
                    PartView pv = NewPart(i, "Part" + i, new Vector3(0f, 1.8f, 0f));
                    Prims.Make("Block", pv.go.transform, Prims.Cube, mat, Vector3.zero, Vector3.one * 0.5f);
                    Marker(pv, spec.color, new Vector3(0f, 0.5f, 0f));
                    Box(pv.go, Vector3.zero, Vector3.one * 0.5f);
                    break;
                }
            }
        }

        // ------------------------------------------------------------------ chapter 11: grandfather clock tower

        private void BuildGrandfatherDecor()
        {
            // plinth, two carved case sides, back panel and a brass floor trim: the open case the movement sits in
            Prims.Make("Plinth", pivot, Prims.Cube, GameMaterials.Mahogany, new Vector3(0f, 0.2f, 0.1f), new Vector3(2.2f, 0.4f, 1.5f));
            Prims.Make("PlinthTrim", pivot, Prims.Cube, GameMaterials.Brass, new Vector3(0f, 0.42f, -0.62f), new Vector3(2.0f, 0.05f, 0.05f));
            Prims.Make("CaseL", pivot, Prims.Cube, GameMaterials.Mahogany, new Vector3(-0.95f, 1.85f, 0.2f), new Vector3(0.2f, 2.9f, 1.1f));
            Prims.Make("CaseR", pivot, Prims.Cube, GameMaterials.Mahogany, new Vector3(0.95f, 1.85f, 0.2f), new Vector3(0.2f, 2.9f, 1.1f));
            Prims.Make("CaseBack", pivot, Prims.Cube, GameMaterials.Charcoal, new Vector3(0f, 1.85f, 0.7f), new Vector3(1.7f, 2.9f, 0.1f));
            Prims.Make("CaseBase", pivot, Prims.Cube, GameMaterials.Mahogany, new Vector3(0f, 0.65f, 0.2f), new Vector3(1.7f, 0.5f, 1.1f));
            for (int k = 0; k < 4; k++)
                Prims.Make("Foot" + k, pivot, Prims.Sphere, GameMaterials.Brass, new Vector3(k < 2 ? -0.9f : 0.9f, 0.06f, k % 2 == 0 ? -0.5f : 0.7f), Vector3.one * 0.22f);
        }

        private void BuildGrandfatherPart(int i, PartSpec spec, float lx, float ly)
        {
            Material mat = GameMaterials.ForBlock(spec.color);
            Vector3 front = new Vector3(0f, 0f, -0.45f);
            switch (spec.type)
            {
                case PartType.Hood:
                {
                    PartView pv = NewPart(i, "ClockHood" + spec.slot, new Vector3(0f, spec.slot == 0 ? 3.55f : 3.15f, -0.05f));
                    Transform t = pv.go.transform;
                    if (spec.slot == 0)
                    {
                        Prims.Make("Crown", t, Prims.Cube, GameMaterials.Mahogany, new Vector3(0f, 0f, 0f), new Vector3(2.15f, 0.34f, 1.35f));
                        Prims.Make("Arch", t, Prims.Cylinder, GameMaterials.Mahogany, new Vector3(0f, 0.2f, 0f), new Vector3(1.8f, 0.68f, 1.2f), new Vector3(90f, 0f, 0f));
                        Prims.Make("Finial", t, Prims.Sphere, GameMaterials.Gold, new Vector3(0f, 0.72f, 0f), Vector3.one * 0.26f);
                        Prims.Make("Tint", t, Prims.Sphere, GameMaterials.Glow(spec.color), new Vector3(0.55f, 0.1f, -0.6f), Vector3.one * 0.16f);
                        Marker(pv, spec.color, new Vector3(0.8f, 0.5f, -0.7f));
                        Box(pv.go, Vector3.zero, new Vector3(2.0f, 0.8f, 1.2f));
                        pv.aim = new Vector3(0f, 0.1f, -0.65f);
                    }
                    else
                    {
                        Prims.Make("Cornice", t, Prims.Cube, GameMaterials.Brass, Vector3.zero, new Vector3(2.0f, 0.2f, 1.25f));
                        Prims.Make("ColumnL", t, Prims.Cylinder, GameMaterials.Brass, new Vector3(-0.8f, -0.25f, -0.55f), new Vector3(0.14f, 0.3f, 0.14f));
                        Prims.Make("ColumnR", t, Prims.Cylinder, GameMaterials.Brass, new Vector3(0.8f, -0.25f, -0.55f), new Vector3(0.14f, 0.3f, 0.14f));
                        Prims.Make("Tint", t, Prims.Sphere, GameMaterials.Glow(spec.color), new Vector3(-0.5f, 0f, -0.66f), Vector3.one * 0.14f);
                        Marker(pv, spec.color, new Vector3(-0.8f, 0.3f, -0.7f));
                        Box(pv.go, Vector3.zero, new Vector3(1.9f, 0.3f, 1.2f));
                        pv.aim = new Vector3(0f, 0f, 0.55f);
                    }
                    break;
                }
                case PartType.ClockDoor:
                {
                    PartView pv = NewPart(i, "GlassDoor", new Vector3(0f, 1.7f, -0.62f));
                    Transform t = pv.go.transform;
                    Prims.Make("Pane", t, Prims.Cube, GameMaterials.Acrylic(spec.color), Vector3.zero, new Vector3(1.55f, 2.0f, 0.06f));
                    Prims.Make("FrameL", t, Prims.Cube, GameMaterials.Brass, new Vector3(-0.78f, 0f, 0f), new Vector3(0.07f, 2.05f, 0.1f));
                    Prims.Make("FrameR", t, Prims.Cube, GameMaterials.Brass, new Vector3(0.78f, 0f, 0f), new Vector3(0.07f, 2.05f, 0.1f));
                    Prims.Make("FrameT", t, Prims.Cube, GameMaterials.Brass, new Vector3(0f, 1.0f, 0f), new Vector3(1.6f, 0.07f, 0.1f));
                    Prims.Make("FrameB", t, Prims.Cube, GameMaterials.Brass, new Vector3(0f, -1.0f, 0f), new Vector3(1.6f, 0.07f, 0.1f));
                    Prims.Make("Handle", t, Prims.Sphere, GameMaterials.Gold, new Vector3(0.6f, 0f, -0.1f), Vector3.one * 0.14f);
                    Prims.Make("Tint", t, Prims.Sphere, GameMaterials.Glow(spec.color), new Vector3(-0.55f, 0.8f, -0.08f), Vector3.one * 0.13f);
                    Marker(pv, spec.color, new Vector3(0.55f, 0.85f, -0.25f));
                    Box(pv.go, Vector3.zero, new Vector3(1.6f, 2.0f, 0.2f));
                    pv.aim = new Vector3(0f, 0.2f, -0.2f);
                    break;
                }
                case PartType.Dial:
                {
                    PartView pv = NewPart(i, "ClockDial", new Vector3(0f, 2.75f, -0.1f));
                    Transform t = pv.go.transform;
                    Prims.Make("Face", t, Prims.Cylinder, GameMaterials.Cream, Vector3.zero, new Vector3(1.25f, 0.05f, 1.25f), new Vector3(90f, 0f, 0f));
                    Prims.Make("Bezel", t, Prims.Cylinder, GameMaterials.Gold, new Vector3(0f, 0f, 0.02f), new Vector3(1.38f, 0.04f, 1.38f), new Vector3(90f, 0f, 0f));
                    for (int k = 0; k < 12; k++)
                    {
                        float a = k * 30f * Mathf.Deg2Rad;
                        Prims.Make("Tick" + k, t, Prims.Cube, GameMaterials.Charcoal, new Vector3(Mathf.Sin(a) * 0.52f, Mathf.Cos(a) * 0.52f, -0.04f), new Vector3(0.05f, k % 3 == 0 ? 0.15f : 0.08f, 0.02f), new Vector3(0f, 0f, -k * 30f));
                    }
                    Prims.Make("HourHand", t, Prims.Cube, GameMaterials.Charcoal, new Vector3(0.1f, 0.14f, -0.06f), new Vector3(0.05f, 0.34f, 0.02f), new Vector3(0f, 0f, -30f));
                    Prims.Make("MinuteHand", t, Prims.Cube, GameMaterials.Charcoal, new Vector3(-0.2f, 0.12f, -0.07f), new Vector3(0.035f, 0.5f, 0.02f), new Vector3(0f, 0f, 60f));
                    Prims.Make("Tint", t, Prims.Sphere, GameMaterials.Glow(spec.color), new Vector3(0f, 0f, -0.1f), Vector3.one * 0.12f);
                    Marker(pv, spec.color, new Vector3(0.75f, 0.75f, -0.2f));
                    Sphere(pv.go, Vector3.zero, 0.7f);
                    pv.aim = new Vector3(0f, 0f, -0.15f);
                    break;
                }
                case PartType.Pendulum:
                {
                    PartView pv = NewPart(i, "Pendulum", new Vector3(0f, 2.15f, 0.25f));
                    Transform t = pv.go.transform;
                    GameObject swing = new GameObject("PendSwing");
                    swing.transform.SetParent(t, false);
                    Prims.Make("Rod", swing.transform, Prims.Cylinder, GameMaterials.Brass, new Vector3(0f, -0.7f, 0f), new Vector3(0.05f, 0.7f, 0.05f));
                    Prims.Make("Bob", swing.transform, Prims.Cylinder, GameMaterials.Gold, new Vector3(0f, -1.45f, 0f), new Vector3(0.62f, 0.05f, 0.62f), new Vector3(90f, 0f, 0f));
                    Prims.Make("BobRim", swing.transform, Prims.Cylinder, GameMaterials.Brass, new Vector3(0f, -1.45f, 0.02f), new Vector3(0.68f, 0.03f, 0.68f), new Vector3(90f, 0f, 0f));
                    Prims.Make("Tint", swing.transform, Prims.Sphere, GameMaterials.Glow(spec.color), new Vector3(0f, -1.45f, -0.05f), Vector3.one * 0.16f);
                    pv.spin = swing.transform;
                    pv.spinSpeed = 1f;
                    Marker(pv, spec.color, new Vector3(0.55f, -1.0f, -0.2f));
                    Sphere(pv.go, new Vector3(0f, -1.0f, 0f), 0.65f);
                    pv.aim = new Vector3(0f, -1.0f, -0.2f);
                    break;
                }
                case PartType.Weight:
                {
                    PartView pv = NewPart(i, "CastWeight" + spec.slot, new Vector3(spec.slot == 0 ? -0.6f : 0.6f, 1.2f, 0.3f));
                    Transform t = pv.go.transform;
                    Prims.Make("Chain", t, Prims.Cylinder, GameMaterials.Steel, new Vector3(0f, 0.7f, 0f), new Vector3(0.025f, 0.7f, 0.025f));
                    Prims.Make("Body", t, Prims.Cylinder, GameMaterials.Brass, new Vector3(0f, -0.1f, 0f), new Vector3(0.42f, 0.45f, 0.42f));
                    Prims.Make("Cap", t, Prims.Sphere, GameMaterials.Gold, new Vector3(0f, 0.35f, 0f), new Vector3(0.3f, 0.12f, 0.3f));
                    Prims.Make("Tint", t, Prims.Cylinder, GameMaterials.Glow(spec.color), new Vector3(0f, -0.1f, -0.2f), new Vector3(0.2f, 0.04f, 0.2f), new Vector3(90f, 0f, 0f));
                    Marker(pv, spec.color, new Vector3(0.4f, 0.35f, -0.3f));
                    Box(pv.go, new Vector3(0f, -0.1f, 0f), new Vector3(0.5f, 0.6f, 0.5f));
                    pv.aim = new Vector3(0f, -0.1f, -0.35f);
                    break;
                }
                case PartType.Chime:
                {
                    PartView pv = NewPart(i, "TubularChime" + spec.slot, new Vector3(spec.slot == 0 ? -0.35f : 0.35f, 2.0f, 0.4f));
                    Transform t = pv.go.transform;
                    for (int k = 0; k < 3; k++)
                    {
                        float h = 0.9f - k * 0.18f;
                        Prims.Make("Tube" + k, t, Prims.Cylinder, k % 2 == 0 ? GameMaterials.Brass : GameMaterials.Gold, new Vector3((k - 1) * 0.2f, -h * 0.5f, 0f), new Vector3(0.1f, h * 0.5f, 0.1f));
                    }
                    Prims.Make("Rail", t, Prims.Cube, GameMaterials.Copper, new Vector3(0f, 0.02f, 0f), new Vector3(0.7f, 0.05f, 0.06f));
                    Prims.Make("Tint", t, Prims.Sphere, GameMaterials.Glow(spec.color), new Vector3(0f, -0.9f, -0.08f), Vector3.one * 0.13f);
                    Marker(pv, spec.color, new Vector3(0.4f, 0.25f, -0.3f));
                    Box(pv.go, new Vector3(0f, -0.4f, 0f), new Vector3(0.75f, 1.0f, 0.4f));
                    pv.aim = new Vector3(0f, -0.4f, -0.3f);
                    break;
                }
                case PartType.Tnt:
                    BuildDynamiteBundle(i, spec, 0f, 0f, 0f);
                    views[i].go.transform.localPosition = new Vector3(spec.slot == 0 ? 0f : 0.3f, spec.slot == 0 ? 1.45f : 1.1f, 0.35f);
                    views[i].go.transform.localScale = Vector3.one * 0.7f;
                    break;
                default:
                {
                    PartView pv = NewPart(i, "Part" + i, new Vector3(0f, 2f, 0f));
                    Prims.Make("Block", pv.go.transform, Prims.Cube, mat, Vector3.zero, Vector3.one * 0.5f);
                    Marker(pv, spec.color, new Vector3(0f, 0.5f, 0f));
                    Box(pv.go, Vector3.zero, Vector3.one * 0.5f);
                    break;
                }
            }
        }

        // ------------------------------------------------------------------ chapter 10: swiss skeleton tourbillon

        /// <summary>Ring of boxes lying in the XY plane (a watch-face circle) instead of the XZ plane.</summary>
        private static void WatchRing(Transform parent, float radius, int n, Material mat, float width, float depth, Material alt)
        {
            for (int k = 0; k < n; k++)
            {
                float a = k * 360f / n;
                float rad = a * Mathf.Deg2Rad;
                float seg = 2f * Mathf.PI * radius / n * 1.1f;
                Prims.Make("Seg" + k, parent, Prims.Cube, (k % 2 == 0 || alt == null) ? mat : alt, new Vector3(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius, 0f), new Vector3(seg, width, depth), new Vector3(0f, 0f, a + 90f));
            }
        }

        private void BuildTourbillonDecor()
        {
            // watchmaker's brass movement holder: base plate, two clamp posts and the open skeleton main plate behind everything
            Prims.Make("HolderBase", pivot, Prims.Cylinder, GameMaterials.Brass, new Vector3(0f, 0.2f, 0.1f), new Vector3(3.2f, 0.4f, 3.2f));
            Prims.Make("HolderTrim", pivot, Prims.Cylinder, GameMaterials.Copper, new Vector3(0f, 0.42f, 0.1f), new Vector3(3.0f, 0.05f, 3.0f));
            Prims.Make("PostL", pivot, Prims.Cylinder, GameMaterials.Brass, new Vector3(-1.2f, 0.9f, 0.45f), new Vector3(0.2f, 1.0f, 0.2f));
            Prims.Make("PostR", pivot, Prims.Cylinder, GameMaterials.Brass, new Vector3(1.2f, 0.9f, 0.45f), new Vector3(0.2f, 1.0f, 0.2f));
            Prims.Make("ClampL", pivot, Prims.Sphere, GameMaterials.Steel, new Vector3(-1.2f, 1.45f, 0.45f), Vector3.one * 0.28f);
            Prims.Make("ClampR", pivot, Prims.Sphere, GameMaterials.Steel, new Vector3(1.2f, 1.45f, 0.45f), Vector3.one * 0.28f);
            GameObject plate = new GameObject("SkeletonPlate");
            plate.transform.SetParent(pivot, false);
            plate.transform.localPosition = new Vector3(0f, 2.1f, 0.5f);
            WatchRing(plate.transform, 1.35f, 36, GameMaterials.Rhodium, 0.08f, 0.06f, GameMaterials.Gold);
            for (int k = 0; k < 6; k++)
            {
                float a = k * 60f * Mathf.Deg2Rad;
                Prims.Make("Spoke" + k, plate.transform, Prims.Cube, GameMaterials.Rhodium, new Vector3(Mathf.Cos(a) * 0.7f, Mathf.Sin(a) * 0.7f, 0f), new Vector3(1.35f, 0.05f, 0.05f), new Vector3(0f, 0f, k * 60f));
            }
            for (int k = 0; k < 10; k++)
            {
                float a = k * 36f * Mathf.Deg2Rad;
                Prims.Make("PlateScrew" + k, plate.transform, Prims.Cylinder, GameMaterials.Gold, new Vector3(Mathf.Cos(a) * 1.35f, Mathf.Sin(a) * 1.35f, -0.04f), new Vector3(0.1f, 0.03f, 0.1f), new Vector3(90f, 0f, 0f));
            }
        }

        private void BuildTourbillonPart(int i, PartSpec spec, float lx, float ly)
        {
            Material mat = GameMaterials.ForBlock(spec.color);
            Vector3 centre = new Vector3(0f, 2.1f, 0.5f);
            switch (spec.type)
            {
                case PartType.Bezel:
                {
                    PartView pv = NewPart(i, "RoseGoldBezel" + spec.slot, centre + new Vector3(0f, 0f, -0.55f - spec.slot * 0.03f));
                    Transform t = pv.go.transform;
                    float r = 1.62f - spec.slot * 0.13f;
                    WatchRing(t, r, 40, GameMaterials.RoseGold, 0.12f, 0.1f, GameMaterials.Brass);
                    for (int k = 0; k < 4; k++)
                    {
                        float a = (45f + k * 90f) * Mathf.Deg2Rad;
                        Prims.Make("Jewel" + k, t, Prims.Sphere, GameMaterials.Glow(spec.color), new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, -0.06f), Vector3.one * 0.13f);
                    }
                    Marker(pv, spec.color, new Vector3(r * 0.8f, r * 0.7f, -0.15f));
                    Sphere(pv.go, Vector3.zero, r * 0.45f);
                    pv.aim = spec.slot >= 2 ? new Vector3(0f, 0f, 0.35f) : new Vector3(0f, 0f, -0.3f);
                    break;
                }
                case PartType.Crystal:
                {
                    PartView pv = NewPart(i, "SapphireCrystal", centre + new Vector3(0f, 0f, -0.75f));
                    Prims.Make("Dome", pv.go.transform, Prims.Sphere, GameMaterials.Acrylic(spec.color), Vector3.zero, new Vector3(2.9f, 2.9f, 0.55f));
                    Prims.Make("Glint", pv.go.transform, Prims.Sphere, GameMaterials.Glow(spec.color), new Vector3(-0.7f, 0.8f, -0.25f), new Vector3(0.5f, 0.12f, 0.05f), new Vector3(0f, 0f, -35f));
                    Marker(pv, spec.color, new Vector3(1.05f, 1.15f, -0.3f));
                    Sphere(pv.go, Vector3.zero, 1.3f);
                    pv.aim = new Vector3(0f, 0f, -0.4f);
                    break;
                }
                case PartType.Pusher:
                {
                    PartView pv = NewPart(i, "ChronographPusher" + spec.slot, centre + new Vector3(1.75f, spec.slot == 0 ? 0.65f : -0.55f, -0.3f));
                    Transform t = pv.go.transform;
                    Prims.Make("Stem", t, Prims.Cylinder, GameMaterials.Steel, Vector3.zero, new Vector3(0.14f, 0.28f, 0.14f), new Vector3(0f, 0f, 90f));
                    Prims.Make("Cap", t, Prims.Cylinder, GameMaterials.RoseGold, new Vector3(0.36f, 0f, 0f), new Vector3(0.3f, 0.12f, 0.3f), new Vector3(0f, 0f, 90f));
                    Prims.Make("Tint", t, Prims.Cylinder, GameMaterials.Glow(spec.color), new Vector3(0.5f, 0f, 0f), new Vector3(0.18f, 0.03f, 0.18f), new Vector3(0f, 0f, 90f));
                    Marker(pv, spec.color, new Vector3(0.3f, 0.4f, -0.2f));
                    Sphere(pv.go, new Vector3(0.3f, 0f, 0f), 0.3f);
                    break;
                }
                case PartType.Bridge:
                {
                    PartView pv = NewPart(i, "SkeletonBridge" + spec.slot, centre + new Vector3(0f, 0f, -0.2f));
                    Transform t = pv.go.transform;
                    float a0 = spec.slot == 0 ? 120f : (spec.slot == 1 ? -60f : 20f);
                    float span = spec.slot == 2 ? 100f : 130f;
                    for (int k = 0; k < 8; k++)
                    {
                        float a = (a0 + span * k / 7f) * Mathf.Deg2Rad;
                        Prims.Make("Arc" + k, t, Prims.Cube, k % 2 == 0 ? GameMaterials.Rhodium : GameMaterials.Gold, new Vector3(Mathf.Cos(a) * 1.0f, Mathf.Sin(a) * 1.0f, 0f), new Vector3(0.34f, 0.14f, 0.07f), new Vector3(0f, 0f, a * Mathf.Rad2Deg + 90f));
                    }
                    Prims.Make("Screw", t, Prims.Cylinder, GameMaterials.Gold, new Vector3(Mathf.Cos(a0 * Mathf.Deg2Rad) * 1.0f, Mathf.Sin(a0 * Mathf.Deg2Rad) * 1.0f, -0.05f), new Vector3(0.14f, 0.03f, 0.14f), new Vector3(90f, 0f, 0f));
                    Prims.Make("Tint", t, Prims.Sphere, GameMaterials.Glow(spec.color), new Vector3(Mathf.Cos((a0 + span * 0.5f) * Mathf.Deg2Rad) * 1.0f, Mathf.Sin((a0 + span * 0.5f) * Mathf.Deg2Rad) * 1.0f, -0.08f), Vector3.one * 0.12f);
                    Marker(pv, spec.color, new Vector3(Mathf.Cos((a0 + span * 0.5f) * Mathf.Deg2Rad) * 1.25f, Mathf.Sin((a0 + span * 0.5f) * Mathf.Deg2Rad) * 1.25f, -0.2f));
                    Sphere(pv.go, new Vector3(Mathf.Cos((a0 + span * 0.5f) * Mathf.Deg2Rad) * 1.0f, Mathf.Sin((a0 + span * 0.5f) * Mathf.Deg2Rad) * 1.0f, 0f), 0.5f);
                    pv.aim = new Vector3(Mathf.Cos((a0 + span * 0.5f) * Mathf.Deg2Rad) * 1.0f, Mathf.Sin((a0 + span * 0.5f) * Mathf.Deg2Rad) * 1.0f, spec.slot == 1 ? 0.3f : -0.15f);
                    break;
                }
                case PartType.TourbillonCage:
                {
                    PartView pv = NewPart(i, "TourbillonCage", centre + new Vector3(0f, -0.05f, -0.1f));
                    Transform t = pv.go.transform;
                    GameObject spin = new GameObject("CageSpin");
                    spin.transform.SetParent(t, false);
                    WatchRing(spin.transform, 0.5f, 20, GameMaterials.Gold, 0.06f, 0.06f, GameMaterials.Rhodium);
                    for (int k = 0; k < 3; k++)
                    {
                        float a = k * 120f * Mathf.Deg2Rad;
                        Prims.Make("Post" + k, spin.transform, Prims.Cylinder, GameMaterials.Gold, new Vector3(Mathf.Cos(a) * 0.5f, Mathf.Sin(a) * 0.5f, 0f), new Vector3(0.05f, 0.18f, 0.05f), new Vector3(90f, 0f, 0f));
                    }
                    Prims.Make("Balance", spin.transform, Prims.Cylinder, GameMaterials.Steel, Vector3.zero, new Vector3(0.62f, 0.02f, 0.62f), new Vector3(90f, 0f, 0f));
                    Prims.Make("Pallet", spin.transform, Prims.Sphere, GameMaterials.RubyJewel, new Vector3(0.22f, 0.12f, -0.06f), Vector3.one * 0.12f);
                    Prims.Make("Tint", spin.transform, Prims.Sphere, GameMaterials.Glow(spec.color), new Vector3(-0.2f, -0.1f, -0.06f), Vector3.one * 0.1f);
                    pv.spin = spin.transform;
                    pv.spinSpeed = 6f; // one turn a minute
                    Marker(pv, spec.color, new Vector3(0.7f, 0.6f, -0.2f));
                    Sphere(pv.go, Vector3.zero, 0.6f);
                    pv.aim = new Vector3(0f, 0f, -0.2f);
                    break;
                }
                case PartType.MainBarrel:
                {
                    PartView pv = NewPart(i, "MainspringBarrel" + spec.slot, centre + new Vector3(spec.slot == 0 ? -0.7f : 0.7f, -0.55f, 0.12f));
                    Transform t = pv.go.transform;
                    GameObject spin = new GameObject("BarrelSpin");
                    spin.transform.SetParent(t, false);
                    WatchRing(spin.transform, 0.36f, 16, GameMaterials.Brass, 0.07f, 0.1f, GameMaterials.Copper);
                    for (int k = 0; k < 4; k++)
                        Prims.Make("Spoke" + k, spin.transform, Prims.Cube, GameMaterials.Gold, Vector3.zero, new Vector3(0.7f, 0.05f, 0.05f), new Vector3(0f, 0f, k * 45f));
                    Prims.Make("Coil", spin.transform, Prims.Cylinder, GameMaterials.Copper, new Vector3(0f, 0f, 0.03f), new Vector3(0.38f, 0.03f, 0.38f), new Vector3(90f, 0f, 0f));
                    Prims.Make("Tint", spin.transform, Prims.Sphere, GameMaterials.Glow(spec.color), new Vector3(0f, 0f, -0.06f), Vector3.one * 0.12f);
                    pv.spin = spin.transform;
                    pv.spinSpeed = 14f;
                    Marker(pv, spec.color, new Vector3(0f, 0.55f, -0.2f));
                    Sphere(pv.go, Vector3.zero, 0.42f);
                    break;
                }
                case PartType.Rotor:
                {
                    PartView pv = NewPart(i, "GoldMicroRotor", centre + new Vector3(0f, -0.2f, 0.2f));
                    Transform t = pv.go.transform;
                    GameObject spin = new GameObject("RotorSpin");
                    spin.transform.SetParent(t, false);
                    Prims.Make("Weight", spin.transform, Prims.Sphere, GameMaterials.Gold, new Vector3(0.35f, 0f, 0f), new Vector3(0.9f, 0.5f, 0.1f));
                    Prims.Make("Hub", spin.transform, Prims.Cylinder, GameMaterials.Steel, Vector3.zero, new Vector3(0.16f, 0.05f, 0.16f), new Vector3(90f, 0f, 0f));
                    Prims.Make("Tint", spin.transform, Prims.Sphere, GameMaterials.Glow(spec.color), new Vector3(0.55f, 0f, -0.06f), Vector3.one * 0.1f);
                    pv.spin = spin.transform;
                    pv.spinSpeed = 40f;
                    Marker(pv, spec.color, new Vector3(0.5f, 0.55f, -0.2f));
                    Sphere(pv.go, Vector3.zero, 0.5f);
                    break;
                }
                case PartType.Tnt:
                    BuildDynamiteBundle(i, spec, 0f, 0f, 0f);
                    views[i].go.transform.localPosition = centre + new Vector3(spec.slot == 0 ? 0f : 0.35f, spec.slot == 0 ? -0.1f : -0.3f, 0.25f);
                    views[i].go.transform.localScale = Vector3.one * 0.7f;
                    break;
                default:
                {
                    PartView pv = NewPart(i, "Part" + i, centre);
                    Prims.Make("Block", pv.go.transform, Prims.Cube, mat, Vector3.zero, Vector3.one * 0.5f);
                    Marker(pv, spec.color, new Vector3(0f, 0.5f, 0f));
                    Box(pv.go, Vector3.zero, Vector3.one * 0.5f);
                    break;
                }
            }
        }

        // ------------------------------------------------------------------ chapter 9: celestial brass orrery

        private static void RingOfBoxes(Transform parent, float radius, int n, Material mat, float width, float height, Material tick)
        {
            for (int k = 0; k < n; k++)
            {
                float a = k * 360f / n;
                float rad = a * Mathf.Deg2Rad;
                Vector3 p = new Vector3(Mathf.Cos(rad) * radius, 0f, Mathf.Sin(rad) * radius);
                float seg = 2f * Mathf.PI * radius / n * 1.12f;
                Prims.Make("Seg" + k, parent, Prims.Cube, (k % 2 == 0 || tick == null) ? mat : tick, p, new Vector3(seg, height, width), new Vector3(0f, -a - 90f, 0f));
            }
        }

        private void BuildOrreryDecor()
        {
            Prims.Make("MahoganyBase", pivot, Prims.Cylinder, GameMaterials.Mahogany, new Vector3(0f, 0.25f, 0.05f), new Vector3(3.5f, 0.5f, 3.5f));
            Prims.Make("BaseTrim", pivot, Prims.Cylinder, GameMaterials.Brass, new Vector3(0f, 0.52f, 0.05f), new Vector3(3.3f, 0.06f, 3.3f));
            Prims.Make("Column", pivot, Prims.Cylinder, GameMaterials.Brass, new Vector3(0f, 1.15f, 0.05f), new Vector3(0.3f, 1.3f, 0.3f));
            Prims.Make("ColumnCollar", pivot, Prims.Cylinder, GameMaterials.Copper, new Vector3(0f, 0.7f, 0.05f), new Vector3(0.6f, 0.12f, 0.6f));
            Prims.Make("Crank", pivot, Prims.Cube, GameMaterials.Steel, new Vector3(1.55f, 0.62f, 0.05f), new Vector3(0.7f, 0.07f, 0.07f));
            Prims.Make("CrankKnob", pivot, Prims.Sphere, GameMaterials.Bakelite, new Vector3(1.9f, 0.7f, 0.05f), Vector3.one * 0.2f);
            for (int k = 0; k < 4; k++)
                Prims.Make("Foot" + k, pivot, Prims.Sphere, GameMaterials.Brass, new Vector3(k < 2 ? -1.3f : 1.3f, 0.08f, k % 2 == 0 ? -1.1f : 1.2f), Vector3.one * 0.3f);
        }

        private void BuildOrreryPart(int i, PartSpec spec, float lx, float ly)
        {
            Material mat = GameMaterials.ForBlock(spec.color);
            Vector3 centre = new Vector3(0f, 2.1f, 0.05f);
            switch (spec.type)
            {
                case PartType.Ring:
                {
                    PartView pv = NewPart(i, "ZodiacRing" + spec.slot, centre);
                    Transform t = pv.go.transform;
                    float r = 1.45f - spec.slot * 0.17f;
                    GameObject ring = new GameObject("Hoop");
                    ring.transform.SetParent(t, false);
                    // slot 0: tilted zodiac hoop, 1: meridian, 2: equator, 3: inner solstice hoop
                    Vector3[] tilts = { new Vector3(70f, 0f, 20f), new Vector3(0f, 0f, 90f), new Vector3(8f, 0f, 0f), new Vector3(90f, 35f, 0f) };
                    ring.transform.localRotation = Quaternion.Euler(tilts[spec.slot % 4]);
                    RingOfBoxes(ring.transform, r, 30, spec.slot % 2 == 0 ? GameMaterials.Brass : GameMaterials.Copper, 0.1f, 0.07f, GameMaterials.Gunmetal);
                    for (int k = 0; k < 4; k++)
                    {
                        float a = k * 90f * Mathf.Deg2Rad;
                        Prims.Make("Jewel" + k, ring.transform, Prims.Sphere, GameMaterials.Glow(spec.color), new Vector3(Mathf.Cos(a) * r, 0.04f, Mathf.Sin(a) * r), Vector3.one * 0.12f);
                    }
                    Marker(pv, spec.color, new Vector3(r * 0.85f, r * 0.55f, -r * 0.6f));
                    Sphere(pv.go, Vector3.zero, r * 0.45f);
                    pv.aim = spec.slot >= 2 ? new Vector3(0f, 0f, r * 0.6f) : new Vector3(0f, 0.1f, -r * 0.8f);
                    break;
                }
                case PartType.Planet:
                {
                    PartView pv = NewPart(i, "OrbitingPlanet" + spec.slot, centre);
                    Transform t = pv.go.transform;
                    GameObject spin = new GameObject("GovSpin");
                    spin.transform.SetParent(t, false);
                    spin.transform.localRotation = Quaternion.Euler(0f, spec.slot * 90f, 0f);
                    float orbit = 0.42f + spec.slot * 0.17f;
                    float size = 0.34f - spec.slot * 0.03f;
                    Prims.Make("GearArm", spin.transform, Prims.Cube, GameMaterials.Brass, new Vector3(orbit * 0.5f, 0f, 0f), new Vector3(orbit, 0.04f, 0.05f));
                    GameObject planet = Prims.Make("Planet", spin.transform, Prims.Sphere, mat, new Vector3(orbit, 0f, 0f), Vector3.one * size);
                    Prims.Make("Band", planet.transform, Prims.Cylinder, GameMaterials.Brass, Vector3.zero, new Vector3(1.5f, 0.05f, 1.5f), new Vector3(70f, 0f, 20f));
                    Prims.Make("Gem", planet.transform, Prims.Sphere, GameMaterials.Glow(spec.color), new Vector3(0f, 0.5f, 0f), Vector3.one * 0.35f);
                    pv.spin = spin.transform;
                    pv.spinSpeed = 60f - spec.slot * 11f;
                    pv.aimT = planet.transform;
                    Marker(pv, spec.color, new Vector3(0f, 0.7f, -0.7f));
                    Sphere(pv.go, Vector3.zero, 0.5f);
                    break;
                }
                case PartType.Cage:
                {
                    PartView pv = NewPart(i, "SunGearCage", centre);
                    Transform t = pv.go.transform;
                    for (int k = 0; k < 3; k++)
                    {
                        GameObject ring = new GameObject("CageRing" + k);
                        ring.transform.SetParent(t, false);
                        ring.transform.localRotation = Quaternion.Euler(k * 60f, k * 45f, 0f);
                        RingOfBoxes(ring.transform, 0.58f, 20, GameMaterials.Brass, 0.06f, 0.05f, GameMaterials.Steel);
                    }
                    Prims.Make("Gear", t, Prims.Cylinder, GameMaterials.Copper, Vector3.zero, new Vector3(0.5f, 0.03f, 0.5f), new Vector3(90f, 0f, 0f));
                    Prims.Make("Glow", t, Prims.Sphere, GameMaterials.Glow(spec.color), Vector3.zero, Vector3.one * 0.3f);
                    Marker(pv, spec.color, new Vector3(0.6f, 0.6f, -0.6f));
                    Sphere(pv.go, Vector3.zero, 0.6f);
                    pv.aim = new Vector3(0f, 0f, -0.45f);
                    break;
                }
                case PartType.Tnt:
                    BuildDynamiteBundle(i, spec, 0f, 0f, 0f);
                    views[i].go.transform.localPosition = centre + new Vector3(spec.slot == 0 ? 0f : 0.25f, spec.slot == 0 ? 0f : -0.12f, -0.05f);
                    views[i].go.transform.localScale = Vector3.one * 0.7f;
                    break;
                default:
                {
                    PartView pv = NewPart(i, "Part" + i, centre);
                    Prims.Make("Block", pv.go.transform, Prims.Cube, mat, Vector3.zero, Vector3.one * 0.5f);
                    Marker(pv, spec.color, new Vector3(0f, 0.5f, 0f));
                    Box(pv.go, Vector3.zero, Vector3.one * 0.5f);
                    break;
                }
            }
        }

        // ------------------------------------------------------------------ chapter 8: vintage brass-horn gramophone

        private void BuildGramophoneDecor()
        {
            // mahogany plinth, brass crank and the swinging tonearm resting over the record
            Prims.Make("Plinth", pivot, Prims.Cube, GameMaterials.Mahogany, new Vector3(0f, 0.22f, 0.1f), new Vector3(3.3f, 0.44f, 2.3f));
            Prims.Make("PlinthTrim", pivot, Prims.Cube, GameMaterials.Brass, new Vector3(0f, 0.46f, 0.1f), new Vector3(3.4f, 0.06f, 2.4f));
            Prims.Make("CrankShaft", pivot, Prims.Cylinder, GameMaterials.Brass, new Vector3(1.78f, 0.35f, 0.1f), new Vector3(0.1f, 0.2f, 0.1f), new Vector3(0f, 0f, 90f));
            Prims.Make("CrankArm", pivot, Prims.Cube, GameMaterials.Steel, new Vector3(2.0f, 0.5f, 0.1f), new Vector3(0.1f, 0.32f, 0.08f));
            Prims.Make("CrankKnob", pivot, Prims.Sphere, GameMaterials.Bakelite, new Vector3(2.0f, 0.7f, 0.1f), Vector3.one * 0.16f);
            Prims.Make("ArmPost", pivot, Prims.Cylinder, GameMaterials.Brass, new Vector3(1.2f, 0.7f, -0.7f), new Vector3(0.14f, 0.2f, 0.14f));
            Prims.Make("ArmBar", pivot, Prims.Cube, GameMaterials.Steel, new Vector3(0.65f, 0.95f, -0.62f), new Vector3(1.3f, 0.06f, 0.06f), new Vector3(0f, 18f, 0f));
            Prims.Make("ArmHead", pivot, Prims.Cube, GameMaterials.Brass, new Vector3(0.05f, 0.95f, -0.45f), new Vector3(0.2f, 0.12f, 0.16f));
            Prims.Make("Needle", pivot, Prims.Cylinder, GameMaterials.Steel, new Vector3(0.05f, 0.85f, -0.45f), new Vector3(0.02f, 0.1f, 0.02f));
        }

        private void BuildGramophonePart(int i, PartSpec spec, float lx, float ly)
        {
            Material mat = GameMaterials.ForBlock(spec.color);
            switch (spec.type)
            {
                case PartType.Horn:
                {
                    bool bell = spec.slot == 0;
                    PartView pv = NewPart(i, bell ? "BrassMorningGloryHorn" : "HornRimBand", new Vector3(lx, ly, 0.1f));
                    Transform t = pv.go.transform;
                    if (bell)
                    {
                        // flared horn: stacked rings growing from the neck to a wide mouth, tilted towards the viewer
                        GameObject flare = new GameObject("Flare");
                        flare.transform.SetParent(t, false);
                        flare.transform.localRotation = Quaternion.Euler(-62f, 0f, 0f);
                        for (int k = 0; k < 9; k++)
                        {
                            float f = k / 8f;
                            float r = 0.18f + 1.05f * f * f;
                            Prims.Make("Ring" + k, flare.transform, Prims.Cylinder, k % 2 == 0 ? GameMaterials.Brass : GameMaterials.Copper, new Vector3(0f, -0.9f + f * 1.7f, 0f), new Vector3(r * 2f, 0.1f, r * 2f));
                        }
                        Prims.Make("Tint", flare.transform, Prims.Cylinder, GameMaterials.Glow(spec.color), new Vector3(0f, 0.86f, 0f), new Vector3(1.3f, 0.03f, 1.3f));
                        Marker(pv, spec.color, new Vector3(0.9f, 0.9f, -0.6f));
                        Sphere(pv.go, new Vector3(0f, 0.2f, -0.5f), 1.0f);
                        pv.aim = new Vector3(0f, 0.3f, -0.8f);
                    }
                    else
                    {
                        GameObject band = new GameObject("Band");
                        band.transform.SetParent(t, false);
                        band.transform.localRotation = Quaternion.Euler(-62f, 0f, 0f);
                        Prims.Make("Lip", band.transform, Prims.Cylinder, mat, new Vector3(0f, -0.1f, 0f), new Vector3(2.6f, 0.07f, 2.6f));
                        Prims.Make("LipBrass", band.transform, Prims.Cylinder, GameMaterials.Brass, new Vector3(0f, -0.15f, 0f), new Vector3(2.68f, 0.05f, 2.68f));
                        Marker(pv, spec.color, new Vector3(-1.1f, 0.5f, -0.6f));
                        Sphere(pv.go, new Vector3(0f, 0f, -0.6f), 0.9f);
                        pv.aim = new Vector3(0f, 0f, -0.8f);
                    }
                    break;
                }
                case PartType.Bracket:
                {
                    PartView pv = NewPart(i, "NickelElbowBracket", new Vector3(lx, ly, 0.2f));
                    Transform t = pv.go.transform;
                    Prims.Make("Neck", t, Prims.Cylinder, GameMaterials.Steel, Vector3.zero, new Vector3(0.34f, 0.8f, 0.34f));
                    Prims.Make("Elbow", t, Prims.Sphere, GameMaterials.Steel, new Vector3(0f, 0.45f, 0f), Vector3.one * 0.44f);
                    Prims.Make("Collar", t, Prims.Cylinder, GameMaterials.Brass, new Vector3(0f, -0.2f, 0f), new Vector3(0.5f, 0.08f, 0.5f));
                    Prims.Make("Tint", t, Prims.Sphere, GameMaterials.Glow(spec.color), new Vector3(0f, 0.45f, -0.22f), Vector3.one * 0.12f);
                    Marker(pv, spec.color, new Vector3(0.55f, 0.5f, -0.2f));
                    Sphere(pv.go, Vector3.zero, 0.5f);
                    break;
                }
                case PartType.Record:
                {
                    bool outer = spec.slot == 0;
                    PartView pv = NewPart(i, outer ? "ShellacRecord" : "RecordLabelCore", new Vector3(lx, ly, -0.05f));
                    Transform t = pv.go.transform;
                    GameObject spin = new GameObject("RecordSpin");
                    spin.transform.SetParent(t, false);
                    spin.transform.localRotation = Quaternion.Euler(-22f, 0f, 0f);
                    GameObject disc = new GameObject("Disc");
                    disc.transform.SetParent(spin.transform, false);
                    if (outer)
                    {
                        Prims.Make("Vinyl", disc.transform, Prims.Cylinder, GameMaterials.Bakelite, Vector3.zero, new Vector3(2.5f, 0.05f, 2.5f));
                        for (int g = 1; g < 5; g++)
                            Prims.Make("Groove" + g, disc.transform, Prims.Cylinder, GameMaterials.Charcoal, new Vector3(0f, 0.027f, 0f), new Vector3(2.5f - g * 0.42f, 0.01f, 2.5f - g * 0.42f));
                        Prims.Make("Label", disc.transform, Prims.Cylinder, GameMaterials.TntRed, new Vector3(0f, 0.04f, 0f), new Vector3(0.78f, 0.03f, 0.78f));
                        Prims.Make("Tint", disc.transform, Prims.Cylinder, GameMaterials.Glow(spec.color), new Vector3(0f, 0.058f, 0f), new Vector3(0.3f, 0.02f, 0.3f));
                        Prims.Make("Spindle", disc.transform, Prims.Cylinder, GameMaterials.Steel, new Vector3(0f, 0.08f, 0f), new Vector3(0.06f, 0.1f, 0.06f));
                        pv.spin = disc.transform;
                        pv.spinSpeed = 100f;
                    }
                    else
                    {
                        Prims.Make("Ring", disc.transform, Prims.Cylinder, GameMaterials.Brass, new Vector3(0f, 0.09f, 0f), new Vector3(0.95f, 0.04f, 0.95f));
                        Prims.Make("Tint", disc.transform, Prims.Cylinder, GameMaterials.Glow(spec.color), new Vector3(0f, 0.12f, 0f), new Vector3(0.6f, 0.02f, 0.6f));
                        pv.spin = disc.transform;
                        pv.spinSpeed = -60f;
                    }
                    Marker(pv, spec.color, new Vector3(1.0f, 0.4f, -0.4f));
                    Sphere(pv.go, new Vector3(0f, 0f, 0f), 1.2f);
                    pv.aim = new Vector3(0f, 0.3f, -0.6f);
                    break;
                }
                case PartType.Governor:
                {
                    PartView pv = NewPart(i, "SpeedGovernor", new Vector3(lx, ly, -0.3f));
                    Transform t = pv.go.transform;
                    GameObject spin = new GameObject("GovSpin");
                    spin.transform.SetParent(t, false);
                    for (int k = 0; k < 3; k++)
                    {
                        float a = k * 120f * Mathf.Deg2Rad;
                        Vector3 p = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * 0.34f;
                        Prims.Make("Arm" + k, spin.transform, Prims.Cube, GameMaterials.Steel, p * 0.5f, new Vector3(0.34f, 0.04f, 0.05f), new Vector3(0f, -k * 120f, 0f));
                        Prims.Make("Weight" + k, spin.transform, Prims.Sphere, GameMaterials.Brass, p, Vector3.one * 0.2f);
                    }
                    Prims.Make("Hub", spin.transform, Prims.Sphere, GameMaterials.Glow(spec.color), Vector3.zero, Vector3.one * 0.18f);
                    pv.spin = spin.transform;
                    pv.spinSpeed = 220f;
                    Marker(pv, spec.color, new Vector3(0f, 0.5f, -0.2f));
                    Sphere(pv.go, Vector3.zero, 0.42f);
                    break;
                }
                case PartType.Cabinet:
                {
                    bool outer = spec.slot == 0;
                    PartView pv = NewPart(i, outer ? "MahoganyCabinetFront" : "MahoganyCabinetSide", new Vector3(lx, ly, -0.7f));
                    Transform t = pv.go.transform;
                    if (outer)
                    {
                        Prims.Make("Panel", t, Prims.Cube, GameMaterials.Mahogany, Vector3.zero, new Vector3(3.1f, 0.9f, 0.1f));
                        Prims.Make("InsetA", t, Prims.Cube, GameMaterials.Bakelite, new Vector3(-0.8f, 0f, -0.06f), new Vector3(1.2f, 0.55f, 0.03f));
                        Prims.Make("InsetB", t, Prims.Cube, GameMaterials.Bakelite, new Vector3(0.8f, 0f, -0.06f), new Vector3(1.2f, 0.55f, 0.03f));
                        Prims.Make("Handle", t, Prims.Sphere, GameMaterials.Brass, new Vector3(0f, 0f, -0.09f), Vector3.one * 0.14f);
                        for (int sx = -1; sx <= 1; sx += 2)
                            Prims.Make("Dovetail" + sx, t, Prims.Cube, GameMaterials.Brass, new Vector3(sx * 1.52f, 0f, -0.05f), new Vector3(0.08f, 0.9f, 0.1f));
                    }
                    else
                    {
                        Prims.Make("Panel", t, Prims.Cube, GameMaterials.Mahogany, Vector3.zero, new Vector3(0.1f, 0.8f, 1.9f));
                        Prims.Make("Grille", t, Prims.Cube, GameMaterials.Rubber, new Vector3(-0.06f, 0f, 0f), new Vector3(0.03f, 0.5f, 1.4f));
                    }
                    Marker(pv, spec.color, new Vector3(outer ? 1.25f : 0.3f, 0.55f, -0.3f));
                    Box(pv.go, Vector3.zero, outer ? new Vector3(3.1f, 0.9f, 0.3f) : new Vector3(0.3f, 0.8f, 1.9f));
                    pv.aim = new Vector3(0f, 0f, -0.3f);
                    break;
                }
                case PartType.Mainspring:
                {
                    PartView pv = NewPart(i, "MainspringBarrel", new Vector3(lx, ly, 0.0f));
                    Transform t = pv.go.transform;
                    Prims.Make("Drum", t, Prims.Cylinder, GameMaterials.Brass, Vector3.zero, new Vector3(0.95f, 0.45f, 0.95f), new Vector3(90f, 0f, 0f));
                    for (int k = 0; k < 4; k++)
                        Prims.Make("Coil" + k, t, Prims.Cylinder, GameMaterials.Copper, new Vector3(0f, 0f, -0.24f - k * 0.01f), new Vector3(0.8f - k * 0.17f, 0.02f, 0.8f - k * 0.17f), new Vector3(90f, 0f, 0f));
                    Prims.Make("Tint", t, Prims.Sphere, GameMaterials.Glow(spec.color), new Vector3(0f, 0f, -0.3f), Vector3.one * 0.14f);
                    Marker(pv, spec.color, new Vector3(0f, 0.6f, -0.3f));
                    Sphere(pv.go, Vector3.zero, 0.5f);
                    break;
                }
                case PartType.Tnt:
                    BuildDynamiteBundle(i, spec, lx, ly, -0.35f);
                    break;
                default:
                {
                    PartView pv = NewPart(i, "Part" + i, new Vector3(lx, ly, 0f));
                    Prims.Make("Block", pv.go.transform, Prims.Cube, mat, Vector3.zero, Vector3.one * 0.5f);
                    Marker(pv, spec.color, new Vector3(0f, 0.5f, 0f));
                    Box(pv.go, Vector3.zero, Vector3.one * 0.5f);
                    break;
                }
            }
        }

        // ------------------------------------------------------------------ chapter 7: vintage mechanical typewriter

        private void BuildTypewriterDecor()
        {
            // cast-iron chassis: black-enamel body with brass trim and a sloping key deck
            Prims.Make("Chassis", pivot, Prims.Cube, GameMaterials.Bakelite, new Vector3(0f, 0.55f, 0.15f), new Vector3(3.6f, 0.7f, 1.9f));
            Prims.Make("ChassisTrim", pivot, Prims.Cube, GameMaterials.Brass, new Vector3(0f, 0.2f, 0.15f), new Vector3(3.7f, 0.1f, 2.0f));
            Prims.Make("RearWall", pivot, Prims.Cube, GameMaterials.Bakelite, new Vector3(0f, 1.6f, 0.95f), new Vector3(3.5f, 1.8f, 0.14f));
            Prims.Make("SideL", pivot, Prims.Cube, GameMaterials.Bakelite, new Vector3(-1.72f, 1.5f, 0.2f), new Vector3(0.14f, 1.5f, 1.6f));
            Prims.Make("SideR", pivot, Prims.Cube, GameMaterials.Bakelite, new Vector3(1.72f, 1.5f, 0.2f), new Vector3(0.14f, 1.5f, 1.6f));
            Prims.Make("Nameplate", pivot, Prims.Cube, GameMaterials.Gold, new Vector3(0f, 0.62f, -0.84f), new Vector3(1.1f, 0.12f, 0.03f));
            for (int k = 0; k < 4; k++)
                Prims.Make("Foot" + k, pivot, Prims.Sphere, GameMaterials.Brass, new Vector3(k < 2 ? -1.55f : 1.55f, 0.08f, k % 2 == 0 ? -0.7f : 0.9f), Vector3.one * 0.26f);
        }

        private void BuildTypewriterPart(int i, PartSpec spec, float lx, float ly)
        {
            Material mat = GameMaterials.ForBlock(spec.color);
            switch (spec.type)
            {
                case PartType.Hood:
                {
                    PartView pv = NewPart(i, "CastIronHood", new Vector3(lx, ly, 0.1f));
                    Transform t = pv.go.transform;
                    Prims.Make("Lid", t, Prims.Cube, GameMaterials.Bakelite, new Vector3(0f, 0.3f, 0f), new Vector3(3.4f, 0.22f, 1.75f));
                    Prims.Make("Ridge", t, Prims.Cube, GameMaterials.Gunmetal, new Vector3(0f, 0.43f, 0f), new Vector3(3.0f, 0.05f, 0.5f));
                    Prims.Make("Front", t, Prims.Cube, GameMaterials.Bakelite, new Vector3(0f, 0.0f, -0.82f), new Vector3(3.4f, 0.7f, 0.12f));
                    Prims.Make("RimF", t, Prims.Cube, GameMaterials.Brass, new Vector3(0f, 0.42f, -0.88f), new Vector3(3.5f, 0.07f, 0.07f));
                    Prims.Make("RimB", t, Prims.Cube, GameMaterials.Brass, new Vector3(0f, 0.42f, 0.88f), new Vector3(3.5f, 0.07f, 0.07f));
                    Prims.Make("RimL", t, Prims.Cube, GameMaterials.Brass, new Vector3(-1.72f, 0.42f, 0f), new Vector3(0.07f, 0.07f, 1.85f));
                    Prims.Make("RimR", t, Prims.Cube, GameMaterials.Brass, new Vector3(1.72f, 0.42f, 0f), new Vector3(0.07f, 0.07f, 1.85f));
                    Prims.Make("Tint", t, Prims.Cube, GameMaterials.Glow(spec.color), new Vector3(0f, 0.0f, -0.89f), new Vector3(1.4f, 0.14f, 0.02f));
                    Marker(pv, spec.color, new Vector3(1.3f, 0.5f, -0.9f));
                    Box(pv.go, Vector3.zero, new Vector3(3.4f, 0.9f, 1.75f));
                    pv.aim = new Vector3(0f, 0f, -0.5f);
                    break;
                }
                case PartType.Platen:
                {
                    PartView pv = NewPart(i, "PlatenRoller", new Vector3(lx, ly, 0.55f));
                    Transform t = pv.go.transform;
                    GameObject roller = new GameObject("Roller");
                    roller.transform.SetParent(t, false);
                    Prims.Make("Rubber", roller.transform, Prims.Cylinder, GameMaterials.Rubber, Vector3.zero, new Vector3(0.46f, 1.55f, 0.46f), new Vector3(0f, 0f, 90f));
                    Prims.Make("KnobL", roller.transform, Prims.Cylinder, GameMaterials.Brass, new Vector3(-1.62f, 0f, 0f), new Vector3(0.3f, 0.08f, 0.3f), new Vector3(0f, 0f, 90f));
                    Prims.Make("KnobR", roller.transform, Prims.Cylinder, GameMaterials.Brass, new Vector3(1.62f, 0f, 0f), new Vector3(0.3f, 0.08f, 0.3f), new Vector3(0f, 0f, 90f));
                    Prims.Make("Tint", roller.transform, Prims.Cylinder, GameMaterials.Glow(spec.color), new Vector3(0f, 0.2f, -0.18f), new Vector3(0.12f, 0.7f, 0.12f), new Vector3(0f, 0f, 90f));
                    Marker(pv, spec.color, new Vector3(0f, 0.55f, -0.3f));
                    Box(pv.go, Vector3.zero, new Vector3(3.4f, 0.5f, 0.5f));
                    pv.aim = new Vector3(0f, 0f, -0.4f);
                    break;
                }
                case PartType.Lever:
                {
                    PartView pv = NewPart(i, "CarriageReturnLever", new Vector3(lx, ly, 0.2f));
                    Transform t = pv.go.transform;
                    Prims.Make("Pivot", t, Prims.Sphere, GameMaterials.Brass, Vector3.zero, Vector3.one * 0.28f);
                    Prims.Make("Arm", t, Prims.Cube, GameMaterials.Steel, new Vector3(-0.3f, 0.35f, 0f), new Vector3(0.1f, 0.95f, 0.1f), new Vector3(0f, 0f, 28f));
                    Prims.Make("Grip", t, Prims.Sphere, GameMaterials.Bakelite, new Vector3(-0.58f, 0.75f, 0f), Vector3.one * 0.26f);
                    Prims.Make("Tint", t, Prims.Sphere, GameMaterials.Glow(spec.color), new Vector3(-0.58f, 0.75f, -0.13f), Vector3.one * 0.09f);
                    Marker(pv, spec.color, new Vector3(-0.5f, 1.15f, -0.1f));
                    Sphere(pv.go, new Vector3(-0.3f, 0.35f, 0f), 0.5f);
                    break;
                }
                case PartType.Keys:
                {
                    PartView pv = NewPart(i, "RoundKeys", new Vector3(lx, ly, -0.35f));
                    Transform t = pv.go.transform;
                    GameObject field = new GameObject("KeyField");
                    field.transform.SetParent(t, false);
                    field.transform.localRotation = Quaternion.Euler(-18f, 0f, 0f);
                    for (int row = 0; row < 3; row++)
                        for (int col = 0; col < 9 - row; col++)
                        {
                            Vector3 p = new Vector3((col - (8 - row) * 0.5f) * 0.34f, 0f, row * 0.3f);
                            Prims.Make("Stem" + row + col, field.transform, Prims.Cylinder, GameMaterials.Steel, p + new Vector3(0f, -0.08f, 0f), new Vector3(0.06f, 0.1f, 0.06f));
                            Prims.Make("Rim" + row + col, field.transform, Prims.Cylinder, GameMaterials.Brass, p, new Vector3(0.28f, 0.05f, 0.28f));
                            Prims.Make("Cap" + row + col, field.transform, Prims.Cylinder, row == 1 ? GameMaterials.Glow(spec.color) : GameMaterials.Porcelain, p + new Vector3(0f, 0.04f, 0f), new Vector3(0.21f, 0.03f, 0.21f));
                        }
                    Marker(pv, spec.color, new Vector3(1.55f, 0.35f, -0.2f));
                    Box(pv.go, new Vector3(0f, 0.05f, 0.3f), new Vector3(3.2f, 0.5f, 1.1f));
                    pv.aim = new Vector3(0f, 0.2f, -0.2f);
                    break;
                }
                case PartType.Typebar:
                {
                    PartView pv = NewPart(i, "TypebarBasket", new Vector3(lx, ly, 0.35f));
                    Transform t = pv.go.transform;
                    GameObject fan = new GameObject("Fan");
                    fan.transform.SetParent(t, false);
                    for (int k = 0; k < 11; k++)
                    {
                        float a = (k - 5) * 11f;
                        GameObject bar = Prims.Make("Bar" + k, fan.transform, Prims.Cube, GameMaterials.Steel, new Vector3(Mathf.Sin(a * Mathf.Deg2Rad) * 0.9f, 0.1f, -Mathf.Cos(a * Mathf.Deg2Rad) * 0.25f), new Vector3(0.05f, 0.7f, 0.04f), new Vector3(0f, a, 0f));
                        Prims.Make("Slug", bar.transform, Prims.Cube, GameMaterials.Brass, new Vector3(0f, 0.5f, 0f), new Vector3(2.4f, 0.3f, 2.4f));
                    }
                    Prims.Make("Ring", t, Prims.Cylinder, GameMaterials.Gunmetal, new Vector3(0f, -0.2f, 0f), new Vector3(2.0f, 0.06f, 1.0f));
                    Prims.Make("Tint", t, Prims.Cylinder, GameMaterials.Glow(spec.color), new Vector3(0f, -0.15f, -0.5f), new Vector3(0.4f, 0.03f, 0.4f));
                    Marker(pv, spec.color, new Vector3(1.2f, 0.7f, -0.2f));
                    Box(pv.go, Vector3.zero, new Vector3(2.4f, 0.9f, 1.0f));
                    pv.aim = new Vector3(0f, 0f, -0.5f);
                    break;
                }
                case PartType.Ribbon:
                {
                    PartView pv = NewPart(i, "InkRibbonSpool" + spec.slot, new Vector3(lx, ly, 0.0f));
                    Transform t = pv.go.transform;
                    GameObject spin = new GameObject("SpoolSpin");
                    spin.transform.SetParent(t, false);
                    Prims.Make("FlangeA", spin.transform, Prims.Cylinder, GameMaterials.Brass, new Vector3(0f, 0.12f, 0f), new Vector3(0.7f, 0.03f, 0.7f));
                    Prims.Make("FlangeB", spin.transform, Prims.Cylinder, GameMaterials.Brass, new Vector3(0f, -0.12f, 0f), new Vector3(0.7f, 0.03f, 0.7f));
                    Prims.Make("Cloth", spin.transform, Prims.Cylinder, spec.slot == 0 ? GameMaterials.TntRed : GameMaterials.Rubber, Vector3.zero, new Vector3(0.58f, 0.22f, 0.58f));
                    Prims.Make("Core", spin.transform, Prims.Cylinder, GameMaterials.Steel, Vector3.zero, new Vector3(0.14f, 0.26f, 0.14f));
                    pv.spin = spin.transform;
                    pv.spinSpeed = 45f;
                    Marker(pv, spec.color, new Vector3(0f, 0.55f, -0.2f));
                    Sphere(pv.go, Vector3.zero, 0.42f);
                    break;
                }
                case PartType.Rack:
                {
                    PartView pv = NewPart(i, "EscapementRack", new Vector3(lx, ly, 0.1f));
                    Transform t = pv.go.transform;
                    Prims.Make("Bar", t, Prims.Cube, GameMaterials.Brass, Vector3.zero, new Vector3(2.6f, 0.12f, 0.2f));
                    for (int k = 0; k < 17; k++)
                        Prims.Make("Tooth" + k, t, Prims.Cube, GameMaterials.Brass, new Vector3(-1.2f + k * 0.15f, 0.12f, 0f), new Vector3(0.07f, 0.14f, 0.16f));
                    Prims.Make("Pawl", t, Prims.Cube, GameMaterials.Steel, new Vector3(0.2f, 0.28f, 0f), new Vector3(0.1f, 0.3f, 0.1f), new Vector3(0f, 0f, 20f));
                    Marker(pv, spec.color, new Vector3(0f, 0.55f, -0.2f));
                    Box(pv.go, Vector3.zero, new Vector3(2.6f, 0.5f, 0.4f));
                    break;
                }
                case PartType.Spring:
                {
                    PartView pv = NewPart(i, "TensionSpring" + spec.slot, new Vector3(lx, ly, 0.0f));
                    Transform t = pv.go.transform;
                    for (int k = 0; k < 6; k++)
                        Prims.Make("Coil" + k, t, Prims.Cylinder, GameMaterials.Copper, new Vector3(0f, -0.3f + k * 0.12f, 0f), new Vector3(0.42f, 0.03f, 0.42f), new Vector3(0f, k * 30f, 0f));
                    Prims.Make("Hook", t, Prims.Sphere, GameMaterials.Steel, new Vector3(0f, 0.5f, 0f), Vector3.one * 0.12f);
                    Marker(pv, spec.color, new Vector3(0f, 0.7f, -0.15f));
                    Sphere(pv.go, Vector3.zero, 0.36f);
                    break;
                }
                case PartType.Tnt:
                    BuildDynamiteBundle(i, spec, lx, ly, -0.1f);
                    break;
                default:
                {
                    PartView pv = NewPart(i, "Part" + i, new Vector3(lx, ly, 0f));
                    Prims.Make("Block", pv.go.transform, Prims.Cube, mat, Vector3.zero, Vector3.one * 0.5f);
                    Marker(pv, spec.color, new Vector3(0f, 0.5f, 0f));
                    Box(pv.go, Vector3.zero, Vector3.one * 0.5f);
                    break;
                }
            }
        }

        // ------------------------------------------------------------------ chapter 6: twin-lens reflex camera

        private void BuildCameraDecor()
        {
            Prims.Make("DarkroomCore", pivot, Prims.Cube, GameMaterials.Charcoal, new Vector3(0f, 1.45f, 0.18f), new Vector3(1.8f, 2.0f, 0.7f));
            Prims.Make("ChromeTop", pivot, Prims.Cube, GameMaterials.Silver, new Vector3(0f, 2.58f, 0.15f), new Vector3(2.05f, 0.1f, 1.25f));
            Prims.Make("ChromeBottom", pivot, Prims.Cube, GameMaterials.Silver, new Vector3(0f, 0.3f, 0.15f), new Vector3(2.05f, 0.14f, 1.25f));
            Prims.Make("LugL", pivot, Prims.Cylinder, GameMaterials.Steel, new Vector3(-1.08f, 2.3f, 0.15f), new Vector3(0.2f, 0.06f, 0.2f), new Vector3(0f, 0f, 90f));
            Prims.Make("LugR", pivot, Prims.Cylinder, GameMaterials.Steel, new Vector3(1.08f, 2.3f, 0.15f), new Vector3(0.2f, 0.06f, 0.2f), new Vector3(0f, 0f, 90f));
            Prims.Make("Plaque", pivot, Prims.Cube, GameMaterials.Gold, new Vector3(0f, 0.55f, -0.64f), new Vector3(0.9f, 0.14f, 0.03f));
        }

        private void BuildCameraPart(int i, PartSpec spec, float lx, float ly)
        {
            Material mat = GameMaterials.ForBlock(spec.color);
            switch (spec.type)
            {
                case PartType.Body:
                {
                    bool front = spec.slot == 0;
                    PartView pv = NewPart(i, front ? "LeatherFrontPlate" : "LeatherBackDoor", new Vector3(lx, ly, front ? -0.5f : 0.78f));
                    Transform t = pv.go.transform;
                    Prims.Make("Leather", t, Prims.Cube, GameMaterials.Bakelite, Vector3.zero, new Vector3(2.0f, 2.1f, 0.12f));
                    Prims.Make("Frame", t, Prims.Cube, GameMaterials.Steel, new Vector3(0f, 1.0f, front ? -0.03f : 0.03f), new Vector3(2.04f, 0.08f, 0.14f));
                    Prims.Make("FrameB", t, Prims.Cube, GameMaterials.Steel, new Vector3(0f, -1.0f, front ? -0.03f : 0.03f), new Vector3(2.04f, 0.08f, 0.14f));
                    Prims.Make("Tint", t, Prims.Cube, GameMaterials.Glow(spec.color), new Vector3(0.8f, -0.75f, front ? -0.07f : 0.07f), new Vector3(0.3f, 0.12f, 0.02f));
                    Marker(pv, spec.color, new Vector3(-0.7f, -0.8f, front ? -0.2f : 0.2f));
                    Box(pv.go, Vector3.zero, new Vector3(2.0f, 2.1f, 0.2f));
                    pv.aim = new Vector3(0f, front ? 0.7f : 0.4f, front ? -0.2f : 0.2f);
                    break;
                }
                case PartType.Hood:
                {
                    PartView pv = NewPart(i, "ViewfinderHood", new Vector3(lx, ly, 0.0f));
                    Transform t = pv.go.transform;
                    Prims.Make("Lid", t, Prims.Cube, GameMaterials.Gunmetal, new Vector3(0f, 0.28f, 0f), new Vector3(1.7f, 0.07f, 1.1f));
                    Prims.Make("Front", t, Prims.Cube, GameMaterials.Charcoal, new Vector3(0f, 0.0f, -0.52f), new Vector3(1.7f, 0.5f, 0.05f), new Vector3(-12f, 0f, 0f));
                    Prims.Make("SideL", t, Prims.Cube, GameMaterials.Charcoal, new Vector3(-0.82f, 0.0f, 0f), new Vector3(0.05f, 0.5f, 1.05f));
                    Prims.Make("SideR", t, Prims.Cube, GameMaterials.Charcoal, new Vector3(0.82f, 0.0f, 0f), new Vector3(0.05f, 0.5f, 1.05f));
                    Prims.Make("Tint", t, Prims.Cube, GameMaterials.Glow(spec.color), new Vector3(0f, 0.33f, -0.3f), new Vector3(0.5f, 0.03f, 0.18f));
                    Marker(pv, spec.color, new Vector3(0.6f, 0.65f, -0.3f));
                    Box(pv.go, Vector3.zero, new Vector3(1.7f, 0.6f, 1.1f));
                    break;
                }
                case PartType.Lens:
                {
                    PartView pv = NewPart(i, spec.slot == 0 ? "ViewingLens" : "TakingLens", new Vector3(lx, ly, -0.62f));
                    Transform t = pv.go.transform;
                    Prims.Make("Ring", t, Prims.Cylinder, GameMaterials.Steel, Vector3.zero, new Vector3(1.0f, 0.16f, 1.0f), new Vector3(90f, 0f, 0f));
                    Prims.Make("Barrel", t, Prims.Cylinder, GameMaterials.Charcoal, new Vector3(0f, 0f, -0.05f), new Vector3(0.78f, 0.12f, 0.78f), new Vector3(90f, 0f, 0f));
                    Prims.Make("Glass", t, Prims.Sphere, GameMaterials.Acrylic(spec.color), new Vector3(0f, 0f, -0.12f), new Vector3(0.66f, 0.66f, 0.18f));
                    Prims.Make("Coating", t, Prims.Sphere, GameMaterials.Glow(spec.color), new Vector3(0.12f, 0.12f, -0.2f), new Vector3(0.12f, 0.12f, 0.02f));
                    Marker(pv, spec.color, new Vector3(0.62f, 0.55f, -0.15f));
                    Sphere(pv.go, Vector3.zero, 0.55f);
                    pv.aim = new Vector3(0f, 0f, -0.3f);
                    break;
                }
                case PartType.Iris:
                {
                    PartView pv = NewPart(i, "IrisShutter", new Vector3(lx, ly, -0.4f));
                    Transform t = pv.go.transform;
                    GameObject spin = new GameObject("BladeSpin");
                    spin.transform.SetParent(t, false);
                    for (int k = 0; k < 8; k++)
                    {
                        float a = k * 45f;
                        Prims.Make("Blade" + k, spin.transform, Prims.Cube, GameMaterials.Steel, new Vector3(Mathf.Cos(a * Mathf.Deg2Rad) * 0.3f, Mathf.Sin(a * Mathf.Deg2Rad) * 0.3f, 0f), new Vector3(0.42f, 0.16f, 0.02f), new Vector3(0f, 0f, a + 25f));
                    }
                    Prims.Make("Rim", t, Prims.Cylinder, GameMaterials.Gunmetal, new Vector3(0f, 0f, 0.02f), new Vector3(1.05f, 0.04f, 1.05f), new Vector3(90f, 0f, 0f));
                    pv.spin = spin.transform;
                    pv.spinSpeed = 25f;
                    Marker(pv, spec.color, new Vector3(0.6f, 0.55f, -0.15f));
                    Sphere(pv.go, Vector3.zero, 0.55f);
                    break;
                }
                case PartType.Crank:
                {
                    PartView pv = NewPart(i, "FilmAdvanceCrank", new Vector3(lx, ly, 0.0f));
                    Transform t = pv.go.transform;
                    Prims.Make("Boss", t, Prims.Cylinder, GameMaterials.Steel, Vector3.zero, new Vector3(0.36f, 0.12f, 0.36f), new Vector3(0f, 0f, 90f));
                    Prims.Make("Arm", t, Prims.Cube, GameMaterials.Steel, new Vector3(0.32f, 0.2f, 0f), new Vector3(0.12f, 0.5f, 0.08f), new Vector3(0f, 0f, -35f));
                    Prims.Make("Knob", t, Prims.Sphere, GameMaterials.Bakelite, new Vector3(0.52f, 0.42f, 0f), Vector3.one * 0.2f);
                    Prims.Make("Tint", t, Prims.Sphere, GameMaterials.Glow(spec.color), new Vector3(0.52f, 0.42f, -0.1f), Vector3.one * 0.07f);
                    Marker(pv, spec.color, new Vector3(0.45f, 0.8f, -0.1f));
                    Sphere(pv.go, new Vector3(0.25f, 0.2f, 0f), 0.45f);
                    break;
                }
                case PartType.Spool:
                {
                    PartView pv = NewPart(i, "FilmSpool" + spec.slot, new Vector3(lx, ly, -0.1f));
                    Transform t = pv.go.transform;
                    GameObject spin = new GameObject("SpoolSpin");
                    spin.transform.SetParent(t, false);
                    Prims.Make("FlangeA", spin.transform, Prims.Cylinder, GameMaterials.Brass, new Vector3(0f, 0f, -0.14f), new Vector3(0.8f, 0.03f, 0.8f), new Vector3(90f, 0f, 0f));
                    Prims.Make("FlangeB", spin.transform, Prims.Cylinder, GameMaterials.Brass, new Vector3(0f, 0f, 0.14f), new Vector3(0.8f, 0.03f, 0.8f), new Vector3(90f, 0f, 0f));
                    Prims.Make("Film", spin.transform, Prims.Cylinder, GameMaterials.Rubber, Vector3.zero, new Vector3(0.6f, 0.13f, 0.6f), new Vector3(90f, 0f, 0f));
                    Prims.Make("Core", spin.transform, Prims.Cylinder, GameMaterials.Steel, Vector3.zero, new Vector3(0.22f, 0.16f, 0.22f), new Vector3(90f, 0f, 0f));
                    Prims.Make("Strip", t, Prims.Cube, GameMaterials.TapeBrown, new Vector3(spec.slot == 0 ? 0.55f : -0.55f, -0.45f, 0f), new Vector3(0.7f, 0.04f, 0.3f), new Vector3(0f, 0f, spec.slot == 0 ? -12f : 12f));
                    pv.spin = spin.transform;
                    pv.spinSpeed = 55f;
                    Marker(pv, spec.color, new Vector3(0f, 0.62f, -0.2f));
                    Sphere(pv.go, Vector3.zero, 0.45f);
                    break;
                }
                case PartType.ShutterGear:
                {
                    PartView pv = NewPart(i, "ShutterGear" + spec.slot, new Vector3(lx, ly, -0.1f));
                    BuildGear(pv, spec.color, 0.7f);
                    Marker(pv, spec.color, new Vector3(0f, 0.55f, -0.15f));
                    Sphere(pv.go, Vector3.zero, 0.4f);
                    break;
                }
                case PartType.Tnt:
                    BuildDynamiteBundle(i, spec, lx, ly, -0.2f);
                    break;
                default:
                {
                    PartView pv = NewPart(i, "Part" + i, new Vector3(lx, ly, 0f));
                    Prims.Make("Block", pv.go.transform, Prims.Cube, mat, Vector3.zero, Vector3.one * 0.5f);
                    Marker(pv, spec.color, new Vector3(0f, 0.5f, 0f));
                    Box(pv.go, Vector3.zero, Vector3.one * 0.5f);
                    break;
                }
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
                        Prims.Make("Top", t, Prims.Cube, GameMaterials.Silver, new Vector3(0f, 2.0f, 0f), new Vector3(3.6f, 0.4f, 1.2f));
                        Prims.Make("Bottom", t, Prims.Cube, GameMaterials.Silver, new Vector3(0f, 0.62f, 0f), new Vector3(3.6f, 0.24f, 1.2f));
                        Prims.Make("Left", t, Prims.Cube, GameMaterials.Silver, new Vector3(-1.425f, 1.2f, 0f), new Vector3(0.75f, 1.4f, 1.2f));
                        Prims.Make("Right", t, Prims.Cube, GameMaterials.Silver, new Vector3(1.425f, 1.2f, 0f), new Vector3(0.75f, 1.4f, 1.2f));
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

        // ------------------------------------------------------------------ vintage rotary phone

        private void BuildPhoneDecor()
        {
            // desk-phone body shell that stays when parts fall off, plus the coiled handset cord
            Prims.Make("PhoneShell", pivot, Prims.Cube, GameMaterials.Bakelite, new Vector3(0f, 0.55f, 0.15f), new Vector3(3.3f, 1.0f, 2.3f));
            Prims.Make("PhoneSlope", pivot, Prims.Cube, GameMaterials.Bakelite, new Vector3(0f, 1.2f, 0.35f), new Vector3(2.7f, 0.5f, 1.6f), new Vector3(-18f, 0f, 0f));
            for (int k = 0; k < 9; k++)
            {
                float u = k / 8f;
                Prims.Make("Cord" + k, pivot, Prims.Sphere, GameMaterials.Charcoal,
                    new Vector3(-1.7f + 0.2f * k * 0.5f, 1.15f + Mathf.Sin(u * Mathf.PI * 3f) * 0.12f - u * 0.2f, -1.2f - Mathf.Sin(u * Mathf.PI) * 0.3f), Vector3.one * 0.17f);
            }
        }

        private void BuildPhonePart(int i, PartSpec spec, float lx, float ly)
        {
            Material mat = GameMaterials.ForBlock(spec.color);
            switch (spec.type)
            {
                case PartType.Handset:
                {
                    bool ear = spec.slot == 0;
                    float dir = ear ? 1f : -1f;
                    PartView pv = NewPart(i, ear ? "HandsetEarpiece" : "HandsetMouthpiece", new Vector3(lx, ly, 0f));
                    Transform t = pv.go.transform;
                    Prims.Make("Cup", t, Prims.Sphere, mat, Vector3.zero, new Vector3(0.85f, 0.7f, 0.85f));
                    Prims.Make("CupRim", t, Prims.Cylinder, GameMaterials.Brass, new Vector3(0f, -0.28f, 0f), new Vector3(0.78f, 0.04f, 0.78f));
                    Prims.Make("Grip", t, Prims.Cylinder, GameMaterials.Bakelite, new Vector3(0.85f * dir, 0.02f, 0f), new Vector3(0.3f, 0.55f, 0.3f), new Vector3(0f, 0f, 90f));
                    Marker(pv, spec.color, new Vector3(0f, 0.7f, -0.3f));
                    Sphere(pv.go, Vector3.zero, 0.5f);
                    break;
                }
                case PartType.Cradle:
                {
                    PartView pv = NewPart(i, "BrassCradle", new Vector3(lx, ly, 0f));
                    Transform t = pv.go.transform;
                    Prims.Make("HookL", t, Prims.Cube, GameMaterials.Brass, new Vector3(-0.75f, 0f, 0f), new Vector3(0.22f, 0.75f, 0.35f));
                    Prims.Make("HookR", t, Prims.Cube, GameMaterials.Brass, new Vector3(0.75f, 0f, 0f), new Vector3(0.22f, 0.75f, 0.35f));
                    Prims.Make("Bar", t, Prims.Cube, GameMaterials.Brass, new Vector3(0f, -0.3f, 0f), new Vector3(1.9f, 0.16f, 0.35f));
                    Marker(pv, spec.color, new Vector3(0f, 0.65f, -0.3f));
                    Box(pv.go, Vector3.zero, new Vector3(1.9f, 0.8f, 0.4f));
                    break;
                }
                case PartType.Casing:
                {
                    PartView pv = NewPart(i, "PhoneCasing", new Vector3(lx, 0f, 0f));
                    Transform t = pv.go.transform;
                    Prims.Make("Body", t, Prims.Cube, mat, new Vector3(0f, 0.9f, 0.45f), new Vector3(3.1f, 1.2f, 1.1f));
                    Prims.Make("RearPlate", t, Prims.Cube, GameMaterials.Brass, new Vector3(0f, 0.9f, 1.02f), new Vector3(2.4f, 0.9f, 0.05f));
                    pv.aim = new Vector3(0f, 0.9f, 0.45f);
                    Marker(pv, spec.color, new Vector3(1.2f, 1.7f, 0.3f));
                    Box(pv.go, new Vector3(0f, 0.9f, 0.45f), new Vector3(3.1f, 1.2f, 1.1f));
                    break;
                }
                case PartType.Dial:
                {
                    PartView pv = NewPart(i, "RotaryDial", new Vector3(lx, ly, -0.95f));
                    Transform t = pv.go.transform;
                    GameObject wheel = new GameObject("FingerWheel");
                    wheel.transform.SetParent(t, false);
                    Prims.Make("Acrylic", wheel.transform, Prims.Cylinder, GameMaterials.Acrylic(spec.color), Vector3.zero, new Vector3(1.9f, 0.05f, 1.9f), new Vector3(90f, 0f, 0f));
                    Prims.Make("BrassRing", wheel.transform, Prims.Cylinder, GameMaterials.Brass, new Vector3(0f, 0f, 0.03f), new Vector3(1.98f, 0.03f, 1.98f), new Vector3(90f, 0f, 0f));
                    for (int k = 0; k < 10; k++)
                    {
                        float a = (k * 30f + 40f) * Mathf.Deg2Rad;
                        Prims.Make("Hole" + k, wheel.transform, Prims.Cylinder, GameMaterials.Rubber, new Vector3(Mathf.Cos(a) * 0.62f, Mathf.Sin(a) * 0.62f, -0.03f), new Vector3(0.26f, 0.02f, 0.26f), new Vector3(90f, 0f, 0f));
                        Prims.Label(wheel.transform, ((k + 1) % 10).ToString(), 0.2f, Color.white, new Vector3(Mathf.Cos(a) * 0.62f, Mathf.Sin(a) * 0.62f, -0.06f));
                    }
                    Prims.Make("CenterCap", wheel.transform, Prims.Cylinder, GameMaterials.Brass, new Vector3(0f, 0f, -0.04f), new Vector3(0.5f, 0.04f, 0.5f), new Vector3(90f, 0f, 0f));
                    pv.spin = wheel.transform;
                    pv.spinSpeed = 0f; // only turns when hit
                    Marker(pv, spec.color, new Vector3(0f, 1.2f, -0.2f));
                    Sphere(pv.go, Vector3.zero, 1.0f);
                    break;
                }
                case PartType.Ringer:
                {
                    PartView pv = NewPart(i, "RingerBell" + spec.slot, new Vector3(lx, ly, -0.15f));
                    Transform t = pv.go.transform;
                    Prims.Make("Dome", t, Prims.Sphere, GameMaterials.Brass, Vector3.zero, new Vector3(0.95f, 0.6f, 0.95f));
                    Prims.Make("Post", t, Prims.Cylinder, GameMaterials.Steel, new Vector3(0f, -0.35f, 0f), new Vector3(0.07f, 0.18f, 0.07f));
                    Prims.Make("Tint", t, Prims.Cylinder, GameMaterials.Glow(spec.color), new Vector3(0f, 0.05f, 0f), new Vector3(0.7f, 0.02f, 0.7f));
                    Marker(pv, spec.color, new Vector3(0f, 0.75f, -0.15f));
                    Sphere(pv.go, Vector3.zero, 0.5f);
                    break;
                }
                case PartType.Coil:
                {
                    PartView pv = NewPart(i, "InductionCoil", new Vector3(lx, ly, -0.3f));
                    Transform t = pv.go.transform;
                    Prims.Make("Core", t, Prims.Cylinder, GameMaterials.Steel, Vector3.zero, new Vector3(0.28f, 0.55f, 0.28f), new Vector3(0f, 0f, 90f));
                    for (int k = 0; k < 9; k++)
                        Prims.Make("Wind" + k, t, Prims.Cylinder, GameMaterials.Copper, new Vector3(-0.5f + k * 0.125f, 0f, 0f), new Vector3(0.62f, 0.03f, 0.62f), new Vector3(0f, 0f, 90f));
                    Marker(pv, spec.color, new Vector3(0f, 0.65f, -0.15f));
                    Sphere(pv.go, Vector3.zero, 0.55f);
                    break;
                }
                case PartType.Tnt:
                    // the first TNT is tucked behind the casing at the rear: spin the phone to find it
                    BuildTnt(i, spec, lx, ly, spec.slot == 0 ? 0.75f : -0.45f);
                    break;
                default:
                {
                    PartView pv = NewPart(i, "Part" + i, new Vector3(lx, ly, 0f));
                    BuildGear(pv, spec.color, 0.85f);
                    Marker(pv, spec.color, new Vector3(0f, 0.6f, -0.15f));
                    Sphere(pv.go, Vector3.zero, 0.42f);
                    break;
                }
            }
        }

        // ------------------------------------------------------------------ antique pocket watch

        private void BuildWatchDecor()
        {
            // movement plate + case body that remain once the cover and dial fall away
            Prims.Make("WatchBody", pivot, Prims.Cylinder, GameMaterials.Gold, new Vector3(0f, 1.5f, 0.25f), new Vector3(2.95f, 0.38f, 2.95f), new Vector3(90f, 0f, 0f));
            Prims.Make("MovementPlate", pivot, Prims.Cylinder, GameMaterials.Brass, new Vector3(0f, 1.5f, 0.02f), new Vector3(2.5f, 0.03f, 2.5f), new Vector3(90f, 0f, 0f));
            Prims.Make("Stem", pivot, Prims.Cylinder, GameMaterials.Gold, new Vector3(0f, 3.05f, 0.25f), new Vector3(0.14f, 0.25f, 0.14f));
        }

        private void BuildWatchPart(int i, PartSpec spec, float lx, float ly)
        {
            Material mat = GameMaterials.ForBlock(spec.color);
            switch (spec.type)
            {
                case PartType.Casing:
                {
                    bool front = spec.slot == 0;
                    PartView pv = NewPart(i, front ? "HunterCover" : "CaseBack", new Vector3(lx, ly, front ? -0.62f : 0.62f));
                    Transform t = pv.go.transform;
                    Prims.Make("Plate", t, Prims.Cylinder, GameMaterials.Gold, Vector3.zero, new Vector3(2.8f, 0.07f, 2.8f), new Vector3(90f, 0f, 0f));
                    Prims.Make("Tint", t, Prims.Cylinder, GameMaterials.Glow(spec.color), new Vector3(0f, 0f, front ? -0.045f : 0.045f), new Vector3(1.9f, 0.015f, 1.9f), new Vector3(90f, 0f, 0f));
                    for (int k = 0; k < 12; k++)
                    {
                        float a = k * 30f * Mathf.Deg2Rad;
                        Prims.Make("Petal" + k, t, Prims.Sphere, GameMaterials.Brass, new Vector3(Mathf.Sin(a) * 1.08f, Mathf.Cos(a) * 1.08f, front ? -0.05f : 0.05f), new Vector3(0.2f, 0.2f, 0.07f));
                    }
                    if (front) Prims.Make("Hinge", t, Prims.Cylinder, GameMaterials.Brass, new Vector3(1.3f, -0.2f, 0f), new Vector3(0.1f, 0.35f, 0.1f));
                    else Prims.Make("BowRing", t, Prims.Cylinder, GameMaterials.Gold, new Vector3(0f, 1.55f, 0f), new Vector3(0.55f, 0.06f, 0.55f), new Vector3(90f, 0f, 0f));
                    Marker(pv, spec.color, new Vector3(front ? -0.9f : 0.9f, 1.1f, front ? -0.2f : 0.2f));
                    Sphere(pv.go, Vector3.zero, 1.35f);
                    break;
                }
                case PartType.Button:
                {
                    PartView pv = NewPart(i, "WindingCrown", new Vector3(lx, ly, 0.25f));
                    Transform t = pv.go.transform;
                    Prims.Make("Crown", t, Prims.Cylinder, mat, Vector3.zero, new Vector3(0.45f, 0.14f, 0.45f));
                    Prims.Make("Knurl", t, Prims.Cylinder, GameMaterials.Gold, new Vector3(0f, 0.08f, 0f), new Vector3(0.5f, 0.03f, 0.5f));
                    Marker(pv, spec.color, new Vector3(0.5f, 0.35f, 0f));
                    Sphere(pv.go, Vector3.zero, 0.3f);
                    break;
                }
                case PartType.Glass:
                {
                    PartView pv = NewPart(i, "CrystalDome", new Vector3(lx, ly, -0.58f));
                    Prims.Make("Dome", pv.go.transform, Prims.Sphere, GameMaterials.Acrylic(spec.color), Vector3.zero, new Vector3(2.5f, 2.5f, 0.55f));
                    Marker(pv, spec.color, new Vector3(1.0f, 1.0f, -0.3f));
                    Sphere(pv.go, Vector3.zero, 1.15f);
                    break;
                }
                case PartType.Dial:
                {
                    PartView pv = NewPart(i, "PorcelainDial", new Vector3(lx, ly, -0.2f));
                    Transform t = pv.go.transform;
                    Prims.Make("Face", t, Prims.Cylinder, GameMaterials.Porcelain, Vector3.zero, new Vector3(2.4f, 0.03f, 2.4f), new Vector3(90f, 0f, 0f));
                    string[] roman = { "XII", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X", "XI" };
                    for (int k = 0; k < 12; k++)
                    {
                        float a = k * 30f * Mathf.Deg2Rad;
                        Prims.Label(t, roman[k], 0.22f, new Color(0.08f, 0.07f, 0.08f), new Vector3(Mathf.Sin(a) * 0.98f, Mathf.Cos(a) * 0.98f, -0.04f));
                    }
                    Marker(pv, spec.color, new Vector3(-1.0f, 1.0f, -0.2f));
                    Sphere(pv.go, Vector3.zero, 1.15f);
                    break;
                }
                case PartType.Hand:
                {
                    PartView pv = NewPart(i, "FiligreeHands", new Vector3(lx, ly, -0.3f));
                    Transform t = pv.go.transform;
                    Prims.Make("Hour", t, Prims.Cube, GameMaterials.Gold, new Vector3(0.14f, 0.18f, 0f), new Vector3(0.08f, 0.55f, 0.02f), new Vector3(0f, 0f, -55f));
                    GameObject minute = Prims.Make("Minute", t, Prims.Cube, GameMaterials.Gold, new Vector3(0f, 0.42f, -0.02f), new Vector3(0.05f, 0.9f, 0.02f));
                    Prims.Make("Hub", t, Prims.Sphere, mat, new Vector3(0f, 0f, -0.04f), Vector3.one * 0.14f);
                    pv.hand = minute.transform;
                    Marker(pv, spec.color, new Vector3(0f, -0.9f, -0.2f));
                    Box(pv.go, new Vector3(0f, 0.3f, 0f), new Vector3(0.4f, 1.0f, 0.1f));
                    break;
                }
                case PartType.BalanceWheel:
                {
                    PartView pv = NewPart(i, "BalanceWheel" + spec.slot, new Vector3(lx, ly, -0.1f));
                    Transform t = pv.go.transform;
                    Prims.Make("Rim", t, Prims.Cylinder, GameMaterials.Gold, Vector3.zero, new Vector3(1.4f, 0.025f, 1.4f), new Vector3(90f, 0f, 0f));
                    Prims.Make("Inner", t, Prims.Cylinder, GameMaterials.Rubber, new Vector3(0f, 0f, 0.02f), new Vector3(1.2f, 0.02f, 1.2f), new Vector3(90f, 0f, 0f));
                    for (int k = 0; k < 3; k++)
                        Prims.Make("Spoke" + k, t, Prims.Cube, GameMaterials.Gold, Vector3.zero, new Vector3(1.3f, 0.06f, 0.03f), new Vector3(0f, 0f, k * 60f));
                    Prims.Make("Hub", t, Prims.Sphere, mat, new Vector3(0f, 0f, -0.04f), Vector3.one * 0.22f);
                    Marker(pv, spec.color, new Vector3(0f, 0.95f, -0.2f));
                    Sphere(pv.go, Vector3.zero, 0.75f);
                    break;
                }
                case PartType.Gear:
                {
                    PartView pv = NewPart(i, "EscapementGear" + spec.slot, new Vector3(lx, ly, -0.05f));
                    BuildGear(pv, spec.color, 0.6f);
                    Marker(pv, spec.color, new Vector3(0f, 0.5f, -0.15f));
                    Sphere(pv.go, Vector3.zero, 0.35f);
                    break;
                }
                case PartType.Ruby:
                {
                    PartView pv = NewPart(i, "RubyBearing" + spec.slot, new Vector3(lx, ly, -0.08f));
                    Transform t = pv.go.transform;
                    Prims.Make("Setting", t, Prims.Cylinder, GameMaterials.Gold, Vector3.zero, new Vector3(0.36f, 0.03f, 0.36f), new Vector3(90f, 0f, 0f));
                    Prims.Make("Jewel", t, Prims.Sphere, GameMaterials.RubyJewel, new Vector3(0f, 0f, -0.04f), Vector3.one * 0.22f);
                    Marker(pv, spec.color, new Vector3(0f, 0.4f, -0.1f));
                    Sphere(pv.go, Vector3.zero, 0.25f);
                    break;
                }
                case PartType.Tnt:
                {
                    // TNT mainspring core: a coiled red spring
                    PartView pv = NewPart(i, "MainspringCore" + spec.slot, new Vector3(lx, ly, -0.18f));
                    Transform t = pv.go.transform;
                    Prims.Make("Barrel", t, Prims.Cylinder, GameMaterials.TntRed, Vector3.zero, new Vector3(0.7f, 0.07f, 0.7f), new Vector3(90f, 0f, 0f));
                    Prims.Make("Coil", t, Prims.Cylinder, GameMaterials.Glow(BlockColor.Yellow), new Vector3(0f, 0f, -0.06f), new Vector3(0.38f, 0.03f, 0.38f), new Vector3(90f, 0f, 0f));
                    Prims.Label(t, "TNT", 0.2f, Color.white, new Vector3(0f, -0.55f, -0.1f));
                    Marker(pv, spec.color, new Vector3(0f, 0.6f, -0.1f));
                    Sphere(pv.go, Vector3.zero, 0.4f);
                    break;
                }
                default:
                {
                    PartView pv = NewPart(i, "Part" + i, new Vector3(lx, ly, 0f));
                    Prims.Make("Block", pv.go.transform, Prims.Cube, mat, Vector3.zero, Vector3.one * 0.5f);
                    Marker(pv, spec.color, new Vector3(0f, 0.5f, 0f));
                    Box(pv.go, Vector3.zero, Vector3.one * 0.5f);
                    break;
                }
            }
        }

        /// <summary>The rubber platen roller slides sideways with a carriage ding when hit.</summary>
        private IEnumerator SlidePlaten(int part)
        {
            PartView pv = views[part];
            if (pv.go == null || pv.broken) yield break;
            Transform roller = pv.go.transform.Find("Roller");
            if (roller == null) yield break;
            if (SoundSynth.Instance != null) { SoundSynth.Instance.Play(Sfx.CarriageSlide, 1.1f, 0.5f); SoundSynth.Instance.Play(Sfx.Ding, 1f, 0.6f); }
            Vector3 home = roller.localPosition;
            float t = 0f;
            while (t < 0.3f && roller != null && !pv.broken)
            {
                t += Time.deltaTime;
                roller.localPosition = home + new Vector3(Mathf.Sin(Mathf.Clamp01(t / 0.3f) * Mathf.PI) * 0.45f, 0f, 0f);
                yield return null;
            }
            if (roller != null) roller.localPosition = home;
        }

        /// <summary>Typebar hammers flick upward erratically when struck.</summary>
        private IEnumerator FlickTypebars(int part)
        {
            PartView pv = views[part];
            if (pv.go == null || pv.broken) yield break;
            Transform fan = pv.go.transform.Find("Fan");
            if (fan == null) yield break;
            if (SoundSynth.Instance != null) SoundSynth.Instance.Play(Sfx.MetalPing, UnityEngine.Random.Range(1.1f, 1.7f), 0.5f);
            float t = 0f;
            while (t < 0.28f && fan != null && !pv.broken)
            {
                t += Time.deltaTime;
                float k = Mathf.Sin(Mathf.Clamp01(t / 0.28f) * Mathf.PI);
                for (int i = 0; i < fan.childCount; i++)
                {
                    Transform bar = fan.GetChild(i);
                    float off = ((i * 37) % 11) / 11f;
                    bar.localRotation = Quaternion.Euler(-k * (20f + 40f * off), bar.localEulerAngles.y, bar.localEulerAngles.z);
                }
                yield return null;
            }
            if (fan != null) for (int i = 0; i < fan.childCount; i++) fan.GetChild(i).localRotation = Quaternion.Euler(0f, fan.GetChild(i).localEulerAngles.y, fan.GetChild(i).localEulerAngles.z);
        }

        /// <summary>Round keys clatter down a little when struck.</summary>
        private IEnumerator ClatterKeys(int part)
        {
            PartView pv = views[part];
            if (pv.go == null || pv.broken) yield break;
            Transform keys = pv.go.transform.Find("KeyField");
            if (keys == null) yield break;
            if (SoundSynth.Instance != null) SoundSynth.Instance.Play(Sfx.Click, UnityEngine.Random.Range(0.8f, 1.2f), 0.6f);
            float t = 0f;
            Vector3 home = keys.localPosition;
            while (t < 0.18f && keys != null && !pv.broken)
            {
                t += Time.deltaTime;
                keys.localPosition = home + new Vector3(0f, -Mathf.Sin(Mathf.Clamp01(t / 0.18f) * Mathf.PI) * 0.06f, 0f);
                yield return null;
            }
            if (keys != null) keys.localPosition = home;
        }

        /// <summary>Every projectile hit physically spins the rotary dial one step with an authentic click.</summary>
        private IEnumerator SpinDialOnce(int part)
        {
            PartView pv = views[part];
            if (pv.spin == null || pv.broken) yield break;
            if (SoundSynth.Instance != null) SoundSynth.Instance.Play(Sfx.DialClick, UnityEngine.Random.Range(0.95f, 1.1f), 0.6f);
            float t = 0f;
            while (t < 0.16f && pv.spin != null)
            {
                t += Time.deltaTime;
                pv.spin.Rotate(0f, 0f, 36f * Time.deltaTime / 0.16f, Space.Self);
                yield return null;
            }
        }

        // ================================================================== combat

        /// <summary>
        /// A turret fires at <paramref name="part"/>. The hit is applied to the model immediately (so every observer sees
        /// consistent HP); the pellet / laser animation and the physical break-off follow when it lands.
        /// </summary>
        public bool Shoot(int target, Vector3 from, BlockColor color, bool mega, bool pierce)
        {
            if (Model == null || target < 0) return false;
            int part = MonumentModel.PartOf(target);
            bool screwShot = MonumentModel.IsScrewCode(target);
            if (screwShot)
            {
                int sk = MonumentModel.ScrewIndexOf(target);
                if (!Model.IsScrewTargetable(part) || ((Model.ScrewMask[part] >> sk) & 1) == 0 || (int)Model.Parts[part].ScrewColorAt(sk) != (int)color) return false;
            }
            else if (!Model.IsTargetable(part) || (int)Model.Parts[part].color != (int)color)
            {
                return false;
            }

            List<int> destroyed = new List<int>();
            List<int> detonated = new List<int>();
            List<int> unscrewed = new List<int>();
            Model.HitTarget(target, destroyed, detonated, unscrewed);

            // Clockwork piercing shot: also damages one part hidden directly behind the target.
            int pierced = -1;
            if (pierce && !screwShot) pierced = Model.PierceBehind(part, destroyed, detonated);

            firingUntil = Time.time + 0.5f;
            StartCoroutine(Pellet(from, target, pierced, color, mega, destroyed, detonated, unscrewed));
            return true;
        }

        /// <summary>Visual only: a shot at a screw-pinned plate of the turret's own colour glances off with a metallic ping.</summary>
        public void Deflect(int part, Vector3 from, BlockColor color)
        {
            if (views == null || part < 0 || part >= views.Length || views[part].go == null) return;
            StartCoroutine(DeflectRoutine(part, from, color));
        }

        private IEnumerator DeflectRoutine(int part, Vector3 from, BlockColor color)
        {
            Vector3 to = PartWorld(part);
            GameObject shot = Prims.Make("Ping", root, Prims.Sphere, GameMaterials.UnlitGlow(GameMaterials.ColorOf(color), 3f), from, Vector3.one * 0.18f);
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.14f;
                shot.transform.position = Vector3.Lerp(from, to, Mathf.Clamp01(t));
                yield return null;
            }
            if (FXManager.Instance != null) FXManager.Instance.Sparks(to, 8);
            if (SoundSynth.Instance != null) SoundSynth.Instance.Play(Sfx.MetalPing, UnityEngine.Random.Range(0.9f, 1.15f), 0.6f);
            Vector3 away = to + (to - from).normalized * 1.6f + new Vector3(0f, 1.2f, 0f);
            t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.25f;
                shot.transform.position = Vector3.Lerp(to, away, Mathf.Clamp01(t));
                yield return null;
            }
            Destroy(shot);
        }

        /// <summary>Slotted brass screw pinning a plate: it must be unscrewed by a turret of the screw colour first.</summary>
        private void AddScrew(int i, PartSpec spec)
        {
            PartView pv = views[i];
            bool rear = (spec.type == PartType.Casing && (Kind == MonumentKind.RotaryPhone || (Kind == MonumentKind.PocketWatch && spec.slot == 1)))
                || (Kind == MonumentKind.EspressoMachine && spec.type == PartType.Lever)
                || (Kind == MonumentKind.TlrCamera && spec.type == PartType.Body && spec.slot == 1)
                || (Kind == MonumentKind.Typewriter && spec.type == PartType.Keys)
                || (Kind == MonumentKind.Orrery && spec.type == PartType.Ring && spec.slot >= 2)
                || (Kind == MonumentKind.PipeOrgan && ((spec.type == PartType.Facade && spec.slot == 1) || spec.type == PartType.Bellows))
                || (Kind == MonumentKind.MusicBox && ((spec.type == PartType.Lid && spec.slot == 1) || (spec.type == PartType.Drum && spec.slot == 1)))
                || (Kind == MonumentKind.GrandfatherClock && ((spec.type == PartType.Hood && spec.slot == 1) || (spec.type == PartType.Weight && spec.slot == 1)))
                || (Kind == MonumentKind.Tourbillon && ((spec.type == PartType.Bezel && spec.slot >= 2) || (spec.type == PartType.Bridge && spec.slot == 1)))
                || (Kind == MonumentKind.Gramophone && (spec.type == PartType.Bracket || spec.type == PartType.Horn && spec.slot == 1));
            for (int k = 0; k < spec.ScrewTotal; k++)
            {
                GameObject sc = new GameObject("BrassScrew" + k);
                sc.transform.SetParent(pv.go.transform, false);
                Vector3 spread = new Vector3(-0.42f * k, 0.22f * k, 0f);
                sc.transform.localPosition = pv.aim + (rear ? new Vector3(0.9f, 0.5f, 0.75f) : new Vector3(0.55f, 0.45f, -0.35f)) + spread;
                sc.transform.localEulerAngles = new Vector3(rear ? -90f : 90f, 0f, 0f);
                Prims.Make("Head", sc.transform, Prims.Cylinder, GameMaterials.Brass, Vector3.zero, new Vector3(0.28f, 0.035f, 0.28f));
                Prims.Make("Slot", sc.transform, Prims.Cube, GameMaterials.Charcoal, new Vector3(0f, 0.04f, 0f), new Vector3(0.22f, 0.02f, 0.045f));
                Prims.Make("ColorRing", sc.transform, Prims.Cylinder, GameMaterials.Glow(spec.ScrewColorAt(k)), new Vector3(0f, -0.02f, 0f), new Vector3(0.36f, 0.012f, 0.36f));
                pv.screws[k] = sc.transform;
            }
        }

        private IEnumerator Unscrew(int code)
        {
            int part = code % 128, sk = code / 128;
            PartView pv = views[part];
            if (pv.screws[sk] == null) yield break;
            Transform sc = pv.screws[sk];
            pv.screws[sk] = null;
            if (SoundSynth.Instance != null) SoundSynth.Instance.Play(Sfx.Ratchet);
            if (FXManager.Instance != null) FXManager.Instance.Sparks(sc.position, 8);
            float t = 0f;
            Vector3 outward = sc.up;
            while (t < 0.65f && sc != null)
            {
                t += Time.deltaTime;
                sc.Rotate(0f, 900f * Time.deltaTime, 0f, Space.Self); // counter-clockwise unscrew
                sc.position += outward * (0.55f * Time.deltaTime);
                yield return null;
            }
            if (sc == null) yield break;
            // pops out with physics
            sc.SetParent(debrisRoot, true);
            Rigidbody rb = sc.gameObject.AddComponent<Rigidbody>();
            rb.mass = 0.05f;
            BoxCollider bc = sc.gameObject.AddComponent<BoxCollider>();
            bc.size = new Vector3(0.3f, 0.12f, 0.3f);
            rb.AddForce((outward + Vector3.up * 1.2f + new Vector3(0f, 0f, -0.8f)) * 3.5f, ForceMode.VelocityChange);
            rb.AddTorque(UnityEngine.Random.insideUnitSphere * 8f, ForceMode.VelocityChange);
            if (FXManager.Instance != null) FXManager.Instance.GoldSparks(sc.position, 10);
            if (SoundSynth.Instance != null) SoundSynth.Instance.Play(Sfx.DebrisClink, 1.3f, 0.5f);
            debrisAlive++;
            StartCoroutine(DebrisLife(sc.gameObject, 3.2f));
            RefreshLabel(part);
        }

        private IEnumerator Pellet(Vector3 from, int target, int pierced, BlockColor color, bool mega, List<int> destroyed, List<int> detonated, List<int> unscrewed)
        {
            InFlight++;
            int part = MonumentModel.PartOf(target);
            Vector3 to = PartWorld(target);
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

            to = PartWorld(target);
            if (FXManager.Instance != null)
            {
                if (destroyed.Count == 0 && unscrewed.Count == 0) FXManager.Instance.Sparks(to, 6);
                FXManager.Instance.Shake(0.04f, 0.1f);
            }
            if (SoundSynth.Instance != null && destroyed.Count == 0 && unscrewed.Count == 0) SoundSynth.Instance.Play(Sfx.DebrisClink, UnityEngine.Random.Range(0.9f, 1.2f), 0.45f);
            for (int u = 0; u < unscrewed.Count; u++) StartCoroutine(Unscrew(unscrewed[u]));

            if (views != null)
            {
                if (part < views.Length) RefreshLabel(part);
                if (pierced >= 0 && pierced < views.Length) RefreshLabel(pierced);
            }
            if (PartDamaged != null && Model != null && !MonumentModel.IsScrewCode(target)) PartDamaged(part, Model.Hp[part]);
            if (views != null && part < views.Length && Model != null && Model.Parts[part].type == PartType.Dial && Kind == MonumentKind.RotaryPhone) StartCoroutine(SpinDialOnce(part));
            if (views != null && part < views.Length && Model != null && Kind == MonumentKind.Typewriter)
            {
                PartType ptype = Model.Parts[part].type;
                if (ptype == PartType.Platen) StartCoroutine(SlidePlaten(part));
                else if (ptype == PartType.Typebar) StartCoroutine(FlickTypebars(part));
                else if (ptype == PartType.Keys) StartCoroutine(ClatterKeys(part));
            }

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
            bool glass = (spec.type == PartType.Lid && spec.slot == 0) || spec.type == PartType.Glass || spec.type == PartType.Crystal || spec.type == PartType.ClockDoor || spec.type == PartType.Door || spec.type == PartType.Lens || spec.type == PartType.SightTube;
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
                    go.transform.localScale = sc * (1f - Mathf.Clamp01(s));
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
                    if (SoundSynth.Instance != null && i == 0 && Kind == MonumentKind.PocketWatch) SoundSynth.Instance.Play(Sfx.Tick, 1f, 0.25f);
                }
                if (Kind == MonumentKind.PocketWatch && !pv.broken && pv.go.name.StartsWith("BalanceWheel"))
                {
                    pv.go.transform.localEulerAngles = new Vector3(0f, 0f, 42f * Mathf.Sin(Time.time * 6.5f));
                }
                if (pv.spin != null && !pv.broken)
                {
                    float speed = pv.spinSpeed * (busy ? 3f : 1f);
                    if (pv.spin.name == "BellowsPump") pv.spin.localScale = new Vector3(1f, 1f + 0.16f * Mathf.Sin(Time.time * 2.2f), 1f);
                    else if (pv.spin.name == "PendSwing") pv.spin.localEulerAngles = new Vector3(0f, 0f, 15f * Mathf.Sin(Time.time * 2.4f));
                    else if (pv.go.name == "CassetteTape")
                    {
                        for (int c = 0; c < pv.spin.childCount; c++)
                        {
                            Transform ch = pv.spin.GetChild(c);
                            if (ch.name.StartsWith("Spoke")) ch.Rotate(0f, 0f, speed * dt, Space.Self);
                        }
                    }
                    else if (pv.spin.name == "Disc" || pv.spin.name == "GovSpin" || (pv.spin.name == "SpoolSpin" && Kind == MonumentKind.Typewriter)) pv.spin.Rotate(0f, speed * dt, 0f, Space.Self);
                    else pv.spin.Rotate(0f, 0f, speed * dt, Space.Self);
                }
                if (refreshLabels && pv.label != null && !pv.broken)
                {
                    bool visible = Model.IsTargetable(i) || Model.IsScrewTargetable(i);
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
