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
        protected const BlockColor P = BlockColor.Purple;
        protected const BlockColor O = BlockColor.Orange;
        protected const BlockColor C = BlockColor.Cyan;

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

        /// <summary>Steam Pressure Gate: every <paramref name="interval"/> player moves it flips between Normal and Pressurized (+2 bonus ammo).</summary>
        protected static GateSpec Steam(GateSpec g, int interval) { g.steamInterval = interval; return g; }

        /// <summary>Welded twin-linked pair: give exactly two blocks the same group id (1..9).</summary>
        protected static Placement Link(Placement p, int group) { p.linkGroup = group; return p; }

        /// <summary>Colour-shifting prism block: starts as cycle[0] and advances one step on every block exit.</summary>
        protected static Placement Prism(Placement p, params BlockColor[] cycle) { p.cycle = cycle; p.color = cycle[0]; return p; }

        /// <summary>Pins a part with up to three coupled brass screws (colours in order); all must be unscrewed before the plate can be hurt.</summary>
        protected static PartSpec Screws(PartSpec p, params BlockColor[] colors)
        {
            p.hasScrew = true;
            p.screwCount = colors.Length;
            p.screwColor = colors[0];
            if (colors.Length > 1) p.screw2 = colors[1];
            if (colors.Length > 2) p.screw3 = colors[2];
            return p;
        }

        /// <summary>Polarity flipper coil at (x, y): PULL (startsPull) or PUSH before the first exit, flips on every gate exit.</summary>
        protected static PolaritySpec Polarity(int x, int y, bool startsPull)
        {
            PolaritySpec q = new PolaritySpec();
            q.cell = new Int2(x, y); q.startsPull = startsPull;
            return q;
        }

        /// <summary>Repulsor coil at (x, y): shoves a block that rests on / beside it one tile away.</summary>
        protected static RepulsorSpec Repulsor(int x, int y)
        {
            RepulsorSpec r = new RepulsorSpec();
            r.cell = new Int2(x, y);
            return r;
        }

        /// <summary>One-way ratchet turnstile at (x, y): blocks may only slide onto it moving in direction <paramref name="dir"/>.</summary>
        protected static TurnstileSpec Turnstile(int x, int y, GateDirection dir)
        {
            TurnstileSpec t = new TurnstileSpec();
            t.cell = new Int2(x, y); t.dir = dir;
            return t;
        }

        /// <summary>Crated mystery block: the colour passed to Rect/Ell is the colour revealed when a neighbour leaves.</summary>
        protected static Placement Crate(Placement p) { p.crate = true; return p; }

        /// <summary>Typewriter carriage row: after every exit, blocks lying inside row <paramref name="row"/> slide one tile (dir +1 right, -1 left).</summary>
        protected static CarriageSpec Carriage(int row, int dir)
        {
            CarriageSpec c = new CarriageSpec();
            c.row = row; c.dir = dir;
            return c;
        }

        /// <summary>Electromagnet pad at (x, y) that pulls a resting neighbour one tile in direction <paramref name="pull"/>.</summary>
        protected static MagnetSpec Magnet(int x, int y, GateDirection pull)
        {
            MagnetSpec m = new MagnetSpec();
            m.cell = new Int2(x, y); m.pull = pull;
            return m;
        }

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
            d.megaPierces = id >= 81;
            d.monument = id >= 131 ? MonumentKind.Locomotive : id >= 121 ? MonumentKind.PipeOrgan : id >= 111 ? MonumentKind.MusicBox : id >= 101 ? MonumentKind.GrandfatherClock : id >= 91 ? MonumentKind.Tourbillon : id >= 81 ? MonumentKind.Orrery : id >= 71 ? MonumentKind.Gramophone : id >= 61 ? MonumentKind.Typewriter : id >= 51 ? MonumentKind.TlrCamera : (id >= 41 ? MonumentKind.EspressoMachine : (id >= 31 ? MonumentKind.PocketWatch : (id >= 21 ? MonumentKind.RotaryPhone : (id >= 11 ? MonumentKind.CassettePlayer : MonumentKind.AlarmClock))));
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
                    case MonumentKind.EspressoMachine:
                        // stage 2 (group head, sight tube, drip grid) sits behind the boiler shell; stage 3 (coils, valves, TNT) behind the group head
                        if (p.type == PartType.GroupHead || p.type == PartType.SightTube || p.type == PartType.DripGrid) mask = MaskOf(d, PartType.Boiler);
                        else if (p.type == PartType.Coil || p.type == PartType.Valve || p.type == PartType.Tnt)
                        {
                            mask = MaskOf(d, PartType.GroupHead);
                            if (mask == 0) mask = MaskOf(d, PartType.Boiler);
                        }
                        break;
                    case MonumentKind.Locomotive:
                        // 4-layer: boiler shells + smokestack -> cab + pistons -> driving wheels + coal bunker -> dynamite in the firebox
                        if (p.type == PartType.Cab || p.type == PartType.Piston) mask = MaskOf(d, PartType.Boiler) | MaskOf(d, PartType.Smokestack);
                        else if (p.type == PartType.Driver || p.type == PartType.Coal)
                        {
                            mask = MaskOf(d, PartType.Cab) | MaskOf(d, PartType.Piston);
                            if (mask == 0) mask = MaskOf(d, PartType.Boiler) | MaskOf(d, PartType.Smokestack);
                        }
                        else if (p.type == PartType.Tnt)
                        {
                            mask = MaskOf(d, PartType.Driver) | MaskOf(d, PartType.Coal);
                            if (mask == 0) mask = MaskOf(d, PartType.Cab) | MaskOf(d, PartType.Piston);
                        }
                        break;
                    case MonumentKind.PipeOrgan:
                        // 4-layer: carved facade -> pipes + keyboard -> bellows + stop rails -> dynamite behind the bellows
                        if (p.type == PartType.Pipes || p.type == PartType.Keyboard) mask = MaskOf(d, PartType.Facade);
                        else if (p.type == PartType.Bellows || p.type == PartType.Stops)
                        {
                            mask = MaskOf(d, PartType.Pipes) | MaskOf(d, PartType.Keyboard);
                            if (mask == 0) mask = MaskOf(d, PartType.Facade);
                        }
                        else if (p.type == PartType.Tnt)
                        {
                            mask = MaskOf(d, PartType.Bellows) | MaskOf(d, PartType.Stops);
                            if (mask == 0) mask = MaskOf(d, PartType.Pipes) | MaskOf(d, PartType.Keyboard);
                        }
                        break;
                    case MonumentKind.MusicBox:
                        // 4-layer: glass dome + brass collar -> dancer + comb -> pinned discs -> dynamite behind the discs
                        if (p.type == PartType.Dancer || p.type == PartType.Comb) mask = MaskOf(d, PartType.Lid);
                        else if (p.type == PartType.Drum)
                        {
                            mask = MaskOf(d, PartType.Dancer) | MaskOf(d, PartType.Comb);
                            if (mask == 0) mask = MaskOf(d, PartType.Lid);
                        }
                        else if (p.type == PartType.Tnt)
                        {
                            mask = MaskOf(d, PartType.Drum);
                            if (mask == 0) mask = MaskOf(d, PartType.Dancer) | MaskOf(d, PartType.Comb);
                        }
                        break;
                    case MonumentKind.GrandfatherClock:
                        // 4-layer: hood + glass door -> dial + pendulum -> weights + chimes -> dynamite music-box behind them
                        if (p.type == PartType.Dial || p.type == PartType.Pendulum)
                        {
                            mask = MaskOf(d, PartType.Hood) | MaskOf(d, PartType.ClockDoor);
                        }
                        else if (p.type == PartType.Weight || p.type == PartType.Chime)
                        {
                            mask = MaskOf(d, PartType.Dial) | MaskOf(d, PartType.Pendulum);
                            if (mask == 0) mask = MaskOf(d, PartType.Hood) | MaskOf(d, PartType.ClockDoor);
                        }
                        else if (p.type == PartType.Tnt)
                        {
                            mask = MaskOf(d, PartType.Weight) | MaskOf(d, PartType.Chime);
                            if (mask == 0) mask = MaskOf(d, PartType.Dial) | MaskOf(d, PartType.Pendulum);
                        }
                        break;
                    case MonumentKind.Tourbillon:
                        // 3-layer: bezel + crystal -> skeleton bridges + tourbillon cage -> barrels + rotor -> dynamite behind the cage
                        if (p.type == PartType.Bridge || p.type == PartType.TourbillonCage)
                        {
                            mask = MaskOf(d, PartType.Bezel) | MaskOf(d, PartType.Crystal);
                        }
                        else if (p.type == PartType.MainBarrel || p.type == PartType.Rotor)
                        {
                            mask = MaskOf(d, PartType.Bridge);
                            if (mask == 0) mask = MaskOf(d, PartType.Bezel) | MaskOf(d, PartType.Crystal);
                        }
                        else if (p.type == PartType.Tnt)
                        {
                            mask = MaskOf(d, PartType.TourbillonCage);
                            if (mask == 0) mask = MaskOf(d, PartType.Bridge);
                        }
                        break;
                    case MonumentKind.Orrery:
                        // 3-layer vault armour: zodiac rings shield the orbiting planets, the planets shield the gear cage, the cage shields the dynamite
                        if (p.type == PartType.Planet) mask = MaskOf(d, PartType.Ring);
                        else if (p.type == PartType.Cage)
                        {
                            mask = MaskOf(d, PartType.Planet);
                            if (mask == 0) mask = MaskOf(d, PartType.Ring);
                        }
                        else if (p.type == PartType.Tnt)
                        {
                            mask = MaskOf(d, PartType.Cage);
                            if (mask == 0) mask = MaskOf(d, PartType.Planet);
                        }
                        break;
                    case MonumentKind.Gramophone:
                        // 2-layer nested armour: the inner bracket / governor / mainspring are shielded until their outer shell is gone
                        if (p.type == PartType.Bracket || p.type == PartType.Record) mask = MaskOf(d, PartType.Horn);
                        else if (p.type == PartType.Governor) mask = MaskOf(d, PartType.Record);
                        else if (p.type == PartType.Cabinet)
                        {
                            mask = MaskOf(d, PartType.Record);
                            if (mask == 0) mask = MaskOf(d, PartType.Horn);
                        }
                        else if (p.type == PartType.Mainspring || p.type == PartType.Tnt) mask = MaskOf(d, PartType.Cabinet);
                        break;
                    case MonumentKind.Typewriter:
                        // stage 2 (typebar basket, ribbon spools) sits behind the hood; stage 3 (rack, springs, TNT) behind the typebars
                        if (p.type == PartType.Typebar || p.type == PartType.Ribbon) mask = MaskOf(d, PartType.Hood);
                        else if (p.type == PartType.Rack || p.type == PartType.Spring || p.type == PartType.Tnt)
                        {
                            mask = MaskOf(d, PartType.Typebar);
                            if (mask == 0) mask = MaskOf(d, PartType.Hood);
                        }
                        break;
                    case MonumentKind.TlrCamera:
                        // stage 2 (iris blades, film crank) sits behind the lenses; stage 3 (spools, shutter gears, TNT) behind the iris
                        if (p.type == PartType.Iris || p.type == PartType.Crank) mask = MaskOf(d, PartType.Lens);
                        else if (p.type == PartType.Spool || p.type == PartType.ShutterGear || p.type == PartType.Tnt)
                        {
                            mask = MaskOf(d, PartType.Iris);
                            if (mask == 0) mask = MaskOf(d, PartType.Lens);
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

        /// <summary>Additionally hides one part behind every part of another type (e.g. the TNT behind the phone casing).</summary>
        protected static void HideBehind(LevelDefinition d, PartType type, int slot, PartType by)
        {
            for (int i = 0; i < d.parts.Count; i++)
            {
                if (d.parts[i].type == type && d.parts[i].slot == slot)
                {
                    PartSpec p = d.parts[i];
                    p.hiddenByMask |= MaskOf(d, by);
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
