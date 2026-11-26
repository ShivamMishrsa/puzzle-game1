using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Chapter 2 Boss: the full three-stage Retro Cassette Player, 24 HP. Chain a Mega-Turret into the TNT capacitor.</summary>
    public sealed class Level20 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(20, "Cassette Player Finale", 7, 7, 22, 4, LevelTier.ChapterBoss, "Armor, cassette, then the core. Chain a Mega-Turret laser into the TNT capacitor!");
            d.reward = "Chapter 2 Complete: Cassette Player Dismantled! Cassette Player trophy unlocked.";
            d.placements.Add(Scrap(1, 1, 2, 2));
            d.placements.Add(Scrap(1, 1, 4, 4));
            d.placements.Add(Ice(Rect(R, 1, 2, 5, 3), 1));
            d.placements.Add(Rect(Y, 1, 1, 4, 3));
            d.placements.Add(Rect(B, 2, 2, 1, 4));
            d.placements.Add(Rect(R, 2, 2, 0, 2));
            d.placements.Add(Ell(Y, 2, 3, 1));
            d.placements.Add(Ell(B, 0, 5, 1));
            d.placements.Add(Peeled(Rect(Y, 2, 1, 0, 6), B));
            d.placements.Add(Rect(B, 1, 2, 6, 2));
            d.gates.Add(Gate(Down, 0, 2, R));
            d.gates.Add(Shutter(Right, 3, 2, R, true));
            d.gates.Add(Gate(Up, 4, 2, R));
            d.gates.Add(Gate(Left, 4, 2, B));
            d.gates.Add(Gate(Down, 4, 2, B));
            d.gates.Add(Gate(Up, 0, 2, B));
            d.gates.Add(Gate(Right, 0, 2, Y));
            d.gates.Add(Gate(Left, 1, 3, Y));
            d.gates.Add(Gate(Up, 2, 2, Y));
            d.parts.Add(Part(PartType.Door, 0, R, 4, 0));
            d.parts.Add(Part(PartType.Casing, 0, B, 3, 0));
            d.parts.Add(Part(PartType.Casing, 1, Y, 3, 0));
            d.parts.Add(Part(PartType.Button, 0, B, 1, 0));
            d.parts.Add(Part(PartType.Button, 1, Y, 1, 0));
            d.parts.Add(Part(PartType.Cassette, 0, B, 5, 1));
            d.parts.Add(Part(PartType.Gear, 0, B, 2, 2));
            d.parts.Add(Part(PartType.Gear, 1, Y, 2, 2));
            d.parts.Add(Part(PartType.Pcb, 0, Y, 2, 2));
            d.parts.Add(Part(PartType.Tnt, 0, R, 1, 2));
            ResolveHidden(d, false);
            return d;
        }
    }
}
