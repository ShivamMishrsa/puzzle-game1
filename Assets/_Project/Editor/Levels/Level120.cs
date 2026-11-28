using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>CHAPTER 12 BOSS: the full four-layer Automaton Music Box. Only two track slots and a very tight clock. Rotate it for the rear bolts, chain polarity coils, a portal, a key-lock, a ratchet and a repulsor with zero wasted moves, then detonate the dynamite behind the pinned discs.</summary>
    public sealed class Level120 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(120, "Automaton Finale", 11, 11, 35, 2, LevelTier.ChapterBoss, "Two slots, rear bolts, coils, portal, key, ratchet - exact moves only!");
            d.reward = "Chapter 12 Complete: Automaton Music Box Dismantled! The Music Box trophy joins your workshop shelf - 120 levels cracked.";
            d.turnstiles.Add(Turnstile(6, 4, Right));
            d.polarities.Add(Polarity(5, 5, true));
            d.polarities.Add(Polarity(2, 4, false));
            d.polarities.Add(Polarity(8, 8, true));
            d.repulsors.Add(Repulsor(7, 6));
            d.portals.Add(Pad(0, 1, 9, 9, 1));
            d.placements.Add(Scrap(1, 2, 3, 2));
            d.placements.Add(Scrap(1, 1, 8, 5));
            d.placements.Add(Rect(Y, 2, 2, 6, 2));
            d.placements.Add(Rect(C, 2, 2, 6, 0));
            d.placements.Add(Rect(P, 2, 2, 4, 1));
            d.placements.Add(Rect(O, 2, 2, 2, 4));
            d.placements.Add(Rect(B, 2, 2, 8, 1));
            d.placements.Add(Rect(G, 2, 2, 8, 3));
            d.placements.Add(Rect(R, 2, 2, 0, 7));
            d.placements.Add(Rect(P, 1, 2, 5, 8));
            d.placements.Add(Rect(G, 2, 1, 1, 3));
            d.placements.Add(Rect(R, 2, 1, 5, 6));
            d.placements.Add(Rect(Y, 1, 2, 4, 7));
            d.placements.Add(Rect(O, 2, 1, 2, 1));
            d.placements.Add(Rect(C, 1, 1, 2, 0));
            d.placements.Add(Rect(P, 1, 1, 1, 5));
            d.placements.Add(Rect(B, 1, 1, 1, 1));
            d.placements.Add(Rect(R, 1, 1, 2, 9));
            d.placements.Add(Rect(Y, 1, 1, 3, 0));
            d.placements.Add(Rect(G, 1, 1, 2, 2));
            d.placements.Add(Key(Rect(B, 1, 1, 6, 5), 0));
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
            d.parts.Add(Screws(Part(PartType.Lid, 0, O, 4, 0), R, B));
            d.parts.Add(Screws(Part(PartType.Lid, 1, C, 3, 0), G, Y));
            d.parts.Add(Part(PartType.Crank, 0, Y, 2, 0));
            d.parts.Add(Part(PartType.Comb, 0, G, 4, 1));
            d.parts.Add(Part(PartType.Dancer, 0, P, 4, 1));
            d.parts.Add(Part(PartType.Drum, 0, R, 4, 2));
            d.parts.Add(Part(PartType.Drum, 1, B, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, Y, 2, 3));
            d.parts.Add(Part(PartType.Tnt, 1, P, 1, 3));
            ResolveHidden(d, false);
            d.solution = new int[] { 67110918, 67241991, 134610955, 134677260, 17367820, 151060496, 134283533, 83886866, 151126035, 83887619, 67110402, 591380, 218237191, 151191562, 655889, 134873361, 17432849, 201589509, 262413, 151257101, 67174671, 151257093, 83887108, 218106118, 17367048, 83887369, 17432587, 201393167, 83887119, 201328386, 201328142, 83887630, 134218242, 83886594, 151060500 };
            d.showRotateHint = true;
            return d;
        }
    }
}
