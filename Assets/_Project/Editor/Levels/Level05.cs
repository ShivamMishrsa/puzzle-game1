using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Tighter Tracks (mini-boss): slide the yellow bar out to free the blue block; 4 track slots.</summary>
    public sealed class Level05 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(5, "Tighter Tracks", 5, 5, 8, 4, LevelTier.MiniBoss, "A yellow bar blocks the blue block. Mind the 4 track slots!");
            d.placements.Add(Rect(Y, 1, 2, 0, 0));
            d.placements.Add(Rect(B, 2, 2, 1, 0));
            d.placements.Add(Rect(R, 2, 1, 3, 3));
            d.placements.Add(Rect(R, 1, 1, 2, 4));
            d.placements.Add(Rect(Y, 1, 1, 4, 0));
            d.gates.Add(Gate(Down, 0, 1, Y));
            d.gates.Add(Gate(Left, 0, 2, B));
            d.gates.Add(Gate(Right, 3, 1, R));
            d.gates.Add(Gate(Up, 2, 1, R));
            d.gates.Add(Gate(Right, 0, 1, Y));
            d.parts.Add(Part(PartType.Casing, 0, B, 4, 0));
            d.parts.Add(Part(PartType.Bell, 0, Y, 3, 0));
            d.parts.Add(Part(PartType.Hand, 0, R, 2, 0));
            ResolveHidden(d, false);
            return d;
        }
    }
}
