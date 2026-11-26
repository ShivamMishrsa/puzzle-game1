using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>The Grand Alarm Clock (chapter boss): dual bells, gear train behind the casing, three chained TNT boxes. 20 HP.</summary>
    public sealed class Level10 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(10, "The Grand Alarm Clock", 6, 6, 24, 2, LevelTier.ChapterBoss, "Only 2 track slots! Chain the TNT, then dismantle the gears.");
            d.reward = "Diorama Unlock: Alarm Clock Disassembled + Chapter Chest";
            d.placements.Add(Scrap(1, 1, 2, 2));
            d.placements.Add(Scrap(1, 1, 3, 3));
            d.placements.Add(Scrap(1, 1, 0, 5));
            d.placements.Add(Scrap(1, 1, 5, 0));
            d.placements.Add(Rect(R, 2, 2, 1, 4));
            d.placements.Add(Ell(Y, 2, 0, 0));
            d.placements.Add(Ell(B, 0, 2, 0));
            d.placements.Add(Rect(Y, 2, 1, 4, 5));
            d.placements.Add(Rect(R, 1, 2, 3, 4));
            d.placements.Add(Rect(B, 1, 2, 1, 2));
            d.gates.Add(Gate(Down, 0, 2, R));
            d.gates.Add(Gate(Up, 4, 1, R));
            d.gates.Add(Gate(Left, 3, 2, B));
            d.gates.Add(Gate(Up, 2, 1, B));
            d.gates.Add(Gate(Right, 1, 2, Y));
            d.gates.Add(Gate(Up, 0, 2, Y));
            d.parts.Add(Part(PartType.Bell, 0, B, 3, 0));
            d.parts.Add(Part(PartType.Bell, 1, Y, 3, 0));
            d.parts.Add(Part(PartType.Tnt, 0, Y, 1, 0));
            d.parts.Add(Part(PartType.Tnt, 1, B, 1, 0));
            d.parts.Add(Part(PartType.Tnt, 2, R, 1, 0));
            d.parts.Add(Part(PartType.Casing, 0, R, 5, 0));
            d.parts.Add(Part(PartType.Gear, 0, Y, 2, 1));
            d.parts.Add(Part(PartType.Gear, 1, B, 2, 1));
            d.parts.Add(Part(PartType.Gear, 2, R, 2, 1));
            ResolveHidden(d, false);
            return d;
        }
    }
}
