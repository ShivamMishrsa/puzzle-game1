using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Locksmith's Music Box (Music Box): ice, mystery crates, two key-lock gates, repulsors and one-way ratchets inside a tight scrap-iron labyrinth - three slots.</summary>
    public sealed class Level118 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(118, "Locksmith's Box", 11, 11, 29, 3, LevelTier.Normal, "Two locks, ice, crates, coils and ratchets in an iron maze!");
            d.turnstiles.Add(Turnstile(2, 4, Right));
            d.turnstiles.Add(Turnstile(8, 6, Left));
            d.polarities.Add(Polarity(3, 6, true));
            d.repulsors.Add(Repulsor(6, 4));
            d.repulsors.Add(Repulsor(4, 8));
            d.placements.Add(Scrap(1, 1, 4, 4));
            d.placements.Add(Scrap(1, 1, 8, 8));
            d.placements.Add(Rect(B, 2, 2, 4, 1));
            d.placements.Add(Rect(R, 2, 2, 0, 8));
            d.placements.Add(Rect(P, 2, 2, 7, 4));
            d.placements.Add(Rect(Y, 2, 2, 2, 9));
            d.placements.Add(Rect(G, 2, 2, 2, 3));
            d.placements.Add(Rect(C, 2, 2, 5, 8));
            d.placements.Add(Rect(O, 2, 2, 9, 0));
            d.placements.Add(Rect(C, 2, 1, 6, 2));
            d.placements.Add(Rect(P, 1, 2, 0, 4));
            d.placements.Add(Ice(Rect(R, 2, 1, 0, 10), 1));
            d.placements.Add(Rect(G, 2, 1, 9, 3));
            d.placements.Add(Crate(Rect(B, 1, 1, 1, 1)));
            d.placements.Add(Rect(C, 1, 1, 7, 1));
            d.placements.Add(Key(Rect(Y, 1, 1, 10, 9), 0));
            d.placements.Add(Key(Rect(O, 1, 1, 1, 5), 1));
            d.placements.Add(Rect(O, 1, 1, 2, 5));
            d.gates.Add(Gate(Up, 0, 2, R));
            d.gates.Add(Gate(Down, 0, 2, R));
            d.gates.Add(Gate(Left, 0, 2, B));
            d.gates.Add(Gate(Right, 0, 2, B));
            d.gates.Add(Gate(Down, 2, 2, Y));
            d.gates.Add(Gate(Up, 2, 2, Y));
            d.gates.Add(Padlock(Gate(Right, 2, 2, G), 0));
            d.gates.Add(Gate(Left, 2, 2, G));
            d.gates.Add(Padlock(Gate(Up, 4, 2, P), 1));
            d.gates.Add(Gate(Down, 4, 2, P));
            d.gates.Add(Gate(Left, 4, 2, O));
            d.gates.Add(Gate(Down, 6, 2, C));
            d.parts.Add(Part(PartType.Lid, 0, O, 4, 0));
            d.parts.Add(Part(PartType.Lid, 1, C, 3, 0));
            d.parts.Add(Part(PartType.Crank, 0, Y, 2, 0));
            d.parts.Add(Part(PartType.Comb, 0, G, 4, 1));
            d.parts.Add(Part(PartType.Dancer, 0, P, 4, 1));
            d.parts.Add(Part(PartType.Drum, 0, R, 4, 2));
            d.parts.Add(Part(PartType.Drum, 1, B, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, C, 2, 3));
            ResolveHidden(d, false);
            d.solution = new int[] { 67109890, 17367557, 67240454, 67568903, 327690, 83887886, 83887625, 67241732, 134349828, 134808335, 17433359, 151126022, 218302732, 17432587, 17367043, 393226, 201721098, 151322640, 151322641, 592136, 134807560, 67371016, 151257096, 17368330, 83887623, 134218242, 83887108, 218106114, 151060493 };
            return d;
        }
    }
}
