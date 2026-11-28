using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Grand Crucible II (Pipe Organ): EXTREME. Coils, ratchets, coupled screws, ice, crates, prisms, two portal pairs and a key-lock in one 11x11 maze on three slots.</summary>
    public sealed class Level128 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(128, "Crucible II", 11, 11, 36, 3, LevelTier.Normal, "Everything at once on three slots - you will want the hint!");
            d.turnstiles.Add(Turnstile(2, 4, Right));
            d.polarities.Add(Polarity(3, 7, true));
            d.polarities.Add(Polarity(7, 3, false));
            d.portals.Add(Pad(0, 1, 2, 9, 8));
            d.placements.Add(Scrap(1, 1, 5, 5));
            d.placements.Add(Rect(B, 2, 2, 7, 1));
            d.placements.Add(Rect(O, 2, 2, 0, 4));
            d.placements.Add(Rect(P, 2, 2, 9, 8));
            d.placements.Add(Rect(G, 2, 2, 5, 9));
            d.placements.Add(Rect(G, 2, 2, 5, 7));
            d.placements.Add(Rect(C, 2, 2, 1, 9));
            d.placements.Add(Rect(Y, 2, 2, 9, 5));
            d.placements.Add(Rect(R, 2, 2, 0, 0));
            d.placements.Add(Rect(O, 2, 2, 3, 0));
            d.placements.Add(Rect(C, 2, 1, 2, 5));
            d.placements.Add(Ice(Rect(R, 2, 1, 8, 10), 1));
            d.placements.Add(Key(Rect(Y, 1, 1, 3, 8), 0));
            d.placements.Add(Crate(Rect(B, 1, 1, 4, 7)));
            d.placements.Add(Rect(C, 1, 1, 7, 0));
            d.placements.Add(Prism(Rect(O, 1, 1, 4, 10), O, C, P, R));
            d.placements.Add(Rect(B, 1, 1, 10, 1));
            d.placements.Add(Rect(P, 1, 1, 9, 2));
            d.placements.Add(Rect(R, 1, 1, 1, 2));
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
            d.parts.Add(Screws(Part(PartType.Facade, 0, O, 4, 0), R, B));
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
            d.solution = new int[] { 201917956, 201786885, 395527, 393738, 83887886, 67110657, 218171920, 67241477, 218106113, 151257090, 83886088, 134217737, 67175437, 201786890, 134349829, 83887626, 134611463, 17433356, 67175940, 134743043, 201916934, 17432850, 151126021, 262153, 151060493, 17433615, 17368067, 201917958, 17367559, 151126020, 83887622, 151257097, 134873099, 17432587, 134350097, 83887377 };
            return d;
        }
    }
}
