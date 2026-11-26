using System;
using System.Collections.Generic;
using UnityEngine;

namespace ColorGateBlast
{
    /// <summary>
    /// The perimeter conveyor around the Target Core. Owns the turret slots: a block may only leave the board when
    /// a slot is free. Turrets enter at the bottom-left corner (nearest the board) and loop clockwise.
    /// </summary>
    public sealed class ConveyorTrackManager : MonoBehaviour
    {
        private struct Pending
        {
            public BlockColor color;
            public int ammo;
            public Vector3 from;
        }

        public float RingSpeed = 6.5f; // ring cells per second

        public event Action SlotsChanged;

        private readonly List<TurretController> turrets = new List<TurretController>();
        private readonly Queue<Pending> pending = new Queue<Pending>();
        private TargetCoreManager core;
        private Transform root;
        private Vector2 center;
        private float cellSize;
        private int cw, ch, ringLen, entry;
        private int maxSlots;
        private GameObject[] pips;
        private Material pipFree, pipUsed;
        private int lastUsedShown = -1;

        public int MaxSlots { get { return maxSlots; } }
        public int UsedSlots { get { return turrets.Count + pending.Count; } }
        public bool HasFreeSlot { get { return UsedSlots < maxSlots; } }
        public bool IsFull { get { return UsedSlots >= maxSlots; } }
        public int PendingCount { get { return pending.Count; } }

        public void Clear()
        {
            for (int i = 0; i < turrets.Count; i++) if (turrets[i] != null) Destroy(turrets[i].gameObject);
            turrets.Clear();
            pending.Clear();
            if (root != null) Destroy(root.gameObject);
            root = null;
            pips = null;
            lastUsedShown = -1;
        }

        public void Build(LevelDefinition def, TargetCoreManager coreMgr, Vector2 corePos, float cell)
        {
            Clear();
            core = coreMgr;
            center = corePos;
            cellSize = cell;
            cw = def.coreWidth;
            ch = def.coreHeight;
            ringLen = RingPath.Length(cw, ch);
            entry = RingPath.EntryIndex(cw, ch);
            maxSlots = def.maxTrackSlots;

            root = new GameObject("TrackRoot").transform;
            root.SetParent(transform, false);

            // conveyor tiles
            MeshBuilder mb = new MeshBuilder();
            float size = cell * 0.88f;
            for (int i = 0; i < ringLen; i++)
            {
                Vector3 p = RingWorld(i);
                mb.AddSlab(new Vector2(p.x, p.y), size, size, 0.05f, size * 0.3f, size * 0.08f, 2);
            }
            GameObject tiles = new GameObject("TrackTiles");
            tiles.transform.SetParent(root, false);
            tiles.transform.position = new Vector3(0, 0, 0.35f);
            tiles.AddComponent<MeshFilter>().sharedMesh = mb.ToMesh("TrackTiles");
            tiles.AddComponent<MeshRenderer>().sharedMaterial = Mats.Lit(ColorPalette.TrackTile, 0.05f);

            // entry marker
            Vector3 e = RingWorld(entry);
            GameObject mark = new GameObject("Entry");
            mark.transform.SetParent(root, false);
            mark.transform.position = new Vector3(e.x, e.y, 0.25f);
            mark.AddComponent<MeshFilter>().sharedMesh = ProceduralMeshes.Slab(size * 0.8f, size * 0.8f, 0.04f, size * 0.25f, size * 0.06f, "Entry");
            mark.AddComponent<MeshRenderer>().sharedMaterial = Mats.Lit(ColorPalette.Lighten(ColorPalette.TrackTile, 0.25f), 0.1f);

            pipFree = Mats.Lit(ColorPalette.SlotFree, 0.05f);
            pipUsed = Mats.Lit(new Color(1f, 0.85f, 0.35f), 0.3f);
            BuildPips();
        }

        private void BuildPips()
        {
            if (pips != null) for (int i = 0; i < pips.Length; i++) if (pips[i] != null) Destroy(pips[i]);
            pips = new GameObject[maxSlots];
            float s = cellSize * 0.34f;
            Mesh m = ProceduralMeshes.Slab(s, s, 0.08f, s * 0.5f, s * 0.15f, "Pip");
            float y = center.y - (ch + 2) * cellSize * 0.5f - cellSize * 0.42f;
            float gap = s * 1.5f;
            for (int i = 0; i < maxSlots; i++)
            {
                GameObject p = new GameObject("SlotPip_" + i);
                p.transform.SetParent(root, false);
                p.transform.position = new Vector3(center.x + (i - (maxSlots - 1) * 0.5f) * gap, y, 0.2f);
                p.AddComponent<MeshFilter>().sharedMesh = m;
                p.AddComponent<MeshRenderer>().sharedMaterial = pipFree;
                pips[i] = p;
            }
            lastUsedShown = -1;
        }

        public Vector3 RingWorld(float idx)
        {
            idx = Mathf.Repeat(idx, ringLen);
            int i0 = Mathf.FloorToInt(idx);
            float f = idx - i0;
            int rx0, ry0, rx1, ry1;
            RingPath.Cell(i0, cw, ch, out rx0, out ry0);
            RingPath.Cell(i0 + 1, cw, ch, out rx1, out ry1);
            float rx = Mathf.Lerp(rx0, rx1, f);
            float ry = Mathf.Lerp(ry0, ry1, f);
            return new Vector3(center.x + (rx - (cw + 1) * 0.5f) * cellSize, center.y + (ry - (ch + 1) * 0.5f) * cellSize, 0f);
        }

        /// <summary>Gives one extra conveyor slot (rewarded-ad reward).</summary>
        public void AddSlot()
        {
            maxSlots++;
            BuildPips();
            if (SlotsChanged != null) SlotsChanged();
        }

        /// <summary>A block has left the board: queue a turret with ammo = its tile count.</summary>
        public void Enqueue(BlockColor color, int ammo, Vector3 from)
        {
            Pending p = new Pending();
            p.color = color;
            p.ammo = ammo;
            p.from = from;
            pending.Enqueue(p);
            if (SlotsChanged != null) SlotsChanged();
        }

        private bool EntryClear()
        {
            for (int i = 0; i < turrets.Count; i++)
            {
                float d = Mathf.Abs(Mathf.Repeat(turrets[i].Progress - entry + ringLen * 0.5f, ringLen) - ringLen * 0.5f);
                if (d < 1.1f) return false;
            }
            return true;
        }

        /// <summary>
        /// True when no turret can ever hit anything again and nothing is waiting to spawn:
        /// the conveyor state can no longer change by itself.
        /// </summary>
        public bool IsSettled()
        {
            if (pending.Count > 0) return false;
            for (int i = 0; i < turrets.Count; i++)
                if (turrets[i].CanStillHit()) return false;
            return true;
        }

        private void Update()
        {
            if (core == null || core.Model == null) return;

            if (pending.Count > 0 && EntryClear())
            {
                Pending p = pending.Dequeue();
                TurretController t = TurretController.Create(this, core, root, p.color, p.ammo, entry, p.from, cellSize, cw, ch, RingSpeed);
                turrets.Add(t);
                if (JuiceManager.Instance != null) JuiceManager.Instance.PlayTick();
            }

            bool changed = false;
            for (int i = turrets.Count - 1; i >= 0; i--)
            {
                TurretController t = turrets[i];
                t.Tick(Time.deltaTime);
                bool done = t.Ammo <= 0 || core.Model.RemainingOfColor((int)t.Color) == 0;
                if (done)
                {
                    turrets.RemoveAt(i);
                    t.Despawn();
                    changed = true;
                }
            }
            if (changed && SlotsChanged != null) SlotsChanged();

            if (UsedSlots != lastUsedShown && pips != null)
            {
                lastUsedShown = UsedSlots;
                for (int i = 0; i < pips.Length; i++)
                    pips[i].GetComponent<MeshRenderer>().sharedMaterial = i < lastUsedShown ? pipUsed : pipFree;
            }
        }
    }
}
