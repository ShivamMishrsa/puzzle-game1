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

        protected static Placement Arrow(Placement p, MoveAxis axis) { p.arrow = axis; return p; }

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
            d.monument = id >= 11 ? MonumentKind.CassettePlayer : MonumentKind.AlarmClock;
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
                if (d.monument == MonumentKind.AlarmClock)
                {
                    if (p.type == PartType.Gear) mask = MaskOf(d, PartType.Casing);
                }
                else
                {
                    if (p.type == PartType.Cassette) mask = MaskOf(d, PartType.Door);
                    else if (p.type == PartType.Gear || p.type == PartType.Pcb || p.type == PartType.Tnt)
                    {
                        mask = MaskOf(d, PartType.Cassette);
                        if (mask == 0) mask = MaskOf(d, PartType.Door);
                    }
                }
                p.hiddenByMask = mask;
                d.parts[i] = p;
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
