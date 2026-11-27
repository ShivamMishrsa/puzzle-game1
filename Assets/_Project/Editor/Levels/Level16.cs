using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Domino Cascade: one central blue block thaws three iced reds and opens the shutter for a 4-block combo chain.</summary>
    public sealed class Level16 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(16, "Domino Cascade", 7, 7, 14, 5, LevelTier.Normal, "Slide the central blue block out: three ice shells crack and the shutter opens!");
            d.placements.Add(Rect(B, 2, 2, 2, 0));
            d.placements.Add(Rect(B, 2, 2, 2, 2));
            d.placements.Add(Ell(R, 3, 5, 2));
            d.placements.Add(Rect(B, 1, 2, 6, 5));
            d.placements.Add(Rect(R, 2, 1, 2, 5));
            d.placements.Add(Ice(Rect(R, 1, 2, 4, 4), 1));
            d.placements.Add(Ice(Rect(R, 1, 2, 1, 2), 1));
            d.placements.Add(Ice(Rect(R, 2, 1, 0, 0), 1));
            d.placements.Add(Rect(R, 1, 1, 0, 3));
            d.gates.Add(Gate(Down, 2, 2, B));
            d.gates.Add(Shutter(Left, 2, 2, R, false));
            d.gates.Add(Gate(Right, 2, 2, R));
            d.gates.Add(Gate(Up, 2, 2, R));
            d.parts.Add(Part(PartType.Door, 0, R, 3, 0));
            d.parts.Add(Part(PartType.Casing, 0, B, 4, 0));
            d.parts.Add(Part(PartType.Cassette, 0, R, 3, 1));
            ResolveHidden(d, false);
            return d;
        }
    }
}
