using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Express Crucible (Steam Locomotive): EXTREME+. Coils, ratchets, coupled screws, ice, crates, prisms, two portal pairs and a key-lock in one 12x12 maze.</summary>
    public sealed class Level138 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(138, "Express Crucible", 12, 12, 46, 3, LevelTier.Normal, "12x12! Everything at once - you will want the hint!");
            d.turnstiles.Add(Turnstile(2, 4, Right));
            d.polarities.Add(Polarity(3, 7, true));
            d.polarities.Add(Polarity(8, 3, false));
            d.portals.Add(Pad(0, 1, 2, 10, 9));
            d.placements.Add(Scrap(1, 1, 5, 5));
            d.placements.Add(Rect(P, 2, 2, 8, 9));
            d.placements.Add(Rect(C, 2, 2, 3, 7));
            d.placements.Add(Rect(Y, 2, 2, 6, 5));
            d.placements.Add(Rect(C, 2, 2, 3, 0));
            d.placements.Add(Rect(O, 2, 2, 5, 3));
            d.placements.Add(Rect(O, 2, 2, 10, 4));
            d.placements.Add(Rect(B, 2, 2, 0, 7));
            d.placements.Add(Rect(G, 2, 2, 8, 3));
            d.placements.Add(Rect(R, 2, 2, 4, 9));
            d.placements.Add(Ice(Rect(R, 2, 1, 10, 8), 1));
            d.placements.Add(Rect(Y, 2, 1, 10, 3));
            d.placements.Add(Rect(G, 1, 2, 1, 10));
            d.placements.Add(Crate(Rect(B, 1, 1, 2, 7)));
            d.placements.Add(Rect(B, 1, 1, 7, 4));
            d.placements.Add(Rect(G, 1, 1, 6, 11));
            d.placements.Add(Rect(P, 1, 1, 8, 11));
            d.placements.Add(Key(Rect(Y, 1, 1, 10, 6), 0));
            d.gates.Add(Gate(Up, 0, 2, R));
            d.gates.Add(Gate(Down, 0, 2, R));
            d.gates.Add(Gate(Left, 0, 2, B));
            d.gates.Add(Gate(Right, 0, 2, B));
            d.gates.Add(Gate(Down, 2, 2, Y));
            d.gates.Add(Gate(Up, 2, 2, Y));
            d.gates.Add(Padlock(Gate(Right, 2, 2, G), 0));
            d.gates.Add(Gate(Left, 2, 2, G));
            d.gates.Add(Gate(Up, 4, 2, P));
            d.gates.Add(Gate(Down, 4, 2, P));
            d.gates.Add(Gate(Left, 4, 2, O));
            d.gates.Add(Gate(Down, 6, 2, C));
            d.parts.Add(Screws(Part(PartType.Boiler, 0, O, 3, 0), R, B));
            d.parts.Add(Part(PartType.Boiler, 1, C, 3, 0));
            d.parts.Add(Part(PartType.Smokestack, 0, Y, 3, 0));
            d.parts.Add(Part(PartType.Cab, 0, G, 3, 1));
            d.parts.Add(Part(PartType.Piston, 0, P, 3, 1));
            d.parts.Add(Part(PartType.Piston, 1, R, 3, 1));
            d.parts.Add(Part(PartType.Driver, 0, B, 3, 2));
            d.parts.Add(Part(PartType.Driver, 1, O, 3, 2));
            d.parts.Add(Part(PartType.Coal, 0, C, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, Y, 2, 3));
            d.parts.Add(Part(PartType.Tnt, 1, G, 2, 3));
            ResolveHidden(d, false);
            d.solution = new int[] { 201786882, 201328132, 67241221, 134348805, 262149, 201786125, 67175181, 67700239, 134939920, 83887620, 67176456, 67176206, 67241475, 134349315, 218172174, 83887618, 134611729, 395782, 17498897, 151257093, 67108871, 134611206, 67371014, 151257094, 67240204, 134807561, 395787, 134611467, 17498635, 17432585, 17499408, 657409, 134874113, 17433601, 150994951, 218237448, 134283533, 83886595, 151126028, 151060493, 134807567, 67239951, 201919247, 151126031, 134742026, 17498122 };
            return d;
        }
    }
}
