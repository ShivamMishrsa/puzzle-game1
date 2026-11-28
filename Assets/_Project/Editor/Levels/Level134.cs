using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Coal Chute Lock (Steam Locomotive): EXTREME+. Two key-lock gates with padlocks, coupled screws on two parts, a portal pair, two polarity coils and a repulsor - the keys must be freed in exactly the right order.</summary>
    public sealed class Level134 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(134, "Coal Chute Lock", 11, 11, 55, 3, LevelTier.Normal, "Extreme+! Two padlocked gates, keys, coupled bolts and a portal.");
            d.polarities.Add(Polarity(5, 5, true));
            d.polarities.Add(Polarity(2, 8, false));
            d.repulsors.Add(Repulsor(7, 3));
            d.portals.Add(Pad(0, 1, 9, 9, 1));
            d.placements.Add(Scrap(1, 2, 3, 2));
            d.placements.Add(Scrap(2, 1, 7, 6));
            d.placements.Add(Rect(C, 2, 2, 6, 8));
            d.placements.Add(Rect(G, 2, 2, 8, 8));
            d.placements.Add(Rect(Y, 2, 2, 3, 6));
            d.placements.Add(Rect(P, 2, 2, 6, 4));
            d.placements.Add(Rect(O, 2, 2, 4, 8));
            d.placements.Add(Rect(O, 2, 2, 1, 5));
            d.placements.Add(Rect(C, 2, 2, 5, 6));
            d.placements.Add(Rect(G, 2, 2, 1, 1));
            d.placements.Add(Rect(R, 2, 2, 1, 7));
            d.placements.Add(Rect(B, 2, 2, 2, 9));
            d.placements.Add(Rect(Y, 2, 2, 4, 1));
            d.placements.Add(Rect(O, 1, 2, 10, 0));
            d.placements.Add(Rect(R, 2, 1, 7, 0));
            d.placements.Add(Rect(B, 1, 2, 8, 2));
            d.placements.Add(Rect(P, 2, 1, 3, 5));
            d.placements.Add(Rect(C, 1, 1, 2, 0));
            d.placements.Add(Key(Rect(G, 1, 1, 9, 0), 1));
            d.placements.Add(Key(Rect(B, 1, 1, 0, 3), 0));
            d.placements.Add(Rect(P, 1, 1, 7, 7));
            d.gates.Add(Gate(Up, 0, 2, R));
            d.gates.Add(Gate(Down, 0, 2, R));
            d.gates.Add(Gate(Left, 0, 2, B));
            d.gates.Add(Gate(Right, 0, 2, B));
            d.gates.Add(Gate(Down, 2, 2, Y));
            d.gates.Add(Padlock(Gate(Up, 2, 2, Y), 1));
            d.gates.Add(Gate(Right, 2, 2, G));
            d.gates.Add(Gate(Left, 2, 2, G));
            d.gates.Add(Padlock(Gate(Up, 4, 2, P), 0));
            d.gates.Add(Gate(Down, 4, 2, P));
            d.gates.Add(Gate(Left, 4, 2, O));
            d.gates.Add(Gate(Down, 6, 2, C));
            d.parts.Add(Screws(Part(PartType.Boiler, 0, O, 4, 0), R, B));
            d.parts.Add(Part(PartType.Boiler, 1, C, 4, 0));
            d.parts.Add(Part(PartType.Smokestack, 0, Y, 3, 0));
            d.parts.Add(Part(PartType.Cab, 0, G, 4, 1));
            d.parts.Add(Part(PartType.Piston, 0, P, 4, 1));
            d.parts.Add(Part(PartType.Piston, 1, R, 3, 1));
            d.parts.Add(Screws(Part(PartType.Driver, 0, B, 4, 2), P, O));
            d.parts.Add(Part(PartType.Driver, 1, O, 3, 2));
            d.parts.Add(Part(PartType.Coal, 0, C, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, Y, 3, 3));
            d.parts.Add(Part(PartType.Tnt, 1, G, 3, 3));
            ResolveHidden(d, false);
            d.solution = new int[] { 201853187, 67242243, 201853186, 67373314, 67371271, 131337, 134676490, 134611460, 134807819, 67109900, 201655312, 67174419, 151060499, 218237187, 151126025, 151257095, 83886090, 134481922, 264717, 201329170, 133650, 218237458, 67307013, 67241221, 134481410, 201328910, 67110927, 83887618, 134807563, 17367556, 67108875, 150994955, 134611464, 67371528, 134677780, 134742790, 17433876, 17433616, 17368069, 134742278, 67371014, 151257094, 328716, 201328145, 83887633, 150994959, 134217742, 17432590, 134479880, 134545932, 17367564, 67108872, 201328136, 83887624, 151257101 };
            return d;
        }
    }
}
