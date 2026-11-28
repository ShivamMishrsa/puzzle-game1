using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>CHAPTER 13 BOSS: the full four-layer Cathedral Pipe Organ. EXTREME. Two track slots. Rotate it for the rear bolts, chain polarity coils, a portal, a key-lock, a ratchet and a repulsor with zero wasted moves - without hints, almost nobody gets through.</summary>
    public sealed class Level130 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(130, "Organ Finale", 11, 11, 34, 2, LevelTier.ChapterBoss, "Two slots, rear bolts, coils, portal, key, ratchet - the hint is your friend!");
            d.reward = "Chapter 13 Complete: Cathedral Pipe Organ Dismantled! The Organ trophy joins your workshop shelf - 130 levels cracked.";
            d.polarities.Add(Polarity(5, 5, true));
            d.polarities.Add(Polarity(2, 4, false));
            d.polarities.Add(Polarity(8, 8, true));
            d.repulsors.Add(Repulsor(7, 6));
            d.portals.Add(Pad(0, 1, 9, 9, 1));
            d.placements.Add(Scrap(1, 2, 3, 2));
            d.placements.Add(Scrap(1, 1, 8, 5));
            d.placements.Add(Rect(G, 2, 2, 0, 3));
            d.placements.Add(Rect(C, 2, 2, 6, 8));
            d.placements.Add(Rect(Y, 2, 2, 8, 1));
            d.placements.Add(Rect(O, 2, 2, 1, 5));
            d.placements.Add(Rect(O, 2, 2, 0, 7));
            d.placements.Add(Rect(P, 2, 2, 4, 2));
            d.placements.Add(Rect(R, 2, 2, 1, 1));
            d.placements.Add(Rect(B, 2, 2, 3, 8));
            d.placements.Add(Rect(G, 2, 2, 0, 9));
            d.placements.Add(Rect(R, 2, 1, 9, 8));
            d.placements.Add(Rect(C, 1, 2, 7, 1));
            d.placements.Add(Rect(C, 1, 1, 7, 0));
            d.placements.Add(Rect(O, 1, 1, 4, 6));
            d.placements.Add(Rect(B, 1, 1, 2, 3));
            d.placements.Add(Rect(R, 1, 1, 7, 10));
            d.placements.Add(Rect(Y, 1, 1, 3, 1));
            d.placements.Add(Rect(P, 1, 1, 5, 9));
            d.placements.Add(Key(Rect(B, 1, 1, 7, 3), 0));
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
            d.parts.Add(Screws(Part(PartType.Facade, 0, O, 4, 0), R, B));
            d.parts.Add(Part(PartType.Facade, 1, C, 3, 0));
            d.parts.Add(Part(PartType.Pipes, 0, G, 4, 1));
            d.parts.Add(Part(PartType.Pipes, 1, P, 3, 1));
            d.parts.Add(Part(PartType.Keyboard, 0, Y, 3, 1));
            d.parts.Add(Part(PartType.Bellows, 0, R, 4, 2));
            d.parts.Add(Part(PartType.Stops, 0, B, 3, 2));
            d.parts.Add(Part(PartType.Stops, 1, O, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, C, 2, 3));
            d.parts.Add(Part(PartType.Tnt, 1, G, 2, 3));
            ResolveHidden(d, false);
            d.solution = new int[] { 67110916, 134283272, 83887885, 201654789, 83887884, 67437582, 67174927, 201984272, 67176211, 83886088, 67239938, 67371014, 151060495, 151257094, 151126018, 83887111, 67239946, 134742281, 83886865, 134283539, 134218244, 83886596, 151257093, 151322638, 83887378, 151060499, 151126026, 67109129, 150994953, 591363, 134742027, 17432587, 83887619, 83886352 };
            d.showRotateHint = true;
            return d;
        }
    }
}
