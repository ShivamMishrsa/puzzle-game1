using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>MINI-BOSS Pressurized Boiler Trap: two welded pairs, a padlocked gate and two brass screw-bolts (lever and TNT bundle). Only 4 track slots - plan the order!</summary>
    public sealed class Level45 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(45, "Boiler Trap", 7, 7, 13, 4, LevelTier.MiniBoss, "Two welds, a padlock and two brass bolts. Stuck? Use the hint or the pliers!");
            d.placements.Add(Scrap(1, 2, 3, 3));
            d.placements.Add(Scrap(2, 1, 5, 1));
            d.placements.Add(Rect(R, 2, 2, 0, 2));
            d.placements.Add(Rect(B, 1, 2, 1, 5));
            d.placements.Add(Rect(G, 2, 1, 4, 0));
            d.placements.Add(Rect(Y, 1, 2, 6, 2));
            d.placements.Add(Link(Rect(R, 1, 1, 4, 1), 1));
            d.placements.Add(Link(Rect(Y, 1, 1, 2, 4), 2));
            d.placements.Add(Link(Rect(B, 1, 1, 4, 2), 1));
            d.placements.Add(Key(Rect(B, 1, 1, 0, 4), 0));
            d.placements.Add(Link(Rect(G, 1, 1, 2, 3), 2));
            d.gates.Add(Padlock(Gate(Down, 0, 2, R), 0));
            d.gates.Add(Gate(Right, 4, 2, R));
            d.gates.Add(Gate(Left, 3, 2, B));
            d.gates.Add(Gate(Up, 0, 2, B));
            d.gates.Add(Gate(Up, 4, 2, Y));
            d.gates.Add(Gate(Right, 0, 2, Y));
            d.gates.Add(Gate(Left, 0, 2, G));
            d.gates.Add(Gate(Down, 4, 2, G));
            d.parts.Add(Part(PartType.Boiler, 0, R, 2, 0));
            d.parts.Add(Part(PartType.Boiler, 1, Y, 1, 0));
            d.parts.Add(Part(PartType.Gauge, 0, B, 1, 0));
            d.parts.Add(Screw(Part(PartType.Lever, 0, G, 2, 0), B));
            d.parts.Add(Part(PartType.GroupHead, 0, B, 2, 1));
            d.parts.Add(Part(PartType.SightTube, 0, Y, 1, 1));
            d.parts.Add(Part(PartType.DripGrid, 0, R, 1, 1));
            d.parts.Add(Part(PartType.Valve, 0, G, 1, 2));
            d.parts.Add(Screw(Part(PartType.Tnt, 0, R, 2, 2), Y));
            ResolveHidden(d, false);
            d.showRotateHint = true;
            return d;
        }
    }
}
