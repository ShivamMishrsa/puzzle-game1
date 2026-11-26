using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Ice Awakening: a 1-layer iced blue bar sits beside a red block; sending the red block out cracks the ice.</summary>
    public sealed class Level11 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(11, "Ice Awakening", 6, 6, 14, 5, LevelTier.Normal, "Frozen blocks cannot move. Send the red block next to the ice out - the shell cracks!");
            d.placements.Add(Ice(Rect(B, 1, 2, 2, 2), 1));
            d.placements.Add(Rect(R, 1, 1, 3, 2));
            d.placements.Add(Rect(B, 2, 1, 2, 5));
            d.placements.Add(Rect(R, 1, 2, 0, 4));
            d.placements.Add(Rect(R, 2, 1, 1, 4));
            d.placements.Add(Rect(B, 1, 1, 4, 0));
            d.gates.Add(Gate(Right, 2, 1, R));
            d.gates.Add(Gate(Up, 3, 2, R));
            d.gates.Add(Gate(Left, 0, 2, R));
            d.gates.Add(Gate(Up, 2, 1, B));
            d.gates.Add(Gate(Down, 0, 3, B));
            d.gates.Add(Gate(Right, 4, 2, B));
            d.parts.Add(Part(PartType.Door, 0, R, 3, 0));
            d.parts.Add(Part(PartType.Casing, 0, B, 3, 0));
            d.parts.Add(Part(PartType.Cassette, 0, R, 2, 1));
            ResolveHidden(d, false);
            return d;
        }
    }
}
