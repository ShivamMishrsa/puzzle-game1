using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Warp Cascade (flow breather): one warp-and-exit thaws three ice blocks and opens the shutter, starting a 4-block combo.</summary>
    public sealed class Level36 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(36, "Warp Cascade", 7, 7, 6, 5, LevelTier.Normal, "Slide the centre block onto the cyan pad: it warps straight out of the left gate and thaws everything around it!");
            d.placements.Add(Scrap(1, 1, 1, 1));
            d.placements.Add(Scrap(1, 1, 5, 1));
            d.placements.Add(Scrap(1, 1, 1, 5));
            d.placements.Add(Scrap(1, 1, 5, 5));
            d.placements.Add(Rect(B, 1, 1, 3, 3));
            d.placements.Add(Ice(Rect(R, 1, 1, 3, 4), 1));
            d.placements.Add(Ice(Rect(R, 1, 1, 3, 2), 1));
            d.placements.Add(Ice(Rect(R, 1, 1, 4, 3), 1));
            d.portals.Add(Pad(0, 2, 3, 0, 3));
            d.gates.Add(Gate(Left, 3, 1, B));
            d.gates.Add(Gate(Up, 3, 1, R));
            d.gates.Add(Gate(Right, 3, 1, R));
            d.gates.Add(Shutter(Down, 3, 1, R, false));
            d.parts.Add(Part(PartType.Casing, 0, R, 3, 0));
            d.parts.Add(Part(PartType.Casing, 1, B, 1, 0));
            ResolveHidden(d, false);
            return d;
        }
    }
}
