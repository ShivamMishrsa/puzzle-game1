using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Prism Cadenza (Pipe Organ): EXTREME. Three 4-colour prisms, welded pairs, two portal pairs, three polarity coils, a key-lock and a ratchet - only TWO track slots.</summary>
    public sealed class Level129 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(129, "Prism Cadenza", 11, 11, 42, 3, LevelTier.Normal, "Two slots! Prisms, welded pairs, portals, coils and a lock.");
            d.turnstiles.Add(Turnstile(5, 4, Right));
            d.polarities.Add(Polarity(3, 5, true));
            d.polarities.Add(Polarity(7, 5, false));
            d.polarities.Add(Polarity(5, 8, true));
            d.portals.Add(Pad(0, 1, 2, 9, 8));
            d.portals.Add(Pad(1, 9, 2, 2, 8));
            d.placements.Add(Scrap(1, 2, 3, 2));
            d.placements.Add(Scrap(1, 1, 7, 7));
            d.placements.Add(Rect(R, 2, 2, 3, 7));
            d.placements.Add(Rect(G, 2, 2, 1, 1));
            d.placements.Add(Rect(B, 2, 2, 8, 1));
            d.placements.Add(Rect(Y, 2, 2, 4, 9));
            d.placements.Add(Rect(P, 2, 2, 5, 7));
            d.placements.Add(Rect(O, 2, 2, 0, 5));
            d.placements.Add(Rect(C, 2, 2, 6, 1));
            d.placements.Add(Rect(R, 2, 2, 1, 9));
            d.placements.Add(Rect(O, 2, 2, 2, 4));
            d.placements.Add(Rect(C, 2, 2, 8, 5));
            d.placements.Add(Rect(Y, 2, 1, 2, 0));
            d.placements.Add(Rect(P, 1, 2, 6, 4));
            d.placements.Add(Rect(C, 2, 1, 4, 6));
            d.placements.Add(Prism(Rect(O, 1, 1, 8, 10), O, C, P, R));
            d.placements.Add(Rect(G, 1, 1, 6, 6));
            d.placements.Add(Link(Rect(Y, 1, 1, 7, 9), 1));
            d.placements.Add(Prism(Rect(P, 1, 1, 5, 0), P, R, O, C));
            d.placements.Add(Prism(Rect(C, 1, 1, 5, 3), C, P, R, O));
            d.placements.Add(Link(Rect(G, 1, 1, 7, 10), 1));
            d.placements.Add(Rect(B, 1, 1, 6, 3));
            d.placements.Add(Rect(R, 1, 1, 4, 2));
            d.placements.Add(Key(Rect(B, 1, 1, 0, 1), 0));
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
            d.parts.Add(Part(PartType.Facade, 0, C, 5, 0));
            d.parts.Add(Part(PartType.Facade, 1, O, 4, 0));
            d.parts.Add(Part(PartType.Pipes, 0, Y, 5, 1));
            d.parts.Add(Part(PartType.Pipes, 1, G, 4, 1));
            d.parts.Add(Part(PartType.Keyboard, 0, R, 4, 1));
            d.parts.Add(Part(PartType.Bellows, 0, P, 5, 2));
            d.parts.Add(Part(PartType.Stops, 0, B, 4, 2));
            d.parts.Add(Part(PartType.Stops, 1, C, 4, 2));
            d.parts.Add(Part(PartType.Tnt, 0, R, 3, 3));
            d.parts.Add(Part(PartType.Tnt, 1, O, 3, 3));
            ResolveHidden(d, false);
            d.solution = new int[] { 134676482, 131331, 67110916, 134677510, 67371015, 151257095, 83887624, 134807561, 134808069, 201984271, 134809105, 67175957, 67241485, 263693, 67307024, 134547211, 151126019, 218302992, 218106116, 218171925, 83887627, 17367557, 17367049, 83886604, 17367042, 151257098, 201721358, 83887630, 67241489, 151060503, 17368070, 17368333, 83887378, 83887379, 394769, 134611473, 83886609, 197140, 83886351, 151191572, 201458198, 17432854 };
            return d;
        }
    }
}
