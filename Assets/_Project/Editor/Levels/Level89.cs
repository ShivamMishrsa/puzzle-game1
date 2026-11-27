using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Pre-Boss Grandmaster Lock: 2-layer ice, mystery crates, repulsors, one-way ratchets and a key-lock gate inside a tight scrap-iron labyrinth.</summary>
    public sealed class Level89 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(89, "Grandmaster Lock", 9, 9, 25, 4, LevelTier.Normal, "Ice, crates, coils, ratchets and a lock in an iron maze!");
            d.turnstiles.Add(Turnstile(2, 4, Right));
            d.repulsors.Add(Repulsor(3, 5));
            d.placements.Add(Scrap(1, 1, 4, 4));
            d.placements.Add(Rect(O, 2, 2, 5, 6));
            d.placements.Add(Rect(C, 2, 2, 6, 1));
            d.placements.Add(Rect(G, 2, 1, 6, 4));
            d.placements.Add(Rect(P, 1, 2, 5, 2));
            d.placements.Add(Rect(O, 2, 1, 1, 5));
            d.placements.Add(Rect(Y, 1, 2, 7, 7));
            d.placements.Add(Ice(Rect(R, 2, 1, 0, 7), 1));
            d.placements.Add(Rect(B, 2, 1, 3, 3));
            d.placements.Add(Crate(Rect(B, 1, 1, 8, 1)));
            d.placements.Add(Rect(C, 1, 1, 1, 6));
            d.placements.Add(Key(Rect(Y, 1, 1, 3, 2), 0));
            d.placements.Add(Rect(G, 1, 1, 0, 2));
            d.placements.Add(Rect(R, 1, 1, 8, 3));
            d.placements.Add(Rect(P, 1, 1, 2, 4));
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
            d.parts.Add(Part(PartType.Ring, 0, O, 3, 0));
            d.parts.Add(Part(PartType.Ring, 1, C, 3, 0));
            d.parts.Add(Part(PartType.Ring, 2, Y, 3, 0));
            d.parts.Add(Part(PartType.Ring, 3, G, 3, 0));
            d.parts.Add(Part(PartType.Planet, 0, P, 3, 1));
            d.parts.Add(Part(PartType.Planet, 1, R, 3, 1));
            d.parts.Add(Part(PartType.Planet, 2, B, 3, 1));
            d.parts.Add(Part(PartType.Cage, 0, O, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, C, 2, 3));
            ResolveHidden(d, false);
            return d;
        }
    }
}
