using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>CHAPTER 11 BOSS: the full four-layer Grandfather Clock Tower. Only three track slots. Rotate it for the rear bolts, chain polarity coils, a portal, a key-lock, a ratchet and a repulsor with zero wasted moves, then detonate the music-box dynamite.</summary>
    public sealed class Level110 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(110, "Clock Tower Finale", 10, 10, 28, 3, LevelTier.ChapterBoss, "Three slots, rear bolts, coils, portal, key, ratchet - exact moves only!");
            d.reward = "Chapter 11 Complete: Grandfather Clock Tower Dismantled! The Clocktower trophy joins your workshop shelf - 110 levels cracked.";
            d.turnstiles.Add(Turnstile(5, 3, Right));
            d.polarities.Add(Polarity(4, 4, true));
            d.polarities.Add(Polarity(2, 3, false));
            d.polarities.Add(Polarity(7, 7, true));
            d.repulsors.Add(Repulsor(6, 6));
            d.portals.Add(Pad(0, 1, 8, 8, 1));
            d.placements.Add(Scrap(1, 2, 3, 2));
            d.placements.Add(Scrap(1, 1, 7, 5));
            d.placements.Add(Rect(C, 2, 2, 4, 1));
            d.placements.Add(Rect(Y, 2, 2, 3, 6));
            d.placements.Add(Rect(P, 2, 2, 3, 4));
            d.placements.Add(Rect(O, 2, 2, 0, 5));
            d.placements.Add(Rect(O, 2, 2, 7, 7));
            d.placements.Add(Rect(G, 2, 2, 8, 2));
            d.placements.Add(Rect(R, 2, 2, 1, 7));
            d.placements.Add(Rect(Y, 2, 2, 5, 4));
            d.placements.Add(Rect(B, 2, 2, 6, 1));
            d.placements.Add(Rect(P, 1, 2, 5, 7));
            d.placements.Add(Rect(C, 2, 1, 0, 3));
            d.placements.Add(Rect(G, 2, 1, 6, 3));
            d.placements.Add(Rect(C, 1, 1, 7, 0));
            d.placements.Add(Rect(R, 1, 1, 1, 1));
            d.placements.Add(Key(Rect(B, 1, 1, 0, 1), 0));
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
            d.parts.Add(Screws(Part(PartType.Hood, 0, O, 4, 0), R, B));
            d.parts.Add(Screws(Part(PartType.Hood, 1, C, 3, 0), G, Y));
            d.parts.Add(Part(PartType.ClockDoor, 0, Y, 4, 0));
            d.parts.Add(Part(PartType.Dial, 0, G, 4, 1));
            d.parts.Add(Part(PartType.Pendulum, 0, P, 4, 1));
            d.parts.Add(Part(PartType.Weight, 0, R, 3, 2));
            d.parts.Add(Part(PartType.Weight, 1, B, 3, 2));
            d.parts.Add(Part(PartType.Chime, 0, C, 3, 2));
            d.parts.Add(Part(PartType.Chime, 1, O, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, Y, 2, 3));
            d.parts.Add(Part(PartType.Tnt, 1, P, 1, 3));
            ResolveHidden(d, false);
            d.solution = new int[] { 67371013, 134676488, 134611459, 17302019, 83886351, 151060496, 151257093, 262156, 218236935, 218302477, 83887886, 67110410, 218105866, 201393666, 67108876, 201328140, 83887628, 134479876, 134480649, 17302025, 201589508, 17301512, 17302795, 17302532, 83887618, 134677766, 67437830, 151257094 };
            d.showRotateHint = true;
            return d;
        }
    }
}
