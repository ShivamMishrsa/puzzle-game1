using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Pathways: L-shapes and vertical 2x1 blocks, 12 HP of clock to chew through.</summary>
    public sealed class Level06 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(6, "Pathways", 6, 6, 9, 4, LevelTier.Normal, "L-shaped blocks need a gate as wide as their box.");
            d.placements.Add(Ell(R, 0, 0, 4));
            d.placements.Add(Rect(R, 1, 2, 5, 4));
            d.placements.Add(Ell(B, 1, 2, 2));
            d.placements.Add(Rect(B, 1, 1, 0, 0));
            d.placements.Add(Rect(Y, 1, 2, 4, 0));
            d.placements.Add(Rect(Y, 1, 1, 4, 2));
            d.gates.Add(Gate(Up, 0, 2, R));
            d.gates.Add(Gate(Up, 5, 1, R));
            d.gates.Add(Gate(Right, 2, 2, B));
            d.gates.Add(Gate(Left, 0, 1, B));
            d.gates.Add(Gate(Down, 4, 1, Y));
            d.gates.Add(Gate(Up, 4, 1, Y));
            d.parts.Add(Part(PartType.Casing, 0, R, 5, 0));
            d.parts.Add(Part(PartType.Bell, 0, B, 4, 0));
            d.parts.Add(Part(PartType.Hand, 0, Y, 3, 0));
            ResolveHidden(d, false);
            return d;
        }
    }
}
