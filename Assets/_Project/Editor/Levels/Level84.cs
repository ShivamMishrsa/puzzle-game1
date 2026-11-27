using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Frozen Prism (Orrery): two prism blocks cycle Orange, Cyan, Purple, Red while sealed in 2-layer ice between scrap-iron pillars.</summary>
    public sealed class Level84 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(84, "Frozen Prism", 8, 8, 31, 4, LevelTier.Normal, "The prisms keep shifting while frozen - thaw them on the right colour!");
            d.placements.Add(Scrap(1, 1, 2, 3));
            d.placements.Add(Scrap(1, 1, 5, 3));
            d.placements.Add(Scrap(1, 1, 2, 5));
            d.placements.Add(Scrap(1, 1, 5, 5));
            d.placements.Add(Rect(Y, 2, 2, 4, 0));
            d.placements.Add(Rect(O, 2, 2, 0, 5));
            d.placements.Add(Rect(P, 2, 2, 2, 1));
            d.placements.Add(Rect(B, 2, 2, 6, 1));
            d.placements.Add(Rect(G, 2, 2, 0, 0));
            d.placements.Add(Rect(C, 2, 2, 6, 4));
            d.placements.Add(Rect(R, 2, 2, 3, 5));
            d.placements.Add(Rect(C, 2, 1, 5, 7));
            d.placements.Add(Prism(Ice(Rect(O, 1, 1, 7, 7), 2), O, C, P, R));
            d.placements.Add(Prism(Ice(Rect(C, 1, 1, 6, 3), 2), C, P, R, O));
            d.placements.Add(Rect(O, 1, 1, 2, 6));
            d.gates.Add(Gate(Up, 0, 2, R));
            d.gates.Add(Gate(Down, 0, 2, R));
            d.gates.Add(Gate(Left, 0, 2, B));
            d.gates.Add(Gate(Right, 0, 2, B));
            d.gates.Add(Gate(Down, 2, 2, Y));
            d.gates.Add(Gate(Up, 2, 2, Y));
            d.gates.Add(Gate(Right, 2, 2, G));
            d.gates.Add(Gate(Left, 2, 2, G));
            d.gates.Add(Gate(Up, 4, 2, P));
            d.gates.Add(Gate(Down, 4, 2, P));
            d.gates.Add(Gate(Left, 4, 2, O));
            d.gates.Add(Gate(Down, 6, 2, C));
            d.parts.Add(Part(PartType.Ring, 0, O, 3, 0));
            d.parts.Add(Part(PartType.Ring, 1, C, 3, 0));
            d.parts.Add(Part(PartType.Ring, 2, P, 3, 0));
            d.parts.Add(Part(PartType.Ring, 3, R, 3, 0));
            d.parts.Add(Part(PartType.Planet, 0, Y, 3, 1));
            d.parts.Add(Part(PartType.Planet, 1, G, 3, 1));
            d.parts.Add(Part(PartType.Planet, 2, B, 3, 1));
            d.parts.Add(Part(PartType.Cage, 0, C, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, O, 2, 3));
            ResolveHidden(d, false);
            return d;
        }
    }
}
