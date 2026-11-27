using System;
using System.Collections.Generic;

// Pure C# (no UnityEngine / UnityEditor) so levels can be solver-checked in a standalone harness too.
// Coordinates: x grows to the right, y grows UP the tray (away from the player, towards the monument).

namespace ColorGateBlast.EditorTools
{
    /// <summary>Authoring helpers shared by every LevelNN class.</summary>
    public abstract class LevelBase
    {
        protected const BlockColor R = BlockColor.Red;
        protected const BlockColor B = BlockColor.Blue;
        protected const BlockColor Y = BlockColor.Yellow;
        protected const BlockColor X = BlockColor.Gray;
        protected const BlockColor G = BlockColor.Green;

        protected const GateDirection Up = GateDirection.Up;
        protected const GateDirection Down = GateDirection.Down;
        protected const GateDirection Left = GateDirection.Left;
        protected const GateDirection Right = GateDirection.Right;

        protected static Placement Rect(BlockColor c, int w, int h, int x, int y)
        {
            Placement p = new Placement();
            p.color = c; p.shape = BlockShape.Rect; p.width = w; p.height = h; p.x = x; p.y = y;
            return p;
        }

        /// <summary>Rectangle without a position: only used by the layout search harness.</summary>
        protected static Placement Rect(BlockColor c, int w, int h) { return Rect(c, w, h, -1, -1); }

        protected static Placement Ell(BlockColor c, int rotation, int x, int y)
        {
            Placement p = new Placement();
            p.color = c; p.shape = BlockShape.L; p.width = 2; p.height = 2; p.rotation = rotation; p.x = x; p.y = y;
            return p;
        }

        protected static Placement Ell(BlockColor c, int rotation) { return Ell(c, rotation, -1, -1); }

        protected static Placement Scrap(int w, int h, int x, int y)
        {
            Placement p = Rect(X, w, h, x, y);
            p.locked = true;
            return p;
        }

        /// <summary>Adds a frosted ice shell (1 or 2 layers).</summary>
        protected static Placement Ice(Placement p, int layers) { p.iceLayer = layers; return p; }

        /// <summary>Makes the block dual-layer: it respawns as a 1x1 of <paramref name="inner"/> after exiting.</summary>
        protected static Placement Peeled(Placement p, BlockColor inner) { p.hasInner = true; p.innerColor = inner; return p; }

        /// <summary>Directional rail block: slides only along <paramref name="axis"/> (Horizontal / Vertical).</summary>
        protected static Placement Arrow(Placement p, MoveAxis axis) { p.arrow = axis; return p; }

        protected const MoveAxis Horizontal = MoveAxis.Horizontal;
        protected const MoveAxis Vertical = MoveAxis.Vertical;

        /// <summary>Brass key block: when it leaves, every padlocked gate with this lock id opens.</summary>
        protected static Placement Key(Placement p, int lockId) { p.hasBrassKey = true; p.lockId = lockId; return p; }

        /// <summary>Peeled block whose respawned inner core carries the brass key for <paramref name="lockId"/>.</summary>
        protected static Placement PeeledKey(Placement p, BlockColor inner, int lockId) { p.hasInner = true; p.innerColor = inner; p.innerHasKey = true; p.lockId = lockId; return p; }

        protected static GateSpec Gate(GateDirection d, int start, int length, BlockColor c)
        {
            GateSpec g = new GateSpec();
            g.direction = d; g.start = start; g.length = length; g.color = c;
            return g;
        }

        /// <summary>Heavy striped steel shutter over the gate. All shutters toggle whenever any block exits.</summary>
        protected static GateSpec Shutter(GateDirection d, int start, int length, BlockColor c, bool startsOpen)
        {
            GateSpec g = Gate(d, start, length, c);
            g.isShutterGate = true; g.startsOpen = startsOpen;
            return g;
        }

        /// <summary>Seals the gate with chains and a brass padlock until the key with this lock id has left the tray.</summary>
        protected static GateSpec Padlock(GateSpec g, int lockId) { g.isPadlocked = true; g.lockId = lockId; return g; }

        /// <summary>Clockwork piercing gate: its turrets pierce the outer shell and the layer behind it.</summary>
        protected static GateSpec Piercing(GateSpec g) { g.isPiercingGate = true; return g; }

        protected static PortalSpec Pad(int pair, int ax, int ay, int bx, int by)
        {
            PortalSpec p = new PortalSpec();
            p.pair = pair; p.a = new Int2(ax, ay); p.b = new Int2(bx, by);
            return p;
        }

        /// <summary>Pins a monument part down with a slotted brass screw of the given colour.</summary>
        protected static PartSpec Screw(PartSpec p, BlockColor screwColor) { p.hasScrew = true; p.screwColor = screwColor; return p; }

        protected static PartSpec Part(PartType t, int slot, BlockColor c, int hp, int layer)
        {
            PartSpec p = new PartSpec();
            p.type = t; p.slot = slot; p.color = c; p.hp = hp; p.layer = layer;
            return p;
        }

        protected static LevelDefinition Make(int id, string title, int w, int h, int moves, int slots, LevelTier tier, string hint)
        {
            LevelDefinition d = new LevelDefinition();
            d.levelID = id; d.title = title; d.gridSize = new Int2(w, h);
            d.moveLimit = moves; d.maxTrackSlots = slots; d.tier = tier; d.hint = hint;
            d.monument = id >= 31 ? MonumentKind.PocketWatch : (id >= 21 ? MonumentKind.RotaryPhone : (id >= 11 ? MonumentKind.CassettePlayer : MonumentKind.AlarmClock));
            return d;
        }

        /// <summary>
        /// Fills in which parts hide which: gears sit inside the alarm clock casing; on the cassette player the
        /// cassette hides behind the acrylic door and the gears / PCB / TNT capacitor hide behind the cassette.
        /// Pass exposeAll = true for levels that start with the outer shell already broken off.
        /// </summary>
        protected static void ResolveHidden(LevelDefinition d, bool exposeAll)
        {
            if (exposeAll) return;
            for (int i = 0; i < d.parts.Count; i++)
            {
                PartSpec p = d.parts[i];
                int mask = 0;
                switch (d.monument)
                {
                    case MonumentKind.AlarmClock:
                        if (p.type == PartType.Gear) mask = MaskOf(d, PartType.Casing);
                        break;
                    case MonumentKind.CassettePlayer:
                        if (p.type == PartType.Cassette) mask = MaskOf(d, PartType.Door);
                        else if (p.type == PartType.Gear || p.type == PartType.Pcb || p.type == PartType.Tnt)
                        {
                            mask = MaskOf(d, PartType.Cassette);
                            if (mask == 0) mask = MaskOf(d, PartType.Door);
                        }
                        break;
                    case MonumentKind.RotaryPhone:
                        // stage 2: the dial sits behind the handset; stage 3 (ringer bells, coil, TNT) sits behind the dial
                        if (p.type == PartType.Dial) mask = MaskOf(d, PartType.Handset);
                        else if (p.type == PartType.Ringer || p.type == PartType.Coil || p.type == PartType.Tnt || p.type == PartType.Gear)
                        {
                            mask = MaskOf(d, PartType.Dial);
                            if (mask == 0) mask = MaskOf(d, PartType.Handset);
                        }
                        break;
                    case MonumentKind.PocketWatch:
                        // stage 2 (dome, porcelain dial, hands) sits behind the hunter case; stage 3 behind the dial
                        if (p.type == PartType.Glass || p.type == PartType.Dial || p.type == PartType.Hand) mask = MaskOf(d, PartType.Casing);
                        else if (p.type == PartType.BalanceWheel || p.type == PartType.Gear || p.type == PartType.Ruby || p.type == PartType.Tnt)
                        {
                            mask = MaskOf(d, PartType.Dial);
                            if (mask == 0) mask = MaskOf(d, PartType.Casing);
                        }
                        break;
                }
                p.hiddenByMask = mask;
                d.parts[i] = p;
            }
        }

        /// <summary>Makes one part visible from the start (e.g. a rear TNT capacitor) regardless of the default hide rules.</summary>
        protected static void Unhide(LevelDefinition d, PartType type, int slot)
        {
            for (int i = 0; i < d.parts.Count; i++)
            {
                if (d.parts[i].type == type && d.parts[i].slot == slot)
                {
                    PartSpec p = d.parts[i];
                    p.hiddenByMask = 0;
                    d.parts[i] = p;
                }
            }
        }

        private static int MaskOf(LevelDefinition d, PartType type)
        {
            int mask = 0;
            for (int i = 0; i < d.parts.Count; i++) if (d.parts[i].type == type) mask |= 1 << i;
            return mask;
        }
    }
}
