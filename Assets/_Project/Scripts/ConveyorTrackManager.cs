using System;
using System.Collections.Generic;
using UnityEngine;

namespace ColorGateBlast
{
    /// <summary>
    /// The rusted, bolted conveyor loop circling the tray. A block leaving a gate spawns a turret at the belt point
    /// nearest that gate; turrets ride the belt at a steady pace. Capacity (maxTrackSlots) is enforced here:
    /// a new colour needs a free slot, while a colour that already has a turret on the belt merges into it for free.
    /// </summary>
    public sealed class ConveyorTrackManager : MonoBehaviour
    {
        public float BaseSpeed = 2.6f; // belt units per second

        public event Action SlotsChanged;

        private readonly List<TurretController> turrets = new List<TurretController>();
        private readonly List<Vector3> pts = new List<Vector3>();
        private readonly List<float> cum = new List<float>();
        private Transform root;
        private MonumentManager monument;
        private int maxSlots;
        private bool unlimited;
        private float length;
        private float overdriveUntil;
        private Vector3 trayCenter;
        private GameObject[] pips;
        private int lastShown = -1;
        private float pipZ;

        public float Speed { get { return BaseSpeed * (OverdriveActive ? 1.75f : 1f); } }
        public bool OverdriveActive { get { return Time.time < overdriveUntil; } }
        public int MaxSlots { get { return maxSlots; } }
        public bool Unlimited { get { return unlimited; } }
        public int UsedSlots { get { return turrets.Count; } }
        public bool IsFull { get { return !unlimited && turrets.Count >= maxSlots; } }
        public float BeltLength { get { return length; } }

        public void Clear()
        {
            for (int i = 0; i < turrets.Count; i++) if (turrets[i] != null) Destroy(turrets[i].gameObject);
            turrets.Clear();
            if (root != null) Destroy(root.gameObject);
            root = null;
            pts.Clear();
            cum.Clear();
            pips = null;
            lastShown = -1;
        }

        public void Build(LevelDefinition def, MonumentManager mon, Vector3 trayCenterWorld, float cellSize)
        {
            Clear();
            monument = mon;
            trayCenter = trayCenterWorld;
            maxSlots = def.maxTrackSlots;
            unlimited = def.UnlimitedTrack;
            overdriveUntil = 0f;

            float gap = cellSize * 0.85f + 0.3f;
            float hx = def.GridWidth * cellSize * 0.5f + gap;
            float hz = def.GridHeight * cellSize * 0.5f + gap;
            BuildPath(hx, hz, 0.75f);
            pipZ = trayCenterWorld.z - hz - 0.9f;

            root = new GameObject("TrackRoot").transform;
            root.SetParent(transform, false);
            BuildBelt();
            BuildPips();
        }

        // ------------------------------------------------------------------ path

        private void BuildPath(float hx, float hz, float r)
        {
            pts.Clear();
            // bottom edge -> bottom-right arc -> right edge -> top-right arc -> top edge -> top-left arc -> left edge -> bottom-left arc
            Vector3 c = trayCenter;
            AddLine(new Vector3(c.x - hx + r, 0f, c.z - hz), new Vector3(c.x + hx - r, 0f, c.z - hz));
            AddArc(new Vector3(c.x + hx - r, 0f, c.z - hz + r), r, -90f, 0f);
            AddLine(new Vector3(c.x + hx, 0f, c.z - hz + r), new Vector3(c.x + hx, 0f, c.z + hz - r));
            AddArc(new Vector3(c.x + hx - r, 0f, c.z + hz - r), r, 0f, 90f);
            AddLine(new Vector3(c.x + hx - r, 0f, c.z + hz), new Vector3(c.x - hx + r, 0f, c.z + hz));
            AddArc(new Vector3(c.x - hx + r, 0f, c.z + hz - r), r, 90f, 180f);
            AddLine(new Vector3(c.x - hx, 0f, c.z + hz - r), new Vector3(c.x - hx, 0f, c.z - hz + r));
            AddArc(new Vector3(c.x - hx + r, 0f, c.z - hz + r), r, 180f, 270f);

            cum.Clear();
            float acc = 0f;
            cum.Add(0f);
            for (int i = 1; i < pts.Count; i++)
            {
                acc += Vector3.Distance(pts[i - 1], pts[i]);
                cum.Add(acc);
            }
            acc += Vector3.Distance(pts[pts.Count - 1], pts[0]);
            length = acc;
        }

        private void AddLine(Vector3 a, Vector3 b)
        {
            int n = Mathf.Max(2, Mathf.CeilToInt(Vector3.Distance(a, b) / 0.3f));
            for (int i = 0; i < n; i++) pts.Add(Vector3.Lerp(a, b, i / (float)n));
        }

        private void AddArc(Vector3 center, float r, float a0, float a1)
        {
            int n = 8;
            for (int i = 0; i < n; i++)
            {
                float a = Mathf.Lerp(a0, a1, i / (float)n) * Mathf.Deg2Rad;
                pts.Add(center + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * r);
            }
        }

        /// <summary>Belt position at arc length s (wraps around), standing on top of the belt.</summary>
        public Vector3 PointAt(float s)
        {
            s = Mathf.Repeat(s, length);
            int lo = 0, hi = cum.Count - 1;
            while (lo < hi)
            {
                int mid = (lo + hi + 1) / 2;
                if (cum[mid] <= s) lo = mid; else hi = mid - 1;
            }
            Vector3 a = pts[lo];
            Vector3 b = pts[(lo + 1) % pts.Count];
            float segLen = (lo + 1 < cum.Count ? cum[lo + 1] : length) - cum[lo];
            float f = segLen > 0.0001f ? (s - cum[lo]) / segLen : 0f;
            Vector3 p = Vector3.Lerp(a, b, f);
            p.y = 0.2f;
            return p;
        }

        /// <summary>Arc length of the belt point nearest to a world position (where a turret spawns for a gate).</summary>
        public float NearestS(Vector3 world)
        {
            float best = float.MaxValue, bestS = 0f;
            for (int i = 0; i < pts.Count; i++)
            {
                Vector3 d = pts[i] - new Vector3(world.x, 0f, world.z);
                float m = d.sqrMagnitude;
                if (m < best) { best = m; bestS = cum[i]; }
            }
            return bestS;
        }

        // ------------------------------------------------------------------ belt geometry

        private void BuildBelt()
        {
            float halfW = 0.38f;
            MeshBuilder top = new MeshBuilder();
            MeshBuilder rails = new MeshBuilder();
            MeshBuilder bolts = new MeshBuilder();
            float boltAcc = 0f;
            for (int i = 0; i < pts.Count; i++)
            {
                Vector3 a = pts[i], b = pts[(i + 1) % pts.Count];
                Vector3 t = (b - a);
                float segLen = t.magnitude;
                if (segLen < 0.0001f) continue;
                t /= segLen;
                Vector3 n = new Vector3(-t.z, 0f, t.x);
                Vector3 up = Vector3.up;
                float y0 = 0.14f, y1 = 0.2f;

                // belt top
                top.AddQuad(a + n * halfW + up * y1, b + n * halfW + up * y1, b - n * halfW + up * y1, a - n * halfW + up * y1, up);
                // rails: raised lips on both sides
                for (int side = -1; side <= 1; side += 2)
                {
                    Vector3 o0 = n * (halfW * side), o1 = n * ((halfW - 0.09f) * side);
                    rails.AddQuad(a + o0 + up * 0.32f, b + o0 + up * 0.32f, b + o1 + up * 0.32f, a + o1 + up * 0.32f, up);
                    rails.AddQuad(a + o0 + up * y0, b + o0 + up * y0, b + o0 + up * 0.32f, a + o0 + up * 0.32f, n * side);
                    rails.AddQuad(a + o1 + up * y1, b + o1 + up * y1, b + o1 + up * 0.32f, a + o1 + up * 0.32f, -n * side);
                }
                boltAcc += segLen;
                if (boltAcc > 0.85f)
                {
                    boltAcc = 0f;
                    bolts.AddSphere(a + n * (halfW - 0.045f) + up * 0.33f, 0.06f, 6, 3);
                    bolts.AddSphere(a - n * (halfW - 0.045f) + up * 0.33f, 0.06f, 6, 3);
                }
            }

            GameObject topGo = new GameObject("BeltTop");
            topGo.transform.SetParent(root, false);
            topGo.AddComponent<MeshFilter>().sharedMesh = top.ToMesh("BeltTop", 0.6f);
            topGo.AddComponent<MeshRenderer>().sharedMaterial = GameMaterials.MetallicBelt;

            GameObject railGo = new GameObject("BeltRails");
            railGo.transform.SetParent(root, false);
            railGo.AddComponent<MeshFilter>().sharedMesh = rails.ToMesh("BeltRails", 0.6f);
            railGo.AddComponent<MeshRenderer>().sharedMaterial = GameMaterials.ScrapIron;

            GameObject boltGo = new GameObject("BeltBolts");
            boltGo.transform.SetParent(root, false);
            boltGo.AddComponent<MeshFilter>().sharedMesh = bolts.ToMesh("BeltBolts");
            boltGo.AddComponent<MeshRenderer>().sharedMaterial = GameMaterials.Brass;

            // colliders so falling debris bounces off the conveyor
            int segs = Mathf.Max(12, Mathf.CeilToInt(length / 1.2f));
            for (int i = 0; i < segs; i++)
            {
                Vector3 a = PointAt(length * i / segs), b = PointAt(length * (i + 1) / segs);
                Vector3 mid = (a + b) * 0.5f;
                GameObject cg = new GameObject("BeltCollider");
                cg.transform.SetParent(root, false);
                cg.transform.position = new Vector3(mid.x, 0.18f, mid.z);
                Vector3 dir = b - a;
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.0001f) cg.transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
                BoxCollider bc = cg.AddComponent<BoxCollider>();
                bc.size = new Vector3(0.8f, 0.3f, dir.magnitude + 0.12f);
            }
        }

        private void BuildPips()
        {
            if (pips != null) for (int i = 0; i < pips.Length; i++) if (pips[i] != null) Destroy(pips[i]);
            if (unlimited) { pips = new GameObject[0]; return; }
            pips = new GameObject[maxSlots];
            for (int i = 0; i < maxSlots; i++)
            {
                GameObject p = Prims.Make("SlotPip_" + i, root, Prims.Sphere, GameMaterials.Rubber,
                    new Vector3(trayCenter.x + (i - (maxSlots - 1) * 0.5f) * 0.55f, 0.18f, pipZ),
                    Vector3.one * 0.32f);
                pips[i] = p;
            }
            lastShown = -1;
        }

        // ------------------------------------------------------------------ turrets

        public bool HasTurretOfColor(BlockColor c)
        {
            for (int i = 0; i < turrets.Count; i++) if (turrets[i].Color == c && !turrets[i].Finished && !turrets[i].Pulling) return true;
            return false;
        }

        /// <summary>A block of this colour may leave if there is a free slot, or its turret would merge into an existing one.</summary>
        public bool CanAccept(BlockColor c)
        {
            return unlimited || turrets.Count < maxSlots || HasTurretOfColor(c);
        }

        /// <summary>Gives one extra slot (rewarded ad): permanently +1 for the current level.</summary>
        public void AddSlot()
        {
            maxSlots++;
            BuildPips();
            if (SlotsChanged != null) SlotsChanged();
        }

        public void TriggerOverdrive(float seconds) { overdriveUntil = Time.time + seconds; }

        /// <summary>A block has left a gate: spawn a turret there with ammo = tile count (merging if its colour is already on the belt).</summary>
        public void SpawnTurret(BlockColor color, int ammo, Vector3 fromWorld, Vector3 gateWorld, bool piercing)
        {
            float s0 = NearestS(gateWorld);
            TurretController existing = null;
            for (int i = 0; i < turrets.Count; i++)
                if (turrets[i].Color == color && !turrets[i].Finished && !turrets[i].Pulling) { existing = turrets[i]; break; }

            TurretController t = TurretController.Create(this, monument, root, color, ammo, s0, fromWorld, piercing);
            turrets.Add(t);
            if (existing != null) existing.MergeInto(t);
            if (SlotsChanged != null) SlotsChanged();
        }

        public void NotifyMerged(TurretController gone)
        {
            turrets.Remove(gone);
            if (SlotsChanged != null) SlotsChanged();
        }

        /// <summary>True when no turret can ever hit anything again: the belt can no longer change by itself.</summary>
        public bool IsSettled()
        {
            for (int i = 0; i < turrets.Count; i++)
                if (!turrets[i].Finished && (turrets[i].Pulling || turrets[i].CanStillHit())) return false;
            return true;
        }

        /// <summary>Live turrets as solver input (colour, ammo) for the hint system.</summary>
        public List<TurretInfo> Snapshot()
        {
            List<TurretInfo> l = new List<TurretInfo>();
            for (int i = 0; i < turrets.Count; i++)
            {
                if (turrets[i].Finished) continue;
                TurretInfo ti = new TurretInfo();
                ti.color = (int)turrets[i].Color;
                ti.ammo = turrets[i].Ammo;
                l.Add(ti);
            }
            return l;
        }

        private void Update()
        {
            bool changed = false;
            for (int i = turrets.Count - 1; i >= 0; i--)
            {
                TurretController t = turrets[i];
                if (t == null) { turrets.RemoveAt(i); changed = true; continue; }
                if (t.Finished || t.Pulling) continue;
                bool done = t.Ammo <= 0 || (monument != null && monument.AliveOfColor((int)t.Color) == 0);
                if (done)
                {
                    turrets.RemoveAt(i);
                    t.Despawn();
                    changed = true;
                }
            }
            if (changed && SlotsChanged != null) SlotsChanged();

            if (pips != null && turrets.Count != lastShown)
            {
                lastShown = turrets.Count;
                for (int i = 0; i < pips.Length; i++)
                    pips[i].GetComponent<MeshRenderer>().sharedMaterial = i < lastShown ? GameMaterials.Brass : GameMaterials.Rubber;
            }
        }
    }
}
