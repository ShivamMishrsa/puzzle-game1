using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Gears Visible: the housing is already gone, copper gears exposed; chain exits for combos.</summary>
    public sealed class Level08 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(8, "Gears Visible", 6, 6, 14, 3, LevelTier.Normal, "The housing is gone! Hit every gear - chain exits for combo notes.");
            d.placements.Add(Scrap(1, 1, 2, 3));
            d.placements.Add(Scrap(1, 1, 3, 2));
            d.placements.Add(Rect(R, 2, 2, 1, 4));
            d.placements.Add(Ell(B, 3, 0, 1));
            d.placements.Add(Ell(Y, 0, 3, 3));
            d.placements.Add(Rect(R, 1, 1, 5, 2));
            d.gates.Add(Gate(Down, 0, 2, R));
            d.gates.Add(Gate(Right, 5, 1, R));
            d.gates.Add(Gate(Up, 3, 2, B));
            d.gates.Add(Gate(Left, 3, 2, Y));
            d.parts.Add(Part(PartType.Gear, 0, R, 3, 0));
            d.parts.Add(Part(PartType.Gear, 1, B, 3, 0));
            d.parts.Add(Part(PartType.Gear, 2, Y, 3, 0));
            d.parts.Add(Part(PartType.Hand, 0, R, 2, 0));
            ResolveHidden(d, true);
            return d;
        }
    }
}
