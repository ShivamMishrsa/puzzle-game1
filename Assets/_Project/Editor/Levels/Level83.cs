using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Repulsor Ping-Pong (Orrery): four repulsors in a diamond bounce blocks across one-way ratchets; one wrong slide strands a block for good.</summary>
    public sealed class Level83 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(83, "Ping-Pong", 8, 8, 18, 4, LevelTier.Normal, "Four coils bounce you around the ratchets - plan the rebounds!");
            d.turnstiles.Add(Turnstile(3, 3, Right));
            d.turnstiles.Add(Turnstile(4, 3, Up));
            d.turnstiles.Add(Turnstile(4, 4, Left));
            d.turnstiles.Add(Turnstile(3, 4, Down));
            d.repulsors.Add(Repulsor(3, 2));
            d.repulsors.Add(Repulsor(5, 3));
            d.repulsors.Add(Repulsor(4, 5));
            d.repulsors.Add(Repulsor(2, 4));
            d.placements.Add(Scrap(1, 1, 6, 6));
            d.placements.Add(Scrap(1, 1, 1, 6));
            d.placements.Add(Rect(P, 2, 2, 4, 6));
            d.placements.Add(Rect(O, 2, 2, 2, 4));
            d.placements.Add(Rect(C, 2, 2, 2, 2));
            d.placements.Add(Rect(G, 2, 2, 0, 3));
            d.placements.Add(Rect(R, 2, 2, 4, 0));
            d.placements.Add(Rect(B, 2, 2, 4, 3));
            d.placements.Add(Rect(Y, 2, 2, 2, 0));
            d.placements.Add(Rect(C, 1, 2, 7, 2));
            d.placements.Add(Rect(O, 2, 1, 5, 2));
            d.placements.Add(Rect(O, 1, 1, 0, 6));
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
            d.parts.Add(Part(PartType.Ring, 0, O, 3, 0));
            d.parts.Add(Part(PartType.Ring, 1, C, 3, 0));
            d.parts.Add(Part(PartType.Ring, 2, Y, 3, 0));
            d.parts.Add(Part(PartType.Ring, 3, R, 3, 0));
            d.parts.Add(Part(PartType.Planet, 0, B, 3, 1));
            d.parts.Add(Part(PartType.Planet, 1, G, 3, 1));
            d.parts.Add(Part(PartType.Planet, 2, P, 3, 1));
            d.parts.Add(Part(PartType.Cage, 0, O, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, C, 2, 3));
            ResolveHidden(d, false);
            return d;
        }
    }
}
