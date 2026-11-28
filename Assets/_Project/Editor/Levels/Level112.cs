using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Teeth of the Comb (Music Box): the dome and the comb are chained down by coupled screws - up to three linked bolts - while a ring of one-way ratchets and two polarity coils guard the middle. Only three track slots.</summary>
    public sealed class Level112 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(112, "Comb Teeth", 11, 11, 35, 3, LevelTier.Normal, "Three slots, linked bolts, a ratchet ring and coils!");
            d.turnstiles.Add(Turnstile(5, 5, Right));
            d.turnstiles.Add(Turnstile(6, 5, Up));
            d.turnstiles.Add(Turnstile(6, 6, Left));
            d.turnstiles.Add(Turnstile(5, 6, Down));
            d.polarities.Add(Polarity(3, 3, true));
            d.polarities.Add(Polarity(8, 8, false));
            d.placements.Add(Scrap(1, 2, 3, 6));
            d.placements.Add(Scrap(2, 1, 7, 2));
            d.placements.Add(Scrap(1, 1, 9, 5));
            d.placements.Add(Rect(O, 2, 2, 0, 2));
            d.placements.Add(Rect(P, 2, 2, 5, 7));
            d.placements.Add(Rect(G, 2, 2, 7, 9));
            d.placements.Add(Rect(C, 2, 2, 6, 0));
            d.placements.Add(Rect(C, 2, 2, 0, 0));
            d.placements.Add(Rect(O, 2, 2, 7, 7));
            d.placements.Add(Rect(B, 2, 2, 1, 9));
            d.placements.Add(Rect(Y, 2, 2, 7, 4));
            d.placements.Add(Rect(R, 2, 2, 9, 1));
            d.placements.Add(Rect(R, 2, 1, 1, 8));
            d.placements.Add(Rect(P, 1, 2, 5, 9));
            d.placements.Add(Rect(G, 2, 1, 1, 4));
            d.placements.Add(Rect(B, 1, 2, 6, 2));
            d.placements.Add(Rect(Y, 1, 1, 3, 9));
            d.placements.Add(Rect(P, 1, 1, 3, 2));
            d.placements.Add(Rect(R, 1, 1, 5, 5));
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
            d.parts.Add(Screws(Part(PartType.Lid, 0, O, 4, 0), R, B, Y));
            d.parts.Add(Part(PartType.Lid, 1, C, 3, 0));
            d.parts.Add(Part(PartType.Crank, 0, Y, 2, 0));
            d.parts.Add(Screws(Part(PartType.Comb, 0, G, 4, 1), P, C));
            d.parts.Add(Part(PartType.Dancer, 0, P, 4, 1));
            d.parts.Add(Part(PartType.Drum, 0, R, 4, 2));
            d.parts.Add(Part(PartType.Drum, 1, B, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, C, 2, 3));
            d.parts.Add(Part(PartType.Tnt, 1, O, 2, 3));
            ResolveHidden(d, false);
            d.solution = new int[] { 134677508, 67307274, 198923, 134742028, 201589262, 262147, 17433360, 201458705, 151257091, 134545682, 83887622, 201328135, 83887623, 67110415, 83886354, 150994959, 17368333, 17368068, 151191566, 134415114, 67307272, 83886602, 134415368, 83887121, 262920, 67307269, 134415621, 67241221, 151126021, 83886092, 67109129, 150994953, 151257096, 134414603, 17367051 };
            return d;
        }
    }
}
