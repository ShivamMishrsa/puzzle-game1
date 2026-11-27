using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>CHAPTER 5 BOSS: the full three-stage Vintage Espresso Machine, 38 HP. Rotate it for the rear screw, snap two welded pairs in order, time the steam gate and detonate the dynamite bundle in the boiler.</summary>
    public sealed class Level50 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(50, "Espresso Finale", 7, 7, 23, 4, LevelTier.ChapterBoss, "Rotate the machine for the rear bolt. Snap both welds, time the steam, blow the dynamite!");
            d.showRotateHint = true;
            d.reward = "Chapter 5 Complete: Vintage Espresso Machine Dismantled! Espresso Machine trophy placed on your workshop shelf.";
            d.placements.Add(Scrap(1, 2, 3, 3));
            d.placements.Add(Scrap(2, 1, 5, 2));
            d.placements.Add(Rect(Y, 2, 2, 4, 4));
            d.placements.Add(Rect(B, 2, 2, 1, 4));
            d.placements.Add(Rect(R, 2, 2, 0, 2));
            d.placements.Add(Rect(G, 2, 2, 2, 0));
            d.placements.Add(Rect(R, 1, 2, 6, 0));
            d.placements.Add(Rect(B, 1, 2, 0, 4));
            d.placements.Add(Rect(G, 1, 2, 5, 0));
            d.placements.Add(Link(Rect(B, 1, 1, 2, 6), 1));
            d.placements.Add(Link(Rect(R, 1, 1, 4, 6), 1));
            d.placements.Add(Link(Rect(Y, 1, 1, 2, 3), 2));
            d.placements.Add(Key(Rect(Y, 1, 1, 5, 6), 0));
            d.placements.Add(Link(Rect(G, 1, 1, 4, 2), 2));
            d.gates.Add(Padlock(Steam(Gate(Down, 0, 2, R), 2), 0));
            d.gates.Add(Gate(Right, 4, 2, R));
            d.gates.Add(Gate(Left, 3, 2, B));
            d.gates.Add(Gate(Up, 0, 2, B));
            d.gates.Add(Gate(Up, 4, 2, Y));
            d.gates.Add(Gate(Right, 0, 2, Y));
            d.gates.Add(Gate(Left, 0, 2, G));
            d.gates.Add(Gate(Down, 4, 2, G));
            d.parts.Add(Part(PartType.Boiler, 0, R, 3, 0));
            d.parts.Add(Part(PartType.Boiler, 1, Y, 2, 0));
            d.parts.Add(Part(PartType.Gauge, 0, B, 2, 0));
            d.parts.Add(Screw(Part(PartType.Lever, 0, G, 3, 0), B));
            d.parts.Add(Part(PartType.GroupHead, 0, B, 4, 1));
            d.parts.Add(Part(PartType.SightTube, 0, Y, 2, 1));
            d.parts.Add(Part(PartType.DripGrid, 0, R, 2, 1));
            d.parts.Add(Part(PartType.Coil, 0, R, 4, 2));
            d.parts.Add(Part(PartType.Valve, 0, Y, 3, 2));
            d.parts.Add(Part(PartType.Valve, 1, G, 4, 2));
            d.parts.Add(Part(PartType.Valve, 2, B, 3, 2));
            d.parts.Add(Screw(Part(PartType.Tnt, 0, R, 3, 2), G));
            d.parts.Add(Part(PartType.Tnt, 1, Y, 3, 2));
            ResolveHidden(d, false);
            return d;
        }
    }
}
