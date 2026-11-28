using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Welded Wheels (Steam Locomotive): EXTREME+. Four welded pairs, three polarity coils, two repulsors and two ratchets pushing and pulling at once on the way to shutter gates - 12x12.</summary>
    public sealed class Level137 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(137, "Welded Wheels", 12, 12, 57, 3, LevelTier.Normal, "12x12! Welded halves, coils and repulsors all at once.");
            d.turnstiles.Add(Turnstile(2, 4, Right));
            d.polarities.Add(Polarity(3, 5, true));
            d.polarities.Add(Polarity(7, 4, false));
            d.polarities.Add(Polarity(5, 2, true));
            d.repulsors.Add(Repulsor(5, 7));
            d.repulsors.Add(Repulsor(8, 3));
            d.placements.Add(Scrap(1, 2, 2, 2));
            d.placements.Add(Scrap(2, 1, 8, 9));
            d.placements.Add(Rect(R, 2, 2, 7, 10));
            d.placements.Add(Rect(G, 2, 2, 1, 0));
            d.placements.Add(Rect(P, 2, 2, 8, 1));
            d.placements.Add(Rect(B, 2, 2, 3, 10));
            d.placements.Add(Rect(O, 2, 2, 1, 5));
            d.placements.Add(Rect(Y, 2, 2, 5, 7));
            d.placements.Add(Rect(C, 2, 2, 0, 8));
            d.placements.Add(Rect(Y, 2, 1, 10, 4));
            d.placements.Add(Rect(P, 2, 1, 1, 7));
            d.placements.Add(Rect(G, 1, 2, 4, 1));
            d.placements.Add(Rect(C, 1, 2, 0, 5));
            d.placements.Add(Rect(C, 1, 1, 3, 3));
            d.placements.Add(Link(Rect(P, 1, 1, 10, 7), 2));
            d.placements.Add(Link(Rect(G, 1, 1, 9, 5), 3));
            d.placements.Add(Link(Rect(C, 1, 1, 2, 11), 1));
            d.placements.Add(Link(Rect(Y, 1, 1, 11, 6), 2));
            d.placements.Add(Link(Rect(O, 1, 1, 0, 11), 1));
            d.placements.Add(Rect(Y, 1, 1, 11, 9));
            d.placements.Add(Link(Rect(R, 1, 1, 9, 4), 3));
            d.placements.Add(Rect(B, 1, 1, 3, 9));
            d.gates.Add(Gate(Up, 0, 2, R));
            d.gates.Add(Gate(Down, 0, 2, R));
            d.gates.Add(Gate(Left, 0, 2, B));
            d.gates.Add(Gate(Right, 0, 2, B));
            d.gates.Add(Gate(Down, 2, 2, Y));
            d.gates.Add(Gate(Up, 2, 2, Y));
            d.gates.Add(Gate(Right, 2, 2, G));
            d.gates.Add(Gate(Left, 2, 2, G));
            d.gates.Add(Gate(Up, 4, 2, P));
            d.gates.Add(Shutter(Down, 4, 2, P, false));
            d.gates.Add(Shutter(Left, 4, 2, O, true));
            d.gates.Add(Gate(Down, 6, 2, C));
            d.parts.Add(Part(PartType.Boiler, 0, O, 3, 0));
            d.parts.Add(Part(PartType.Boiler, 1, C, 3, 0));
            d.parts.Add(Part(PartType.Smokestack, 0, P, 3, 0));
            d.parts.Add(Part(PartType.Cab, 0, G, 3, 1));
            d.parts.Add(Part(PartType.Piston, 0, Y, 3, 1));
            d.parts.Add(Part(PartType.Piston, 1, R, 3, 1));
            d.parts.Add(Part(PartType.Driver, 0, B, 3, 2));
            d.parts.Add(Part(PartType.Driver, 1, Y, 3, 2));
            d.parts.Add(Part(PartType.Coal, 0, C, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, P, 2, 3));
            d.parts.Add(Part(PartType.Tnt, 1, G, 2, 3));
            ResolveHidden(d, false);
            d.solution = new int[] { 134217731, 131075, 330249, 132107, 134547215, 67241743, 658195, 151126019, 201656326, 201655820, 83887628, 134545414, 67371014, 151257094, 134546185, 134677511, 134677774, 17499406, 67502346, 134676999, 201720842, 67175181, 201393677, 83887629, 67372561, 134480657, 83886865, 67373843, 134480659, 83886867, 134349835, 83887114, 201917461, 67175445, 151060501, 201982981, 67175429, 150994949, 202049040, 67438096, 134873090, 17432578, 17433095, 201852424, 17498633, 460559, 134676751, 134284292, 218237707, 83886356, 131343, 151126031, 151322642, 83887624, 17433604, 201655824, 83887632 };
            return d;
        }
    }
}
