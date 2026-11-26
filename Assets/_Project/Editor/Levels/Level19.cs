using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Pre-Boss Gridlock: 2-layer ice, a peeled block and a shutter gate around a central scrap pillar.</summary>
    public sealed class Level19 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(19, "Pre-Boss Gridlock", 7, 7, 20, 4, LevelTier.Normal, "Everything at once: ice, peeled cores and a shutter around the iron pillar.");
            d.placements.Add(Scrap(1, 3, 3, 2));
            d.placements.Add(Ice(Rect(Y, 2, 2, 0, 5), 2));
            d.placements.Add(Rect(B, 2, 1, 1, 4));
            d.placements.Add(Rect(B, 1, 2, 5, 5));
            d.placements.Add(Peeled(Rect(R, 2, 1, 4, 2), B));
            d.placements.Add(Rect(R, 1, 2, 5, 3));
            d.placements.Add(Rect(R, 1, 1, 3, 5));
            d.placements.Add(Rect(Y, 1, 1, 0, 3));
            d.gates.Add(Gate(Up, 0, 2, Y));
            d.gates.Add(Shutter(Right, 2, 2, R, true));
            d.gates.Add(Gate(Down, 4, 2, R));
            d.gates.Add(Gate(Left, 4, 2, B));
            d.gates.Add(Gate(Down, 0, 2, B));
            d.gates.Add(Gate(Up, 5, 1, Y));
            d.parts.Add(Part(PartType.Door, 0, R, 3, 0));
            d.parts.Add(Part(PartType.Casing, 0, B, 3, 0));
            d.parts.Add(Part(PartType.Cassette, 0, Y, 3, 1));
            d.parts.Add(Part(PartType.Tnt, 0, B, 1, 2));
            ResolveHidden(d, false);
            return d;
        }
    }
}
