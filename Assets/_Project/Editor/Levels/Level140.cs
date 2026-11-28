using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>CHAPTER 14 BOSS: the full four-layer Steam Locomotive. EXTREME+. A 12x12 tray. Rotate it for the rear bolts, chain polarity coils, a portal, a key-lock, a ratchet and two repulsors with zero wasted moves - the hardest level yet.</summary>
    public sealed class Level140 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(140, "Locomotive Finale", 12, 12, 48, 3, LevelTier.ChapterBoss, "12x12! Rear bolts, coils, portal, key, ratchet - the hint is your friend!");
            d.reward = "Chapter 14 Complete: Steam Locomotive Dismantled! The Locomotive trophy joins your workshop shelf - 140 levels cracked.";
            d.turnstiles.Add(Turnstile(6, 4, Right));
            d.polarities.Add(Polarity(5, 5, true));
            d.polarities.Add(Polarity(2, 4, false));
            d.polarities.Add(Polarity(9, 9, true));
            d.repulsors.Add(Repulsor(8, 6));
            d.repulsors.Add(Repulsor(3, 8));
            d.portals.Add(Pad(0, 1, 10, 10, 1));
            d.placements.Add(Scrap(1, 2, 3, 2));
            d.placements.Add(Scrap(1, 1, 9, 5));
            d.placements.Add(Rect(P, 2, 2, 1, 2));
            d.placements.Add(Rect(C, 2, 2, 5, 7));
            d.placements.Add(Rect(O, 2, 2, 6, 9));
            d.placements.Add(Rect(C, 2, 2, 8, 10));
            d.placements.Add(Rect(O, 2, 2, 9, 7));
            d.placements.Add(Rect(Y, 2, 2, 8, 3));
            d.placements.Add(Rect(R, 2, 2, 1, 10));
            d.placements.Add(Rect(G, 2, 2, 2, 7));
            d.placements.Add(Rect(B, 2, 2, 2, 5));
            d.placements.Add(Rect(Y, 1, 2, 4, 2));
            d.placements.Add(Rect(R, 2, 1, 0, 1));
            d.placements.Add(Rect(G, 2, 1, 8, 9));
            d.placements.Add(Rect(Y, 1, 1, 4, 11));
            d.placements.Add(Key(Rect(B, 1, 1, 2, 0), 0));
            d.placements.Add(Rect(B, 1, 1, 5, 4));
            d.placements.Add(Rect(P, 1, 1, 1, 7));
            d.placements.Add(Rect(G, 1, 1, 6, 5));
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
            d.parts.Add(Screws(Part(PartType.Boiler, 0, O, 3, 0), R, B));
            d.parts.Add(Part(PartType.Boiler, 1, C, 3, 0));
            d.parts.Add(Part(PartType.Smokestack, 0, Y, 3, 0));
            d.parts.Add(Part(PartType.Cab, 0, G, 3, 1));
            d.parts.Add(Part(PartType.Piston, 0, P, 3, 1));
            d.parts.Add(Part(PartType.Piston, 1, R, 3, 1));
            d.parts.Add(Part(PartType.Driver, 0, B, 3, 2));
            d.parts.Add(Part(PartType.Driver, 1, O, 3, 2));
            d.parts.Add(Part(PartType.Coal, 0, C, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, Y, 2, 3));
            d.parts.Add(Part(PartType.Tnt, 1, G, 2, 3));
            ResolveHidden(d, false);
            d.solution = new int[] { 201787139, 134808580, 134874885, 67110919, 67241731, 134350339, 134873096, 524809, 134677254, 134545674, 67371782, 67109899, 134218507, 83886859, 201918733, 67308045, 134939406, 17498894, 150994959, 134218247, 67175696, 201786385, 134546706, 67372306, 83887619, 67241733, 134350341, 83887621, 17432584, 83886092, 393482, 151257094, 262402, 201589506, 218302989, 218303250, 67371274, 67108874, 67305737, 150994954, 134808068, 67502596, 151257092, 17499153, 17433602, 83886599, 151126025, 151060496 };
            d.showRotateHint = true;
            return d;
        }
    }
}
