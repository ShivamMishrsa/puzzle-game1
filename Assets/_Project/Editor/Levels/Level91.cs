using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Polarity Parity (Tourbillon): two Magnetic Polarity Flipper Coils glow cyan (PULL) or crimson-orange (PUSH) and flip on every block exit - exits decide whether a resting block is yanked in or shoved away. Exact optimal moves.</summary>
    public sealed class Level91 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(91, "Polarity Parity", 9, 9, 23, 4, LevelTier.Normal, "Coils flip PULL/PUSH after every exit - count your exits!");
            d.polarities.Add(Polarity(3, 4, true));
            d.polarities.Add(Polarity(5, 4, false));
            d.placements.Add(Scrap(1, 1, 1, 1));
            d.placements.Add(Scrap(1, 1, 7, 7));
            d.placements.Add(Rect(P, 2, 2, 1, 4));
            d.placements.Add(Rect(G, 2, 2, 6, 2));
            d.placements.Add(Rect(C, 2, 2, 5, 7));
            d.placements.Add(Rect(O, 2, 2, 1, 2));
            d.placements.Add(Rect(B, 2, 2, 2, 0));
            d.placements.Add(Rect(R, 2, 2, 3, 7));
            d.placements.Add(Rect(Y, 2, 2, 3, 3));
            d.placements.Add(Rect(C, 2, 1, 4, 2));
            d.placements.Add(Rect(Y, 2, 1, 4, 0));
            d.placements.Add(Rect(O, 1, 2, 6, 4));
            d.placements.Add(Rect(O, 1, 1, 0, 5));
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
            d.parts.Add(Part(PartType.Bezel, 0, O, 3, 0));
            d.parts.Add(Part(PartType.Bezel, 1, C, 3, 0));
            d.parts.Add(Part(PartType.Crystal, 0, Y, 3, 0));
            d.parts.Add(Part(PartType.Bridge, 0, G, 3, 1));
            d.parts.Add(Part(PartType.Bridge, 1, P, 3, 1));
            d.parts.Add(Part(PartType.TourbillonCage, 0, R, 3, 1));
            d.parts.Add(Part(PartType.MainBarrel, 0, B, 3, 2));
            d.parts.Add(Part(PartType.Rotor, 0, O, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, C, 2, 3));
            d.parts.Add(Part(PartType.Tnt, 1, Y, 2, 3));
            ResolveHidden(d, false);
            d.solution = new int[] { 134676487, 151322636, 218236675, 201459209, 83887625, 17235975, 134479874, 262920, 17236488, 201589506, 67175426, 151257099, 67372292, 262405, 263428, 201655812, 83887620, 17236994, 151257093, 132106, 218105606, 134349322, 17302026 };
            return d;
        }
    }
}
