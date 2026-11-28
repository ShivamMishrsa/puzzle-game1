using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Boiler Room Blues (Steam Locomotive): EXTREME+. Three polarity coils, two repulsors and a ratchet trio bend every route while coupled screws chain the boiler and the cab. Hints strongly recommended.</summary>
    public sealed class Level131 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(131, "Boiler Blues", 11, 11, 56, 3, LevelTier.Normal, "Extreme+! Coils, repulsors, ratchets and chained bolts - use the hint.");
            d.turnstiles.Add(Turnstile(2, 6, Up));
            d.turnstiles.Add(Turnstile(8, 7, Left));
            d.polarities.Add(Polarity(3, 4, true));
            d.polarities.Add(Polarity(7, 6, false));
            d.polarities.Add(Polarity(5, 8, true));
            d.repulsors.Add(Repulsor(5, 5));
            d.repulsors.Add(Repulsor(8, 3));
            d.placements.Add(Scrap(1, 1, 1, 1));
            d.placements.Add(Scrap(1, 1, 9, 9));
            d.placements.Add(Scrap(1, 2, 9, 3));
            d.placements.Add(Rect(P, 2, 2, 5, 4));
            d.placements.Add(Rect(P, 2, 2, 7, 4));
            d.placements.Add(Rect(G, 2, 2, 7, 8));
            d.placements.Add(Rect(G, 2, 2, 0, 3));
            d.placements.Add(Rect(C, 2, 2, 7, 0));
            d.placements.Add(Rect(C, 2, 2, 5, 8));
            d.placements.Add(Rect(O, 2, 2, 0, 5));
            d.placements.Add(Rect(O, 2, 2, 2, 0));
            d.placements.Add(Rect(Y, 2, 2, 7, 6));
            d.placements.Add(Rect(B, 2, 2, 9, 6));
            d.placements.Add(Rect(R, 2, 2, 3, 4));
            d.placements.Add(Rect(B, 2, 2, 7, 2));
            d.placements.Add(Rect(Y, 2, 2, 0, 9));
            d.placements.Add(Rect(R, 2, 1, 3, 6));
            d.placements.Add(Rect(C, 2, 1, 3, 7));
            d.placements.Add(Rect(O, 1, 2, 6, 6));
            d.placements.Add(Rect(G, 2, 1, 9, 5));
            d.placements.Add(Rect(Y, 1, 2, 5, 6));
            d.placements.Add(Rect(C, 1, 1, 2, 4));
            d.placements.Add(Rect(R, 1, 1, 4, 8));
            d.gates.Add(Gate(Up, 0, 2, R));
            d.gates.Add(Gate(Down, 0, 2, R));
            d.gates.Add(Gate(Left, 0, 2, B));
            d.gates.Add(Gate(Right, 0, 2, B));
            d.gates.Add(Gate(Down, 2, 2, Y));
            d.gates.Add(Gate(Up, 2, 2, Y));
            d.gates.Add(Gate(Right, 2, 2, G));
            d.gates.Add(Gate(Left, 2, 2, G));
            d.gates.Add(Gate(Up, 4, 2, P));
            d.gates.Add(Gate(Down, 4, 2, P));
            d.gates.Add(Gate(Left, 4, 2, O));
            d.gates.Add(Gate(Down, 6, 2, C));
            d.parts.Add(Screws(Part(PartType.Boiler, 0, O, 4, 0), R, B, Y));
            d.parts.Add(Part(PartType.Boiler, 1, C, 4, 0));
            d.parts.Add(Part(PartType.Smokestack, 0, Y, 3, 0));
            d.parts.Add(Screws(Part(PartType.Cab, 0, G, 4, 1), P, C));
            d.parts.Add(Part(PartType.Piston, 0, P, 4, 1));
            d.parts.Add(Part(PartType.Piston, 1, R, 3, 1));
            d.parts.Add(Part(PartType.Driver, 0, B, 4, 2));
            d.parts.Add(Part(PartType.Driver, 1, O, 3, 2));
            d.parts.Add(Part(PartType.Coal, 0, C, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, Y, 3, 3));
            d.parts.Add(Part(PartType.Tnt, 1, G, 3, 3));
            ResolveHidden(d, false);
            d.solution = new int[] { 67239942, 134219271, 83887623, 67371017, 67176206, 201916943, 17367567, 134742294, 17432854, 151126022, 151257097, 218106126, 67306755, 134349827, 83887107, 67372562, 67307268, 134547219, 67241732, 134284292, 83887108, 67307283, 67438347, 134612748, 151191571, 134743048, 134743301, 67306253, 134348813, 17367053, 67437328, 134545424, 17432592, 67371793, 201525009, 83887633, 134611732, 134742280, 17367828, 134743045, 67306245, 201852168, 151126021, 201327370, 197386, 67241739, 134283787, 83886603, 393749, 151257098, 151257106, 67110412, 218106124, 83887624, 201721365, 83887893 };
            return d;
        }
    }
}
