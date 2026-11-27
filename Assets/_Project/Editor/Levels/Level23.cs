using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Steel Guide Rails: horizontal-only and vertical-only rail bars guard the handset.</summary>
    public sealed class Level23 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(23, "Steel Guide Rails", 6, 6, 12, 5, LevelTier.Normal, "Rail blocks only slide along their steel guide. Arrows show the direction.");
            d.placements.Add(Scrap(1, 1, 2, 3));
            d.placements.Add(Scrap(1, 1, 3, 2));
            d.placements.Add(Rect(B, 2, 2, 4, 2));
            d.placements.Add(Rect(Y, 2, 1, 0, 2));
            d.placements.Add(Arrow(Rect(R, 2, 1, 2, 1), Horizontal));
            d.placements.Add(Arrow(Rect(G, 1, 2, 1, 3), Vertical));
            d.placements.Add(Rect(Y, 1, 1, 4, 0));
            d.placements.Add(Rect(R, 1, 1, 4, 1));
            d.placements.Add(Rect(G, 1, 1, 3, 4));
            d.gates.Add(Gate(Left, 3, 1, R));
            d.gates.Add(Gate(Right, 1, 1, R));
            d.gates.Add(Gate(Down, 4, 2, R));
            d.gates.Add(Gate(Up, 1, 1, G));
            d.gates.Add(Gate(Down, 1, 1, G));
            d.gates.Add(Gate(Right, 4, 1, G));
            d.gates.Add(Gate(Up, 3, 2, B));
            d.gates.Add(Gate(Left, 0, 2, B));
            d.gates.Add(Gate(Right, 3, 3, Y));
            d.gates.Add(Gate(Down, 0, 1, Y));
            d.parts.Add(Part(PartType.Handset, 0, R, 3, 0));
            d.parts.Add(Part(PartType.Handset, 1, G, 3, 0));
            d.parts.Add(Part(PartType.Cradle, 0, Y, 2, 0));
            d.parts.Add(Part(PartType.Dial, 0, B, 2, 1));
            ResolveHidden(d, false);
            return d;
        }
    }
}
