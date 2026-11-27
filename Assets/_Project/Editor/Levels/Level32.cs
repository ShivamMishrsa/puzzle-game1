using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Clockwork Piercing Showcase: a brass-geared piercing gate on the top border lets one green block pierce the hinged gold lid AND the crystal behind it.</summary>
    public sealed class Level32 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(32, "Piercing Showcase", 6, 6, 17, 5, LevelTier.Normal, "Brass gears frame the top gate: green shots pierce the lid and the dome behind it!");
            d.placements.Add(Scrap(1, 1, 2, 3));
            d.placements.Add(Scrap(1, 1, 3, 2));
            d.placements.Add(Rect(Y, 2, 2, 3, 3));
            d.placements.Add(Rect(B, 2, 1, 0, 1));
            d.placements.Add(Rect(G, 2, 1, 2, 5));
            d.placements.Add(Rect(R, 2, 1, 3, 0));
            d.placements.Add(Rect(R, 1, 1, 5, 0));
            d.placements.Add(Rect(B, 1, 1, 0, 5));
            d.portals.Add(Pad(0, 0, 3, 5, 3));
            d.gates.Add(Piercing(Gate(Up, 0, 3, G)));
            d.gates.Add(Gate(Right, 1, 1, R));
            d.gates.Add(Gate(Down, 4, 2, R));
            d.gates.Add(Gate(Left, 4, 2, B));
            d.gates.Add(Gate(Down, 0, 2, B));
            d.gates.Add(Gate(Right, 3, 2, Y));
            d.parts.Add(Part(PartType.Casing, 0, G, 2, 0));
            d.parts.Add(Part(PartType.Casing, 1, R, 2, 0));
            d.parts.Add(Part(PartType.Glass, 0, Y, 2, 1));
            d.parts.Add(Part(PartType.Dial, 0, B, 3, 1));
            ResolveHidden(d, false);
            return d;
        }
    }
}
