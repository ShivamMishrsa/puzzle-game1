using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>MINI-BOSS Stop-Knob Lock: EXTREME. Only TWO track slots. Coupled screws on the facade and pipes, two padlocked gates, a portal pair, two polarity coils and a ratchet. Zero spare moves.</summary>
    public sealed class Level125 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(125, "Stop-Knob Lock", 11, 11, 35, 3, LevelTier.MiniBoss, "Only two slots! Coupled bolts, padlocks, portal, coils - hint recommended.");
            d.polarities.Add(Polarity(5, 5, true));
            d.polarities.Add(Polarity(2, 8, false));
            d.portals.Add(Pad(0, 1, 9, 9, 1));
            d.placements.Add(Scrap(1, 2, 3, 2));
            d.placements.Add(Scrap(2, 1, 7, 6));
            d.placements.Add(Rect(Y, 2, 2, 2, 9));
            d.placements.Add(Rect(C, 2, 2, 6, 1));
            d.placements.Add(Rect(O, 2, 2, 2, 4));
            d.placements.Add(Rect(P, 2, 2, 0, 0));
            d.placements.Add(Rect(O, 2, 2, 0, 4));
            d.placements.Add(Rect(B, 2, 2, 1, 7));
            d.placements.Add(Rect(G, 2, 2, 9, 2));
            d.placements.Add(Rect(R, 2, 2, 9, 6));
            d.placements.Add(Rect(G, 1, 2, 0, 6));
            d.placements.Add(Rect(C, 2, 1, 5, 4));
            d.placements.Add(Rect(R, 2, 1, 0, 2));
            d.placements.Add(Rect(O, 1, 1, 3, 0));
            d.placements.Add(Rect(C, 1, 1, 5, 7));
            d.placements.Add(Rect(P, 1, 1, 1, 3));
            d.placements.Add(Rect(B, 1, 1, 10, 1));
            d.placements.Add(Rect(R, 1, 1, 1, 10));
            d.placements.Add(Key(Rect(G, 1, 1, 0, 3), 1));
            d.placements.Add(Rect(G, 1, 1, 8, 3));
            d.placements.Add(Key(Rect(B, 1, 1, 8, 1), 0));
            d.placements.Add(Rect(Y, 1, 1, 6, 0));
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
            d.solution = new int[] { 461065, 201590283, 134218253, 201786894, 218171920, 17432849, 151257094, 262415, 134218517, 83887619, 218237192, 83887627, 83887630, 83886869, 151191570, 17367554, 218302995, 201590276, 262669, 201327621, 83887109, 83886092, 67239946, 151126026, 151060500, 201786375, 67109895, 150994951, 134676489, 17367049, 328205, 151322637, 201589775, 17433615, 151257092 };
            d.showRotateHint = true;
            return d;
        }
    }
}
