using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Bridge & Portal: a scrap wall splits the tray. The welded Red+Yellow pair snaps at the left red gate; the freed Yellow 1x1 can then warp through the cyan portal to the yellow gate on the other side.</summary>
    public sealed class Level43 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(43, "Bridge and Portal", 7, 7, 14, 5, LevelTier.Normal, "Snap the weld first - then the freed 1x1 can use the portal pad.");
            d.placements.Add(Scrap(1, 7, 3, 0));
            d.placements.Add(Link(Rect(R, 1, 1, 1, 3), 1));
            d.placements.Add(Link(Rect(Y, 1, 1, 2, 3), 1));
            d.placements.Add(Rect(G, 2, 1, 0, 4));
            d.placements.Add(Rect(B, 1, 2, 5, 1));
            d.placements.Add(Rect(Y, 2, 1, 4, 5));
            d.placements.Add(Rect(R, 1, 2, 1, 1));
            d.placements.Add(Rect(Y, 1, 1, 6, 2));
            d.placements.Add(Rect(B, 1, 1, 6, 0));
            d.placements.Add(Rect(R, 1, 1, 6, 5));
            d.portals.Add(Pad(0, 1, 2, 5, 2));
            d.gates.Add(Gate(Left, 3, 1, R));
            d.gates.Add(Gate(Down, 0, 2, R));
            d.gates.Add(Gate(Right, 2, 1, Y));
            d.gates.Add(Gate(Up, 4, 2, Y));
            d.gates.Add(Gate(Left, 0, 2, B));
            d.gates.Add(Gate(Right, 4, 2, B));
            d.gates.Add(Gate(Up, 0, 2, G));
            d.gates.Add(Gate(Down, 4, 2, G));
            d.parts.Add(Part(PartType.Boiler, 0, R, 2, 0));
            d.parts.Add(Part(PartType.Gauge, 0, Y, 2, 0));
            d.parts.Add(Part(PartType.Lever, 0, B, 1, 0));
            d.parts.Add(Part(PartType.GroupHead, 0, B, 2, 1));
            d.parts.Add(Part(PartType.DripGrid, 0, G, 2, 1));
            d.parts.Add(Part(PartType.Valve, 0, R, 2, 2));
            d.parts.Add(Part(PartType.Tnt, 0, Y, 2, 2));
            ResolveHidden(d, false);
            return d;
        }
    }
}
