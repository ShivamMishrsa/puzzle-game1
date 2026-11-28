using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Exposed Pipework (Pipe Organ): EXTREME. The facade is already gone - the pipe banks, keyboard, bellows and stop rails stand exposed behind chained coupled screws, ratchets and three polarity coils.</summary>
    public sealed class Level126 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(126, "Exposed Pipework", 11, 11, 45, 3, LevelTier.Normal, "Extreme! No facade - but triple-chained bolts, ratchets and coils.");
            d.turnstiles.Add(Turnstile(2, 6, Up));
            d.turnstiles.Add(Turnstile(6, 3, Right));
            d.turnstiles.Add(Turnstile(8, 5, Left));
            d.polarities.Add(Polarity(3, 3, true));
            d.polarities.Add(Polarity(7, 7, false));
            d.polarities.Add(Polarity(4, 8, true));
            d.placements.Add(Scrap(1, 1, 5, 5));
            d.placements.Add(Scrap(1, 2, 1, 8));
            d.placements.Add(Scrap(2, 1, 8, 2));
            d.placements.Add(Rect(P, 2, 2, 6, 3));
            d.placements.Add(Rect(G, 2, 2, 5, 8));
            d.placements.Add(Rect(Y, 2, 2, 2, 0));
            d.placements.Add(Rect(O, 2, 2, 3, 7));
            d.placements.Add(Rect(O, 2, 2, 2, 5));
            d.placements.Add(Rect(C, 2, 2, 6, 1));
            d.placements.Add(Rect(R, 2, 2, 8, 6));
            d.placements.Add(Rect(B, 2, 2, 9, 0));
            d.placements.Add(Rect(R, 2, 2, 0, 1));
            d.placements.Add(Rect(Y, 2, 2, 3, 9));
            d.placements.Add(Rect(B, 2, 1, 6, 10));
            d.placements.Add(Rect(P, 2, 1, 4, 6));
            d.placements.Add(Rect(C, 1, 2, 2, 9));
            d.placements.Add(Rect(G, 1, 2, 2, 3));
            d.placements.Add(Rect(C, 1, 1, 6, 6));
            d.placements.Add(Rect(O, 1, 1, 0, 5));
            d.placements.Add(Rect(B, 1, 1, 9, 8));
            d.placements.Add(Rect(G, 1, 1, 1, 3));
            d.placements.Add(Rect(P, 1, 1, 2, 2));
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
            d.parts.Add(Screws(Part(PartType.Pipes, 0, C, 4, 0), R, B, Y));
            d.parts.Add(Screws(Part(PartType.Pipes, 1, O, 4, 0), G, P));
            d.parts.Add(Part(PartType.Keyboard, 0, Y, 4, 0));
            d.parts.Add(Part(PartType.Bellows, 0, R, 4, 1));
            d.parts.Add(Part(PartType.Stops, 0, G, 3, 1));
            d.parts.Add(Part(PartType.Stops, 1, P, 3, 1));
            d.parts.Add(Part(PartType.Tnt, 0, B, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 1, O, 2, 2));
            ResolveHidden(d, false);
            d.solution = new int[] { 83886597, 460305, 134743827, 151191572, 201523984, 134415363, 67371527, 83887107, 151322642, 83887624, 83887889, 67110675, 150994963, 83886091, 151257095, 201458704, 201458709, 83887125, 67568143, 134808076, 17367564, 150994954, 151126032, 201852932, 590598, 201786895, 83887887, 67569165, 134676493, 67174413, 151060493, 525326, 134611721, 67306249, 134349065, 83886089, 67504132, 134611716, 67175172, 201852430, 67371782, 151126020, 151257094, 134808590, 17433614 };
            return d;
        }
    }
}
