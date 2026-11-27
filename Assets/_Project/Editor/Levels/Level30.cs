using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Chapter 3 Boss: the full three-stage Vintage Rotary Phone, 30 HP. Rotate it to find the rear screw, unlock the gates in order, detonate the TNT.</summary>
    public sealed class Level30 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(30, "Rotary Phone Finale", 7, 7, 20, 4, LevelTier.ChapterBoss, "Rotate the phone: a green screw hides on the back. Unlock the gates in order, then blow the TNT!");
            d.reward = "Chapter 3 Complete: Vintage Rotary Phone Dismantled! Rotary Phone trophy unlocked.";
            d.showRotateHint = true;
            d.placements.Add(Scrap(1, 1, 3, 3));
            d.placements.Add(Scrap(1, 1, 1, 4));
            d.placements.Add(Rect(R, 2, 2, 4, 1));
            d.placements.Add(Rect(R, 2, 2, 2, 1));
            d.placements.Add(Rect(B, 2, 2, 5, 3));
            d.placements.Add(Rect(G, 2, 2, 0, 2));
            d.placements.Add(Rect(G, 2, 1, 3, 5));
            d.placements.Add(Rect(Y, 1, 2, 1, 0));
            d.placements.Add(Rect(Y, 2, 1, 5, 5));
            d.placements.Add(Key(Rect(Y, 1, 1, 0, 6), 0));
            d.placements.Add(Key(Rect(B, 1, 1, 5, 6), 1));
            d.placements.Add(Rect(R, 1, 1, 1, 5));
            d.gates.Add(Gate(Down, 0, 2, Y));
            d.gates.Add(Gate(Right, 4, 2, Y));
            d.gates.Add(Padlock(Gate(Left, 3, 2, B), 0));
            d.gates.Add(Gate(Up, 4, 2, B));
            d.gates.Add(Gate(Down, 4, 2, R));
            d.gates.Add(Gate(Left, 0, 2, R));
            d.gates.Add(Gate(Up, 0, 2, R));
            d.gates.Add(Padlock(Gate(Right, 3, 2, G), 1));
            d.gates.Add(Padlock(Gate(Up, 2, 2, G), 1));
            d.parts.Add(Screw(Part(PartType.Handset, 0, R, 4, 0), B));
            d.parts.Add(Screw(Part(PartType.Handset, 1, G, 4, 0), R));
            d.parts.Add(Part(PartType.Cradle, 0, Y, 2, 0));
            d.parts.Add(Screw(Part(PartType.Casing, 0, B, 3, 0), G));
            d.parts.Add(Part(PartType.Dial, 0, R, 3, 1));
            d.parts.Add(Part(PartType.Ringer, 0, Y, 4, 2));
            d.parts.Add(Part(PartType.Ringer, 1, B, 4, 2));
            d.parts.Add(Part(PartType.Coil, 0, G, 5, 2));
            d.parts.Add(Part(PartType.Tnt, 0, R, 1, 2));
            ResolveHidden(d, false);
            HideBehind(d, PartType.Tnt, 0, PartType.Casing);
            return d;
        }
    }
}
