using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>MINI-BOSS Celestial Alignment: first 9x9 tray. Four brass screws, two padlocked gates, repulsor coils and a portal pair guard the 3-layer armour around the dynamite. Zero spare moves.</summary>
    public sealed class Level85 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(85, "Celestial Alignment", 9, 9, 29, 4, LevelTier.MiniBoss, "Four bolts, two padlocks, coils and a portal. Not one wasted move!");
            d.repulsors.Add(Repulsor(4, 4));
            d.repulsors.Add(Repulsor(2, 6));
            d.portals.Add(Pad(0, 1, 7, 7, 1));
            d.placements.Add(Scrap(1, 2, 3, 2));
            d.placements.Add(Scrap(2, 1, 5, 6));
            d.placements.Add(Scrap(1, 1, 6, 3));
            d.placements.Add(Rect(C, 2, 2, 4, 1));
            d.placements.Add(Rect(Y, 2, 2, 3, 7));
            d.placements.Add(Rect(O, 2, 2, 1, 5));
            d.placements.Add(Rect(P, 2, 2, 3, 5));
            d.placements.Add(Rect(B, 2, 2, 6, 0));
            d.placements.Add(Rect(R, 2, 2, 1, 3));
            d.placements.Add(Rect(G, 2, 2, 1, 1));
            d.placements.Add(Rect(O, 1, 2, 0, 5));
            d.placements.Add(Rect(C, 2, 1, 7, 2));
            d.placements.Add(Rect(Y, 2, 1, 3, 0));
            d.placements.Add(Rect(C, 1, 1, 6, 4));
            d.placements.Add(Rect(O, 1, 1, 0, 4));
            d.placements.Add(Rect(R, 1, 1, 8, 4));
            d.placements.Add(Key(Rect(G, 1, 1, 8, 3), 1));
            d.placements.Add(Rect(P, 1, 1, 5, 0));
            d.placements.Add(Key(Rect(B, 1, 1, 0, 1), 0));
            d.gates.Add(Gate(Up, 0, 2, R));
            d.gates.Add(Gate(Down, 0, 2, R));
            d.gates.Add(Gate(Left, 0, 2, B));
            d.gates.Add(Gate(Right, 0, 2, B));
            d.gates.Add(Gate(Down, 2, 2, Y));
            d.gates.Add(Padlock(Gate(Up, 2, 2, Y), 1));
            d.gates.Add(Gate(Right, 2, 2, G));
            d.gates.Add(Gate(Left, 2, 2, G));
            d.gates.Add(Padlock(Gate(Up, 4, 2, P), 0));
            d.gates.Add(Gate(Down, 4, 2, P));
            d.gates.Add(Gate(Left, 4, 2, O));
            d.gates.Add(Gate(Down, 6, 2, C));
            d.parts.Add(Screw(Part(PartType.Ring, 0, O, 3, 0), R));
            d.parts.Add(Screw(Part(PartType.Ring, 1, C, 3, 0), B));
            d.parts.Add(Screw(Part(PartType.Ring, 2, Y, 3, 0), G));
            d.parts.Add(Screw(Part(PartType.Ring, 3, G, 3, 0), P));
            d.parts.Add(Part(PartType.Planet, 0, C, 3, 1));
            d.parts.Add(Part(PartType.Planet, 1, O, 3, 1));
            d.parts.Add(Part(PartType.Planet, 2, B, 3, 1));
            d.parts.Add(Part(PartType.Planet, 3, R, 3, 1));
            d.parts.Add(Part(PartType.Cage, 0, P, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, Y, 2, 3));
            ResolveHidden(d, false);
            d.showRotateHint = true;
            return d;
        }
    }
}
