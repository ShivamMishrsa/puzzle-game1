using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Signal Cadenza (Steam Locomotive): EXTREME+. Three prisms, welded pairs, two portal pairs, three polarity coils, a key-lock and a ratchet on a 12x12 tray.</summary>
    public sealed class Level139 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(139, "Signal Cadenza", 12, 12, 51, 3, LevelTier.Normal, "12x12! Prisms, welded pairs, portals, coils and a lock.");
            d.turnstiles.Add(Turnstile(5, 4, Right));
            d.polarities.Add(Polarity(3, 5, true));
            d.polarities.Add(Polarity(8, 5, false));
            d.polarities.Add(Polarity(5, 9, true));
            d.portals.Add(Pad(0, 1, 2, 10, 9));
            d.portals.Add(Pad(1, 10, 2, 2, 9));
            d.placements.Add(Scrap(1, 2, 3, 2));
            d.placements.Add(Scrap(1, 1, 8, 8));
            d.placements.Add(Rect(R, 2, 2, 5, 1));
            d.placements.Add(Rect(R, 2, 2, 9, 4));
            d.placements.Add(Rect(P, 2, 2, 9, 8));
            d.placements.Add(Rect(G, 2, 2, 10, 10));
            d.placements.Add(Rect(Y, 2, 2, 7, 2));
            d.placements.Add(Rect(O, 2, 2, 10, 2));
            d.placements.Add(Rect(C, 2, 2, 1, 5));
            d.placements.Add(Rect(B, 2, 2, 5, 9));
            d.placements.Add(Rect(Y, 2, 1, 7, 6));
            d.placements.Add(Rect(O, 1, 2, 0, 5));
            d.placements.Add(Rect(C, 2, 1, 10, 0));
            d.placements.Add(Rect(C, 1, 1, 1, 3));
            d.placements.Add(Link(Rect(Y, 1, 1, 1, 10), 1));
            d.placements.Add(Prism(Rect(P, 1, 1, 8, 9), P, R, O, C));
            d.placements.Add(Prism(Rect(C, 1, 1, 11, 8), C, P, R, O));
            d.placements.Add(Prism(Rect(O, 1, 1, 3, 8), O, C, P, R));
            d.placements.Add(Key(Rect(B, 1, 1, 1, 8), 0));
            d.placements.Add(Link(Rect(G, 1, 1, 3, 10), 1));
            d.gates.Add(Gate(Up, 0, 2, R));
            d.gates.Add(Gate(Down, 0, 2, R));
            d.gates.Add(Gate(Left, 0, 2, B));
            d.gates.Add(Gate(Right, 0, 2, B));
            d.gates.Add(Gate(Down, 2, 2, Y));
            d.gates.Add(Gate(Up, 2, 2, Y));
            d.gates.Add(Gate(Right, 2, 2, G));
            d.gates.Add(Gate(Left, 2, 2, G));
            d.gates.Add(Padlock(Gate(Up, 4, 2, P), 0));
            d.gates.Add(Gate(Down, 4, 2, P));
            d.gates.Add(Gate(Left, 4, 2, O));
            d.gates.Add(Gate(Down, 6, 2, C));
            d.parts.Add(Part(PartType.Boiler, 0, C, 3, 0));
            d.parts.Add(Part(PartType.Boiler, 1, O, 3, 0));
            d.parts.Add(Part(PartType.Smokestack, 0, Y, 3, 0));
            d.parts.Add(Part(PartType.Cab, 0, G, 3, 1));
            d.parts.Add(Part(PartType.Piston, 0, R, 3, 1));
            d.parts.Add(Part(PartType.Piston, 1, P, 3, 1));
            d.parts.Add(Part(PartType.Driver, 0, B, 3, 2));
            d.parts.Add(Part(PartType.Driver, 1, C, 3, 2));
            d.parts.Add(Part(PartType.Coal, 0, R, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, O, 2, 3));
            d.parts.Add(Part(PartType.Tnt, 1, Y, 2, 3));
            ResolveHidden(d, false);
            d.solution = new int[] { 67110146, 134217730, 329987, 67110662, 134218246, 67371019, 151257099, 201917961, 134219276, 67111431, 83887628, 262413, 201590285, 83887629, 67698958, 67437329, 83886082, 151257105, 83886598, 134217991, 67111427, 67504388, 201457935, 67439376, 67242244, 134349828, 83887108, 134875397, 67242245, 218237445, 460554, 201656072, 83887624, 134677002, 67110409, 393487, 262407, 150994953, 134217731, 151257095, 151322640, 201918734, 17498634, 67307790, 218303251, 67436815, 151322639, 83886606, 67174930, 218172178, 17432579 };
            return d;
        }
    }
}
