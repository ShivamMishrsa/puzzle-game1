using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Twin Portals & Rail Runners: cyan and violet portal pairs on a tray full of horizontal-only and vertical-only rail blocks.</summary>
    public sealed class Level34 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(34, "Twin Portals", 7, 7, 12, 5, LevelTier.Normal, "Two portal pairs and steel rails: 1x1 blocks warp, rail blocks only slide on their guide.");
            d.placements.Add(Scrap(1, 1, 3, 3));
            d.placements.Add(Rect(Y, 2, 2, 3, 5));
            d.placements.Add(Rect(B, 2, 1, 1, 1));
            d.placements.Add(Arrow(Rect(B, 1, 2, 5, 1), Vertical));
            d.placements.Add(Arrow(Rect(R, 1, 2, 1, 4), Vertical));
            d.placements.Add(Arrow(Rect(G, 2, 1, 3, 1), Horizontal));
            d.placements.Add(Rect(Y, 1, 1, 4, 2));
            d.placements.Add(Rect(R, 1, 1, 6, 3));
            d.placements.Add(Rect(G, 1, 1, 1, 3));
            d.portals.Add(Pad(0, 1, 3, 5, 3));
            d.portals.Add(Pad(1, 3, 1, 3, 5));
            d.gates.Add(Gate(Up, 1, 1, R));
            d.gates.Add(Gate(Down, 1, 1, R));
            d.gates.Add(Gate(Right, 4, 2, R));
            d.gates.Add(Gate(Left, 4, 1, G));
            d.gates.Add(Gate(Right, 0, 2, G));
            d.gates.Add(Gate(Up, 5, 1, B));
            d.gates.Add(Gate(Down, 5, 1, B));
            d.gates.Add(Gate(Left, 0, 2, B));
            d.gates.Add(Gate(Up, 3, 2, Y));
            d.gates.Add(Gate(Down, 3, 2, Y));
            d.parts.Add(Part(PartType.Casing, 0, Y, 3, 0));
            d.parts.Add(Part(PartType.Casing, 1, R, 3, 0));
            d.parts.Add(Part(PartType.Glass, 0, B, 2, 1));
            d.parts.Add(Part(PartType.Dial, 0, G, 2, 1));
            ResolveHidden(d, false);
            return d;
        }
    }
}
