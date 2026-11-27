using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Chapter 4 Boss: the full three-stage Antique Pocket Watch, 34 HP. Rotate it for the rear screws, warp the key out of its pocket, launch through the piercing gate, detonate the mainspring.</summary>
    public sealed class Level40 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(40, "Pocket Watch Finale", 7, 7, 24, 4, LevelTier.ChapterBoss, "Rotate the watch for the rear screws. Warp the key out, unlock the piercing gate, blow the mainspring!");
            d.reward = "Chapter 4 Complete: Antique Pocket Watch Dismantled! Pocket Watch trophy unlocked.";
            d.showRotateHint = true;
            d.placements.Add(Scrap(1, 2, 2, 5));
            d.placements.Add(Scrap(2, 1, 0, 4));
            d.placements.Add(Scrap(1, 1, 4, 3));
            d.placements.Add(Key(Rect(B, 1, 1, 0, 6), 0));
            d.placements.Add(Rect(Y, 2, 2, 2, 1));
            d.placements.Add(Rect(R, 2, 2, 3, 5));
            d.placements.Add(Rect(B, 2, 2, 4, 0));
            d.placements.Add(Rect(G, 2, 1, 0, 1));
            d.placements.Add(Rect(B, 1, 2, 6, 1));
            d.placements.Add(Rect(Y, 2, 1, 5, 4));
            d.placements.Add(Rect(G, 1, 1, 1, 6));
            d.placements.Add(Rect(Y, 1, 1, 5, 6));
            d.placements.Add(Rect(R, 1, 1, 1, 0));
            d.portals.Add(Pad(1, 1, 6, 4, 2));
            d.gates.Add(Gate(Right, 1, 2, B));
            d.gates.Add(Gate(Down, 4, 2, B));
            d.gates.Add(Padlock(Piercing(Gate(Up, 4, 2, Y)), 0));
            d.gates.Add(Padlock(Piercing(Gate(Left, 3, 2, Y)), 0));
            d.gates.Add(Gate(Down, 0, 2, R));
            d.gates.Add(Gate(Right, 4, 2, R));
            d.gates.Add(Gate(Left, 0, 2, G));
            d.gates.Add(Gate(Up, 0, 2, G));
            d.parts.Add(Screw(Part(PartType.Casing, 0, Y, 4, 0), B));
            d.parts.Add(Screw(Part(PartType.Casing, 1, R, 4, 0), G));
            d.parts.Add(Part(PartType.Button, 0, B, 2, 0));
            d.parts.Add(Part(PartType.Glass, 0, B, 5, 1));
            d.parts.Add(Part(PartType.Dial, 0, Y, 3, 1));
            d.parts.Add(Part(PartType.Hand, 0, G, 3, 1));
            d.parts.Add(Part(PartType.Tnt, 0, R, 1, 2));
            d.parts.Add(Part(PartType.BalanceWheel, 0, R, 6, 2));
            d.parts.Add(Part(PartType.Gear, 0, Y, 2, 2));
            d.parts.Add(Part(PartType.Ruby, 0, R, 2, 2));
            d.parts.Add(Part(PartType.Ruby, 1, G, 2, 2));
            ResolveHidden(d, false);
            return d;
        }
    }
}
