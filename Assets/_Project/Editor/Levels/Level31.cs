using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Portal Warp Introduction: a scrap-iron wall splits the tray; a portal pair warps a trapped 1x1 red block into the red gate on the right. Larger blocks roll over pads without warping.</summary>
    public sealed class Level31 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(31, "Portal Warp", 6, 6, 16, 5, LevelTier.Normal, "Only 1x1 blocks warp. Slide the lone red block onto the cyan pad - it jumps to the other side of the wall!");
            d.showRotateHint = true;
            d.placements.Add(Scrap(1, 6, 2, 0));
            d.placements.Add(Rect(R, 1, 1, 0, 2));
            d.placements.Add(Rect(Y, 2, 2, 0, 3));
            d.placements.Add(Rect(R, 2, 1, 3, 3));
            d.placements.Add(Rect(B, 2, 1, 4, 1));
            d.placements.Add(Rect(B, 1, 2, 4, 4));
            d.placements.Add(Rect(Y, 1, 1, 5, 2));
            d.portals.Add(Pad(0, 1, 2, 4, 2));
            d.gates.Add(Gate(Right, 2, 1, R));
            d.gates.Add(Gate(Up, 3, 2, R));
            d.gates.Add(Gate(Left, 0, 3, B));
            d.gates.Add(Gate(Right, 3, 3, B));
            d.gates.Add(Gate(Down, 3, 2, B));
            d.gates.Add(Gate(Down, 0, 2, Y));
            d.gates.Add(Gate(Up, 0, 2, Y));
            d.gates.Add(Gate(Right, 0, 2, Y));
            d.parts.Add(Part(PartType.Casing, 0, Y, 3, 0));
            d.parts.Add(Part(PartType.Casing, 1, R, 2, 0));
            d.parts.Add(Part(PartType.Dial, 0, B, 3, 1));
            ResolveHidden(d, false);
            return d;
        }
    }
}
