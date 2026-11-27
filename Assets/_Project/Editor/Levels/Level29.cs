using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Pre-Boss Locksmith: 2-layer ice, rail blocks, a shutter and a key-lock gate around a central scrap-iron pillar.</summary>
    public sealed class Level29 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(29, "Pre-Boss Locksmith", 7, 7, 13, 4, LevelTier.Normal, "Ice, rails, a shutter and a padlock - all around the iron pillar.");
            d.placements.Add(Scrap(1, 3, 3, 2));
            d.placements.Add(Ice(Rect(G, 2, 2, 0, 5), 2));
            d.placements.Add(Rect(Y, 2, 1, 3, 6));
            d.placements.Add(Rect(R, 1, 2, 5, 5));
            d.placements.Add(Arrow(Rect(B, 1, 2, 4, 4), Vertical));
            d.placements.Add(Arrow(Rect(R, 2, 1, 2, 5), Horizontal));
            d.placements.Add(Rect(B, 1, 1, 5, 4));
            d.placements.Add(Rect(G, 1, 1, 2, 6));
            d.placements.Add(Key(Rect(Y, 1, 1, 6, 6), 0));
            d.placements.Add(Rect(B, 1, 1, 6, 1));
            d.gates.Add(Shutter(Left, 4, 2, R, true));
            d.gates.Add(Gate(Down, 4, 2, R));
            d.gates.Add(Gate(Up, 4, 2, B));
            d.gates.Add(Gate(Right, 0, 2, B));
            d.gates.Add(Padlock(Gate(Up, 0, 3, G), 0));
            d.gates.Add(Padlock(Gate(Left, 0, 3, G), 0));
            d.gates.Add(Gate(Down, 0, 3, Y));
            d.gates.Add(Gate(Right, 4, 3, Y));
            d.parts.Add(Screw(Part(PartType.Handset, 0, R, 3, 0), Y));
            d.parts.Add(Part(PartType.Handset, 1, G, 3, 0));
            d.parts.Add(Part(PartType.Cradle, 0, Y, 2, 0));
            d.parts.Add(Part(PartType.Dial, 0, B, 3, 1));
            d.parts.Add(Screw(Part(PartType.Tnt, 0, B, 1, 2), G));
            ResolveHidden(d, false);
            return d;
        }
    }
}
