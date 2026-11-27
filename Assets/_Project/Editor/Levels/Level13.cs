using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Peeled Core Introduction: a yellow block hides a blue core, and thawing the iced blue bar beside it.</summary>
    public sealed class Level13 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(13, "Peeled Core", 7, 7, 21, 5, LevelTier.Normal, "The stripe shows what is inside: the yellow block leaves a blue core behind!");
            d.placements.Add(Rect(R, 1, 2, 5, 3));
            d.placements.Add(Rect(Y, 2, 1, 3, 0));
            d.placements.Add(Rect(B, 2, 1, 2, 2));
            d.placements.Add(Peeled(Rect(Y, 2, 1, 2, 5), B));
            d.placements.Add(Ice(Rect(B, 1, 2, 0, 1), 1));
            d.placements.Add(Rect(R, 1, 2, 6, 4));
            d.placements.Add(Rect(B, 1, 1, 1, 5));
            d.placements.Add(Rect(B, 1, 1, 4, 5));
            d.placements.Add(Rect(R, 1, 1, 5, 1));
            d.gates.Add(Gate(Down, 1, 2, Y));
            d.gates.Add(Gate(Up, 3, 1, B));
            d.gates.Add(Gate(Left, 0, 3, B));
            d.gates.Add(Gate(Up, 4, 2, R));
            d.gates.Add(Gate(Right, 3, 3, R));
            d.parts.Add(Part(PartType.Door, 0, R, 3, 0));
            d.parts.Add(Part(PartType.Casing, 0, B, 4, 0));
            d.parts.Add(Part(PartType.Casing, 1, Y, 2, 0));
            ResolveHidden(d, false);
            return d;
        }
    }
}
