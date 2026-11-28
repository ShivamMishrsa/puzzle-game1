using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Exposed Engine (Steam Locomotive): EXTREME+. The boiler shells and smokestack are already gone - the cab, pistons, wheels and coal bunker stand exposed behind chained coupled screws, ratchets and three polarity coils on a 12x12 tray.</summary>
    public sealed class Level136 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(136, "Exposed Engine", 12, 12, 61, 3, LevelTier.Normal, "12x12! No boiler - but chained bolts, ratchets and coils.");
            d.turnstiles.Add(Turnstile(2, 6, Up));
            d.turnstiles.Add(Turnstile(6, 3, Right));
            d.turnstiles.Add(Turnstile(9, 5, Left));
            d.polarities.Add(Polarity(3, 3, true));
            d.polarities.Add(Polarity(8, 8, false));
            d.polarities.Add(Polarity(4, 9, true));
            d.placements.Add(Scrap(1, 1, 5, 5));
            d.placements.Add(Scrap(1, 2, 1, 9));
            d.placements.Add(Scrap(2, 1, 9, 2));
            d.placements.Add(Rect(O, 2, 2, 7, 0));
            d.placements.Add(Rect(Y, 2, 2, 3, 2));
            d.placements.Add(Rect(O, 2, 2, 10, 6));
            d.placements.Add(Rect(P, 2, 2, 3, 8));
            d.placements.Add(Rect(G, 2, 2, 9, 3));
            d.placements.Add(Rect(R, 2, 2, 9, 0));
            d.placements.Add(Rect(R, 2, 2, 8, 8));
            d.placements.Add(Rect(B, 2, 2, 1, 7));
            d.placements.Add(Rect(C, 2, 2, 4, 0));
            d.placements.Add(Rect(C, 2, 1, 10, 11));
            d.placements.Add(Rect(P, 1, 2, 6, 7));
            d.placements.Add(Rect(B, 2, 1, 5, 4));
            d.placements.Add(Rect(G, 2, 1, 0, 11));
            d.placements.Add(Rect(Y, 1, 2, 6, 5));
            d.placements.Add(Rect(B, 1, 1, 8, 6));
            d.placements.Add(Rect(C, 1, 1, 0, 7));
            d.placements.Add(Rect(O, 1, 1, 0, 6));
            d.placements.Add(Rect(P, 1, 1, 11, 10));
            d.placements.Add(Rect(Y, 1, 1, 0, 9));
            d.placements.Add(Rect(G, 1, 1, 9, 6));
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
            d.parts.Add(Screws(Part(PartType.Cab, 0, C, 4, 0), R, B, Y));
            d.parts.Add(Screws(Part(PartType.Piston, 0, O, 4, 0), G, P));
            d.parts.Add(Part(PartType.Piston, 1, Y, 3, 0));
            d.parts.Add(Part(PartType.Driver, 0, R, 4, 1));
            d.parts.Add(Part(PartType.Driver, 1, G, 3, 1));
            d.parts.Add(Part(PartType.Coal, 0, P, 3, 1));
            d.parts.Add(Part(PartType.Tnt, 0, B, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 1, O, 2, 2));
            ResolveHidden(d, false);
            d.solution = new int[] { 197380, 83886596, 201851910, 134416135, 134940428, 134480910, 202048271, 67436563, 134874388, 17499412, 67569673, 720917, 202048021, 17498645, 151322643, 461317, 201722390, 67308054, 218303254, 590854, 201786122, 134677773, 394768, 132107, 134218499, 67110663, 201459211, 134218243, 262659, 134218759, 132103, 134217736, 67176465, 218172177, 151257091, 83886088, 83887627, 151126023, 67175438, 151060494, 67175178, 150994954, 134677517, 83887117, 134611728, 83886864, 201787154, 83887890, 134676489, 134677253, 83886089, 67241740, 134350348, 83887628, 67306245, 197381, 151257093, 202049295, 17433606, 67307279, 218302991 };
            return d;
        }
    }
}
