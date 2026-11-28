using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Frozen Fugue (Pipe Organ): EXTREME. Three ice-sealed 4-colour prisms, two mystery crates, a clockwork carriage row and two polarity coils - a single wrong thaw order ruins the whole route.</summary>
    public sealed class Level122 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(122, "Frozen Fugue", 11, 11, 29, 3, LevelTier.Normal, "Extreme! Ice, crates, prisms and a carriage - the order is everything.");
            d.carriages.Add(Carriage(3, 1));
            d.polarities.Add(Polarity(5, 5, true));
            d.polarities.Add(Polarity(8, 7, false));
            d.placements.Add(Scrap(1, 1, 2, 7));
            d.placements.Add(Scrap(1, 1, 8, 4));
            d.placements.Add(Scrap(1, 1, 6, 8));
            d.placements.Add(Rect(B, 2, 2, 0, 0));
            d.placements.Add(Rect(P, 2, 2, 9, 0));
            d.placements.Add(Rect(G, 2, 2, 8, 9));
            d.placements.Add(Rect(G, 2, 2, 2, 2));
            d.placements.Add(Rect(R, 2, 2, 0, 9));
            d.placements.Add(Rect(C, 2, 2, 3, 4));
            d.placements.Add(Rect(Y, 2, 2, 2, 0));
            d.placements.Add(Rect(O, 2, 2, 0, 5));
            d.placements.Add(Rect(O, 2, 2, 6, 2));
            d.placements.Add(Rect(R, 2, 1, 0, 8));
            d.placements.Add(Rect(C, 1, 2, 2, 5));
            d.placements.Add(Prism(Ice(Rect(C, 1, 1, 1, 3), 2), C, P, R, O));
            d.placements.Add(Prism(Ice(Rect(O, 1, 1, 4, 2), 2), O, C, P, R));
            d.placements.Add(Prism(Ice(Rect(P, 1, 1, 2, 4), 2), P, R, O, C));
            d.placements.Add(Rect(Y, 1, 1, 2, 8));
            d.placements.Add(Crate(Rect(B, 1, 1, 8, 1)));
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
            d.parts.Add(Part(PartType.Facade, 0, O, 4, 0));
            d.parts.Add(Part(PartType.Facade, 1, C, 3, 0));
            d.parts.Add(Part(PartType.Pipes, 0, G, 4, 1));
            d.parts.Add(Part(PartType.Pipes, 1, P, 3, 1));
            d.parts.Add(Part(PartType.Keyboard, 0, Y, 3, 1));
            d.parts.Add(Part(PartType.Bellows, 0, R, 4, 2));
            d.parts.Add(Part(PartType.Stops, 0, B, 3, 2));
            d.parts.Add(Part(PartType.Stops, 1, O, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, C, 2, 3));
            d.parts.Add(Part(PartType.Tnt, 1, G, 2, 3));
            ResolveHidden(d, false);
            d.solution = new int[] { 134808581, 67371018, 151257098, 201590280, 17367047, 17432588, 83886601, 17433105, 67110411, 134218763, 83887624, 201656077, 83887885, 67372037, 201524485, 218237189, 201327883, 197899, 134284050, 134218756, 83887108, 150994947, 17433615, 218237190, 201524750, 83887630, 151060498, 151257104, 151257099 };
            return d;
        }
    }
}
