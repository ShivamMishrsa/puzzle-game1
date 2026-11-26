using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>The Maze: a central scrap-iron wall forces winding slides; tight 15-move limit.</summary>
    public sealed class Level07 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(7, "The Maze", 6, 6, 15, 3, LevelTier.Normal, "Wind around the central wall - plan your path!");
            d.placements.Add(Scrap(2, 2, 2, 2));
            d.placements.Add(Rect(R, 2, 2, 2, 0));
            d.placements.Add(Ell(B, 0, 4, 2));
            d.placements.Add(Ell(Y, 2, 0, 2));
            d.placements.Add(Rect(R, 1, 2, 3, 4));
            d.gates.Add(Gate(Up, 0, 2, R));
            d.gates.Add(Gate(Down, 4, 2, R));
            d.gates.Add(Gate(Left, 2, 2, B));
            d.gates.Add(Gate(Right, 3, 2, Y));
            d.gates.Add(Gate(Up, 0, 1, B));
            d.gates.Add(Gate(Up, 4, 2, Y));
            d.parts.Add(Part(PartType.Casing, 0, R, 4, 0));
            d.parts.Add(Part(PartType.Hand, 0, R, 2, 0));
            d.parts.Add(Part(PartType.Bell, 0, B, 3, 0));
            d.parts.Add(Part(PartType.Bell, 1, Y, 3, 0));
            ResolveHidden(d, false);
            return d;
        }
    }
}
