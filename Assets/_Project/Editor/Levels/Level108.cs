using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Locksmith's Clock (Clock Tower): ice, mystery crates, repulsors, one-way ratchets, a key-lock gate and polarity coils inside a tight scrap-iron labyrinth.</summary>
    public sealed class Level108 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(108, "Locksmith's Clock", 10, 10, 29, 4, LevelTier.Normal, "Ice, crates, coils, ratchets and a lock in an iron maze!");
            d.turnstiles.Add(Turnstile(2, 4, Right));
            d.polarities.Add(Polarity(3, 5, true));
            d.repulsors.Add(Repulsor(6, 4));
            d.placements.Add(Scrap(1, 1, 4, 4));
            d.placements.Add(Scrap(1, 1, 7, 7));
            d.placements.Add(Rect(O, 2, 2, 4, 2));
            d.placements.Add(Rect(P, 2, 2, 5, 8));
            d.placements.Add(Rect(Y, 2, 2, 3, 0));
            d.placements.Add(Rect(O, 2, 2, 0, 4));
            d.placements.Add(Rect(G, 2, 2, 1, 2));
            d.placements.Add(Rect(C, 2, 2, 6, 1));
            d.placements.Add(Rect(C, 2, 1, 8, 1));
            d.placements.Add(Ice(Rect(R, 2, 1, 1, 0), 1));
            d.placements.Add(Rect(R, 2, 1, 7, 8));
            d.placements.Add(Rect(B, 1, 2, 3, 8));
            d.placements.Add(Crate(Rect(B, 1, 1, 0, 1)));
            d.placements.Add(Key(Rect(Y, 1, 1, 4, 5), 0));
            d.placements.Add(Rect(B, 1, 1, 1, 1));
            d.placements.Add(Rect(P, 1, 1, 9, 3));
            d.placements.Add(Rect(G, 1, 1, 9, 0));
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
            d.parts.Add(Part(PartType.Hood, 0, O, 4, 0));
            d.parts.Add(Part(PartType.Hood, 1, C, 3, 0));
            d.parts.Add(Part(PartType.ClockDoor, 0, Y, 4, 0));
            d.parts.Add(Part(PartType.Dial, 0, G, 4, 1));
            d.parts.Add(Part(PartType.Pendulum, 0, P, 4, 1));
            d.parts.Add(Part(PartType.Weight, 0, R, 3, 2));
            d.parts.Add(Part(PartType.Weight, 1, B, 3, 2));
            d.parts.Add(Part(PartType.Chime, 0, O, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, C, 2, 3));
            ResolveHidden(d, false);
            d.solution = new int[] { 134743043, 151257093, 134546189, 17367565, 17302531, 151126022, 134348802, 262146, 134415631, 17368335, 83887623, 134284808, 83887624, 133392, 151126032, 201327620, 67109643, 134742026, 201327876, 328964, 134611460, 17302020, 151257090, 17367050, 218106123, 134217737, 83886089, 151060492, 151060494 };
            return d;
        }
    }
}
