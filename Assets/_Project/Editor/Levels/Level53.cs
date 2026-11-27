using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Prism Chain & Merge: two prism blocks (Red->Green and Yellow->Green) can be timed to be Green together and merge into a Twin-Barrel Green Mega-Turret.</summary>
    public sealed class Level53 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(53, "Prism Chain", 7, 7, 12, 5, LevelTier.Normal, "Both prisms turn Green on odd exits - send them out together to merge!");
            d.placements.Add(Scrap(1, 1, 3, 3));
            d.placements.Add(Rect(G, 2, 2, 4, 2));
            d.placements.Add(Rect(B, 2, 2, 3, 4));
            d.placements.Add(Rect(Y, 1, 2, 6, 5));
            d.placements.Add(Rect(R, 1, 2, 1, 4));
            d.placements.Add(Prism(Rect(R, 1, 1, 2, 5), R, G));
            d.placements.Add(Rect(R, 1, 1, 0, 2));
            d.placements.Add(Prism(Rect(Y, 1, 1, 2, 0), Y, G));
            d.placements.Add(Rect(G, 1, 1, 1, 2));
            d.gates.Add(Gate(Down, 0, 2, G));
            d.gates.Add(Gate(Up, 4, 2, G));
            d.gates.Add(Gate(Right, 2, 2, G));
            d.gates.Add(Gate(Left, 4, 2, R));
            d.gates.Add(Gate(Up, 0, 2, R));
            d.gates.Add(Gate(Right, 4, 2, Y));
            d.gates.Add(Gate(Down, 4, 2, Y));
            d.gates.Add(Gate(Left, 0, 2, B));
            d.gates.Add(Gate(Up, 2, 2, B));
            d.parts.Add(Part(PartType.Body, 0, G, 3, 0));
            d.parts.Add(Part(PartType.Hood, 0, G, 2, 0));
            d.parts.Add(Part(PartType.Lens, 0, R, 2, 0));
            d.parts.Add(Part(PartType.Lens, 1, B, 2, 0));
            d.parts.Add(Part(PartType.Iris, 0, Y, 2, 1));
            d.parts.Add(Part(PartType.Crank, 0, R, 1, 1));
            d.parts.Add(Part(PartType.Spool, 0, B, 2, 2));
            d.parts.Add(Part(PartType.Tnt, 0, G, 2, 2));
            ResolveHidden(d, false);
            return d;
        }
    }
}
