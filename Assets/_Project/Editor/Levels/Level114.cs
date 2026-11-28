using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Twin Portals (Music Box): two portal pairs, a clockwork-piercing gate and a shutter gate with a polarity coil deciding which block gets pulled through - three slots.</summary>
    public sealed class Level114 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(114, "Twin Portals", 11, 11, 26, 3, LevelTier.Normal, "Two wormholes, a piercing gate, a shutter and a coil!");
            d.polarities.Add(Polarity(5, 6, true));
            d.portals.Add(Pad(0, 1, 2, 9, 8));
            d.portals.Add(Pad(1, 9, 2, 2, 8));
            d.placements.Add(Scrap(1, 1, 5, 3));
            d.placements.Add(Scrap(1, 2, 3, 5));
            d.placements.Add(Scrap(2, 1, 7, 7));
            d.placements.Add(Rect(P, 2, 2, 6, 8));
            d.placements.Add(Rect(G, 2, 2, 9, 3));
            d.placements.Add(Rect(O, 2, 2, 0, 0));
            d.placements.Add(Rect(C, 2, 2, 6, 0));
            d.placements.Add(Rect(C, 2, 2, 7, 2));
            d.placements.Add(Rect(B, 2, 2, 0, 2));
            d.placements.Add(Rect(Y, 2, 2, 3, 7));
            d.placements.Add(Rect(R, 2, 2, 1, 7));
            d.placements.Add(Rect(Y, 1, 2, 3, 2));
            d.placements.Add(Rect(R, 2, 1, 1, 10));
            d.placements.Add(Rect(G, 2, 1, 9, 1));
            d.placements.Add(Rect(P, 1, 2, 8, 4));
            d.placements.Add(Rect(O, 1, 1, 6, 5));
            d.placements.Add(Rect(B, 1, 1, 2, 1));
            d.gates.Add(Gate(Up, 0, 2, R));
            d.gates.Add(Gate(Down, 0, 2, R));
            d.gates.Add(Gate(Left, 0, 2, B));
            d.gates.Add(Gate(Right, 0, 2, B));
            d.gates.Add(Gate(Down, 2, 2, Y));
            d.gates.Add(Shutter(Up, 2, 2, Y, false));
            d.gates.Add(Gate(Right, 2, 2, G));
            d.gates.Add(Gate(Left, 2, 2, G));
            d.gates.Add(Gate(Up, 4, 2, P));
            d.gates.Add(Gate(Down, 4, 2, P));
            d.gates.Add(Piercing(Gate(Left, 4, 2, O)));
            d.gates.Add(Gate(Down, 6, 2, C));
            d.parts.Add(Part(PartType.Lid, 0, C, 4, 0));
            d.parts.Add(Part(PartType.Lid, 1, O, 3, 0));
            d.parts.Add(Part(PartType.Crank, 0, R, 2, 0));
            d.parts.Add(Part(PartType.Comb, 0, Y, 4, 1));
            d.parts.Add(Part(PartType.Dancer, 0, G, 4, 1));
            d.parts.Add(Part(PartType.Drum, 0, P, 4, 2));
            d.parts.Add(Part(PartType.Drum, 1, B, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, R, 2, 3));
            d.parts.Add(Part(PartType.Tnt, 1, C, 2, 3));
            ResolveHidden(d, false);
            return d;
        }
    }
}
