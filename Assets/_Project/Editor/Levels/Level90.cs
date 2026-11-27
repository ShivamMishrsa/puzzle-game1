using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>CHAPTER 9 BOSS: the full three-stage Celestial Brass Orrery. Rotate it for the four hidden bolts, chain repulsors, a portal and a key-lock with zero wasted moves, pierce the 3-layer armour and detonate the solar core.</summary>
    public sealed class Level90 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(90, "Orrery Finale", 9, 9, 32, 4, LevelTier.ChapterBoss, "Rotate for the rear bolts. Coils, a portal, a key - exact moves only!");
            d.reward = "Chapter 9 Complete: Celestial Brass Orrery Dismantled! Orrery trophy placed on your workshop shelf.";
            d.repulsors.Add(Repulsor(4, 4));
            d.repulsors.Add(Repulsor(2, 3));
            d.repulsors.Add(Repulsor(6, 6));
            d.portals.Add(Pad(0, 1, 7, 7, 1));
            d.placements.Add(Scrap(1, 2, 3, 2));
            d.placements.Add(Scrap(1, 1, 6, 5));
            d.placements.Add(Rect(P, 2, 2, 1, 0));
            d.placements.Add(Rect(C, 2, 2, 2, 7));
            d.placements.Add(Rect(O, 2, 2, 2, 4));
            d.placements.Add(Rect(C, 2, 2, 7, 2));
            d.placements.Add(Rect(O, 2, 2, 3, 0));
            d.placements.Add(Rect(R, 2, 2, 1, 2));
            d.placements.Add(Rect(Y, 2, 2, 6, 6));
            d.placements.Add(Rect(G, 2, 2, 0, 5));
            d.placements.Add(Rect(B, 2, 1, 5, 0));
            d.placements.Add(Rect(P, 1, 2, 5, 7));
            d.placements.Add(Rect(Y, 1, 2, 7, 0));
            d.placements.Add(Rect(G, 2, 1, 5, 4));
            d.placements.Add(Key(Rect(B, 1, 1, 8, 1), 0));
            d.placements.Add(Rect(B, 1, 1, 8, 6));
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
            d.parts.Add(Screw(Part(PartType.Ring, 0, O, 4, 0), R));
            d.parts.Add(Screw(Part(PartType.Ring, 1, C, 4, 0), B));
            d.parts.Add(Screw(Part(PartType.Ring, 2, Y, 4, 0), G));
            d.parts.Add(Screw(Part(PartType.Ring, 3, G, 4, 0), P));
            d.parts.Add(Part(PartType.Planet, 0, C, 4, 1));
            d.parts.Add(Part(PartType.Planet, 1, O, 4, 1));
            d.parts.Add(Part(PartType.Planet, 2, B, 3, 1));
            d.parts.Add(Part(PartType.Planet, 3, R, 3, 1));
            d.parts.Add(Part(PartType.Cage, 0, P, 5, 2));
            d.parts.Add(Part(PartType.Tnt, 0, Y, 2, 3));
            d.parts.Add(Part(PartType.Tnt, 1, G, 1, 3));
            ResolveHidden(d, false);
            d.showRotateHint = true;
            return d;
        }
    }
}
