using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Overdrive Surge Showcase: three blue exits in a row (one peeled) trigger Color-Chain Overdrive and melt two screws.</summary>
    public sealed class Level24 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(24, "Overdrive Surge", 6, 6, 17, 5, LevelTier.Normal, "Exit three blue blocks in a row for an OVERDRIVE SURGE - free rapid fire unscrews both bolts!");
            d.placements.Add(Scrap(1, 1, 2, 3));
            d.placements.Add(Peeled(Rect(B, 2, 1, 2, 2), R));
            d.placements.Add(Rect(G, 2, 1, 4, 1));
            d.placements.Add(Rect(R, 2, 1, 0, 5));
            d.placements.Add(Rect(B, 1, 2, 5, 3));
            d.placements.Add(Rect(Y, 1, 1, 1, 1));
            d.placements.Add(Rect(Y, 1, 1, 3, 5));
            d.placements.Add(Rect(B, 1, 1, 0, 2));
            d.gates.Add(Gate(Down, 0, 3, B));
            d.gates.Add(Gate(Up, 3, 3, B));
            d.gates.Add(Gate(Right, 0, 3, R));
            d.gates.Add(Gate(Left, 3, 3, R));
            d.gates.Add(Gate(Down, 3, 3, G));
            d.gates.Add(Gate(Left, 0, 2, Y));
            d.gates.Add(Gate(Right, 4, 2, Y));
            d.parts.Add(Screw(Part(PartType.Handset, 0, R, 3, 0), B));
            d.parts.Add(Screw(Part(PartType.Handset, 1, G, 2, 0), B));
            d.parts.Add(Part(PartType.Cradle, 0, Y, 2, 0));
            d.parts.Add(Part(PartType.Dial, 0, B, 3, 1));
            ResolveHidden(d, false);
            return d;
        }
    }
}
