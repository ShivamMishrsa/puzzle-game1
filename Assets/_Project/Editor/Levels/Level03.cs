using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Obstruction: first 2x2 blocks and gray scrap-iron corners; metallic bells with 4 HP.</summary>
    public sealed class Level03 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(3, "Obstruction", 5, 5, 7, 5, LevelTier.Normal, "Gray scrap iron never moves. Clear the blue block first!");
            d.placements.Add(Scrap(1, 1, 0, 0));
            d.placements.Add(Scrap(1, 1, 4, 0));
            d.placements.Add(Scrap(1, 1, 0, 4));
            d.placements.Add(Scrap(1, 1, 4, 4));
            d.placements.Add(Rect(R, 2, 2, 1, 0));
            d.placements.Add(Rect(B, 2, 2, 1, 2));
            d.placements.Add(Rect(B, 1, 2, 3, 0));
            d.gates.Add(Gate(Up, 1, 2, R));
            d.gates.Add(Gate(Right, 2, 2, B));
            d.gates.Add(Gate(Down, 3, 1, B));
            d.parts.Add(Part(PartType.Bell, 0, R, 4, 0));
            d.parts.Add(Part(PartType.Bell, 1, B, 4, 0));
            ResolveHidden(d, false);
            return d;
        }
    }
}
