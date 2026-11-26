using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Double Thaw & Merge: two iced red bars thaw one after the other and exit back-to-back into a Red Mega-Turret.</summary>
    public sealed class Level12 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(12, "Double Thaw & Merge", 6, 6, 16, 5, LevelTier.Normal, "Thaw both red bars in turn, then send them out back-to-back to merge into a laser!");
            d.placements.Add(Rect(Y, 1, 1, 1, 3));
            d.placements.Add(Ice(Rect(R, 1, 2, 2, 3), 1));
            d.placements.Add(Ice(Rect(R, 1, 2, 3, 3), 1));
            d.placements.Add(Rect(B, 2, 1, 0, 5));
            d.placements.Add(Rect(Y, 1, 1, 5, 0));
            d.placements.Add(Rect(B, 1, 1, 1, 4));
            d.gates.Add(Gate(Left, 3, 1, Y));
            d.gates.Add(Gate(Down, 0, 3, Y));
            d.gates.Add(Gate(Up, 2, 2, R));
            d.gates.Add(Gate(Right, 0, 3, B));
            d.gates.Add(Gate(Up, 4, 2, B));
            d.parts.Add(Part(PartType.Door, 0, R, 4, 0));
            d.parts.Add(Part(PartType.Casing, 0, B, 3, 0));
            d.parts.Add(Part(PartType.Casing, 1, Y, 2, 0));
            ResolveHidden(d, false);
            return d;
        }
    }
}
