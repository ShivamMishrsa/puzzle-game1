using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>MINI-BOSS Signal Tower: EXTREME+. First 12x12 tray. Coupled screws, two padlocked gates, a portal pair, three polarity coils, a repulsor and a ratchet - zero spare moves.</summary>
    public sealed class Level135 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(135, "Signal Tower", 12, 12, 53, 3, LevelTier.MiniBoss, "12x12! Coupled bolts, padlocks, portal, coils - the hint is essential.");
            d.turnstiles.Add(Turnstile(6, 4, Right));
            d.polarities.Add(Polarity(5, 5, true));
            d.polarities.Add(Polarity(2, 9, false));
            d.polarities.Add(Polarity(9, 3, true));
            d.repulsors.Add(Repulsor(7, 7));
            d.portals.Add(Pad(0, 1, 10, 10, 1));
            d.placements.Add(Scrap(1, 2, 3, 2));
            d.placements.Add(Scrap(2, 1, 8, 6));
            d.placements.Add(Rect(C, 2, 2, 0, 10));
            d.placements.Add(Rect(G, 2, 2, 7, 9));
            d.placements.Add(Rect(Y, 2, 2, 1, 1));
            d.placements.Add(Rect(P, 2, 2, 5, 9));
            d.placements.Add(Rect(O, 2, 2, 0, 4));
            d.placements.Add(Rect(O, 2, 2, 8, 7));
            d.placements.Add(Rect(C, 2, 2, 10, 7));
            d.placements.Add(Rect(G, 2, 2, 9, 10));
            d.placements.Add(Rect(R, 2, 2, 8, 1));
            d.placements.Add(Rect(B, 2, 2, 2, 4));
            d.placements.Add(Rect(Y, 2, 2, 6, 6));
            d.placements.Add(Rect(O, 1, 2, 7, 3));
            d.placements.Add(Rect(R, 2, 1, 1, 0));
            d.placements.Add(Rect(B, 1, 2, 2, 9));
            d.placements.Add(Rect(P, 2, 1, 5, 0));
            d.placements.Add(Rect(C, 1, 1, 0, 2));
            d.placements.Add(Key(Rect(G, 1, 1, 5, 6), 1));
            d.placements.Add(Key(Rect(B, 1, 1, 0, 1), 0));
            d.placements.Add(Rect(P, 1, 1, 6, 1));
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
            d.parts.Add(Screws(Part(PartType.Cab, 0, G, 4, 1), P, O));
            d.parts.Add(Part(PartType.Piston, 0, P, 4, 1));
            d.parts.Add(Part(PartType.Piston, 1, R, 3, 1));
            d.parts.Add(Part(PartType.Driver, 0, B, 4, 2));
            d.parts.Add(Part(PartType.Driver, 1, O, 3, 2));
            d.parts.Add(Part(PartType.Coal, 0, C, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, Y, 3, 3));
            d.parts.Add(Part(PartType.Tnt, 1, G, 3, 3));
            ResolveHidden(d, false);
            d.solution = new int[] { 134808581, 67242504, 201984521, 67110922, 201589515, 67241741, 134350093, 134350344, 67242505, 201918979, 134217742, 134218768, 151060499, 67174417, 134284564, 83887624, 17433605, 83887120, 83887380, 151257094, 67371010, 83886094, 67109124, 201327108, 83886596, 67109391, 218237449, 67242499, 218237443, 526343, 134742535, 67371527, 201591307, 67111435, 197901, 218106379, 134480909, 67372306, 134611468, 218303250, 17433100, 201328399, 201393681, 132625, 150994959, 134217738, 83886090, 67108866, 201328130, 151257095, 83887618, 151257101, 83887633 };
            d.showRotateHint = true;
            return d;
        }
    }
}
