using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>MINI-BOSS Brass Horn Gridlock: first 8x8 tray. Three brass screws, two padlocked gates, one-way turnstiles and 2-layer armour protecting the dynamite. Optimal moves, no padding.</summary>
    public sealed class Level75 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(75, "Horn Gridlock", 8, 8, 27, 4, LevelTier.MiniBoss, "Three bolts, two padlocks, one-way ratchets. Zero wasted moves!");
            d.turnstiles.Add(Turnstile(3, 4, Right));
            d.turnstiles.Add(Turnstile(5, 3, Up));
            d.turnstiles.Add(Turnstile(2, 5, Down));
            d.placements.Add(Scrap(1, 2, 3, 2));
            d.placements.Add(Scrap(2, 1, 5, 5));
            d.placements.Add(Scrap(1, 1, 1, 6));
            d.placements.Add(Rect(P, 2, 2, 6, 6));
            d.placements.Add(Rect(G, 2, 2, 1, 2));
            d.placements.Add(Rect(B, 2, 2, 4, 0));
            d.placements.Add(Rect(Y, 2, 2, 3, 4));
            d.placements.Add(Rect(R, 2, 2, 1, 0));
            d.placements.Add(Rect(Y, 2, 1, 2, 7));
            d.placements.Add(Rect(G, 1, 2, 5, 3));
            d.placements.Add(Rect(P, 1, 2, 6, 1));
            d.placements.Add(Rect(G, 2, 1, 6, 3));
            d.placements.Add(Rect(R, 2, 1, 1, 5));
            d.placements.Add(Rect(R, 1, 2, 0, 4));
            d.placements.Add(Rect(B, 1, 2, 7, 0));
            d.placements.Add(Rect(Y, 1, 2, 3, 0));
            d.placements.Add(Rect(P, 2, 1, 4, 2));
            d.placements.Add(Key(Rect(B, 1, 1, 1, 7), 0));
            d.placements.Add(Key(Rect(G, 1, 1, 0, 3), 1));
            d.gates.Add(Gate(Up, 0, 2, R));
            d.gates.Add(Gate(Down, 0, 2, R));
            d.gates.Add(Gate(Left, 0, 2, B));
            d.gates.Add(Gate(Right, 0, 2, B));
            d.gates.Add(Gate(Down, 2, 2, Y));
            d.gates.Add(Padlock(Gate(Up, 2, 2, Y), 1));
            d.gates.Add(Gate(Right, 2, 2, G));
            d.gates.Add(Gate(Left, 2, 2, G));
            d.gates.Add(Padlock(Gate(Up, 4, 2, P), 0));
            d.gates.Add(Gate(Down, 4, 2, P));
            d.parts.Add(Screw(Part(PartType.Horn, 0, Y, 3, 0), G));
            d.parts.Add(Screw(Part(PartType.Horn, 1, R, 3, 0), P));
            d.parts.Add(Screw(Part(PartType.Bracket, 0, B, 3, 1), R));
            d.parts.Add(Part(PartType.Record, 0, P, 3, 1));
            d.parts.Add(Part(PartType.Governor, 0, G, 3, 2));
            d.parts.Add(Part(PartType.Cabinet, 0, R, 3, 2));
            d.parts.Add(Part(PartType.Mainspring, 0, B, 3, 3));
            d.parts.Add(Part(PartType.Tnt, 0, Y, 2, 3));
            ResolveHidden(d, false);
            d.showRotateHint = true;
            return d;
        }
    }
}
