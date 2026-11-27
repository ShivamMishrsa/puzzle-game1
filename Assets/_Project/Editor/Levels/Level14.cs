using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Thick Frost: a 2-layer iced 2x2 yellow block flanked by scrap iron - thaw it from above and below.</summary>
    public sealed class Level14 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(14, "Thick Frost", 7, 7, 21, 5, LevelTier.Normal, "Two layers of ice: free blocks on two different sides to thaw it!");
            d.placements.Add(Scrap(1, 2, 1, 2));
            d.placements.Add(Scrap(1, 2, 4, 2));
            d.placements.Add(Rect(B, 2, 2, 2, 4));
            d.placements.Add(Ice(Rect(Y, 2, 2, 5, 1), 2));
            d.placements.Add(Rect(B, 1, 2, 5, 5));
            d.placements.Add(Rect(Y, 2, 1, 2, 6));
            d.placements.Add(Rect(R, 2, 1, 0, 4));
            d.placements.Add(Rect(B, 2, 1, 3, 1));
            d.placements.Add(Rect(R, 1, 1, 4, 5));
            d.placements.Add(Rect(R, 1, 1, 5, 4));
            d.placements.Add(Rect(B, 1, 1, 1, 1));
            d.gates.Add(Gate(Left, 4, 1, R));
            d.gates.Add(Gate(Right, 1, 1, B));
            d.gates.Add(Gate(Up, 2, 2, Y));
            d.gates.Add(Gate(Up, 4, 2, R));
            d.gates.Add(Gate(Down, 0, 2, B));
            d.parts.Add(Part(PartType.Door, 0, R, 3, 0));
            d.parts.Add(Part(PartType.Casing, 0, B, 3, 0));
            d.parts.Add(Part(PartType.Casing, 1, Y, 4, 0));
            ResolveHidden(d, false);
            return d;
        }
    }
}
