using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>The Escapement Trap (mini-boss): a padlocked gate, two screws inside the watch, and a portal that is the only way out of the pocket holding the brass key.</summary>
    public sealed class Level35 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(35, "Escapement Trap", 7, 7, 15, 4, LevelTier.MiniBoss, "The key is walled in: warp it out through the portal. Stuck? Use the hint!");
            d.placements.Add(Scrap(1, 2, 2, 5));
            d.placements.Add(Scrap(2, 1, 0, 4));
            d.placements.Add(Key(Rect(B, 1, 1, 0, 6), 0));
            d.placements.Add(Rect(R, 2, 2, 3, 4));
            d.placements.Add(Rect(Y, 1, 2, 0, 1));
            d.placements.Add(Rect(B, 2, 1, 5, 2));
            d.placements.Add(Rect(G, 2, 1, 1, 0));
            d.placements.Add(Rect(Y, 2, 1, 2, 1));
            d.placements.Add(Rect(R, 1, 1, 1, 1));
            d.placements.Add(Rect(G, 1, 1, 3, 6));
            d.placements.Add(Rect(R, 1, 1, 6, 1));
            d.placements.Add(Rect(B, 1, 1, 5, 1));
            d.portals.Add(Pad(0, 1, 6, 4, 2));
            d.gates.Add(Gate(Right, 1, 2, B));
            d.gates.Add(Gate(Down, 4, 2, B));
            d.gates.Add(Padlock(Gate(Down, 0, 2, R), 0));
            d.gates.Add(Padlock(Gate(Right, 4, 2, R), 0));
            d.gates.Add(Gate(Up, 4, 2, Y));
            d.gates.Add(Gate(Left, 0, 2, Y));
            d.gates.Add(Gate(Right, 0, 1, G));
            d.gates.Add(Gate(Up, 3, 1, G));
            d.parts.Add(Part(PartType.Casing, 0, Y, 3, 0));
            d.parts.Add(Part(PartType.Casing, 1, R, 3, 0));
            d.parts.Add(Part(PartType.Dial, 0, B, 3, 1));
            d.parts.Add(Screw(Part(PartType.BalanceWheel, 0, R, 3, 2), Y));
            d.parts.Add(Screw(Part(PartType.Gear, 0, G, 2, 2), B));
            ResolveHidden(d, false);
            return d;
        }
    }
}
