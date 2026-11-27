using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Pre-Boss Chronometer: 2-layer ice, twin portals, shutter gates and three brass screws around scrap-iron obstacles.</summary>
    public sealed class Level39 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(39, "Pre-Boss Chronometer", 7, 7, 17, 4, LevelTier.Normal, "Ice, portals, shutters and three screws - the chronometer wants everything.");
            d.placements.Add(Scrap(1, 1, 3, 3));
            d.placements.Add(Scrap(1, 1, 2, 5));
            d.placements.Add(Ice(Rect(G, 2, 2, 5, 3), 2));
            d.placements.Add(Rect(B, 1, 2, 1, 1));
            d.placements.Add(Rect(Y, 2, 1, 4, 1));
            d.placements.Add(Rect(B, 2, 1, 0, 4));
            d.placements.Add(Rect(R, 2, 1, 4, 0));
            d.placements.Add(Rect(G, 1, 1, 5, 6));
            d.placements.Add(Rect(R, 1, 1, 1, 5));
            d.placements.Add(Rect(Y, 1, 1, 3, 0));
            d.placements.Add(Rect(R, 1, 1, 2, 2));
            d.placements.Add(Rect(Y, 1, 1, 6, 1));
            d.portals.Add(Pad(0, 1, 3, 5, 3));
            d.portals.Add(Pad(1, 3, 1, 3, 5));
            d.gates.Add(Shutter(Left, 4, 2, R, true));
            d.gates.Add(Gate(Down, 4, 2, R));
            d.gates.Add(Gate(Right, 0, 2, Y));
            d.gates.Add(Shutter(Up, 4, 2, Y, false));
            d.gates.Add(Gate(Up, 0, 2, B));
            d.gates.Add(Gate(Left, 0, 3, B));
            d.gates.Add(Gate(Down, 0, 3, G));
            d.gates.Add(Gate(Right, 4, 3, G));
            d.parts.Add(Screw(Part(PartType.Casing, 0, Y, 3, 0), B));
            d.parts.Add(Screw(Part(PartType.Casing, 1, R, 3, 0), G));
            d.parts.Add(Screw(Part(PartType.Dial, 0, B, 3, 1), R));
            d.parts.Add(Part(PartType.BalanceWheel, 0, G, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, Y, 1, 2));
            ResolveHidden(d, false);
            return d;
        }
    }
}
