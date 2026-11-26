using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Exposed Copper Gears: 7x7 tray; the outer shell is already broken off, exposing the reels, gears and twin TNT (15 HP).</summary>
    public sealed class Level18 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(18, "Exposed Copper Gears", 7, 7, 20, 5, LevelTier.Normal, "The shell is gone! Reels, gears and TNT are all exposed.");
            d.placements.Add(Scrap(1, 1, 3, 3));
            d.placements.Add(Scrap(1, 1, 5, 1));
            d.placements.Add(Rect(B, 2, 2, 0, 2));
            d.placements.Add(Ell(R, 0, 0, 4));
            d.placements.Add(Ell(Y, 2, 1, 4));
            d.placements.Add(Rect(R, 1, 2, 2, 1));
            d.placements.Add(Rect(B, 1, 2, 5, 2));
            d.placements.Add(Ice(Rect(B, 2, 1, 3, 2), 1));
            d.placements.Add(Rect(Y, 2, 1, 2, 6));
            d.placements.Add(Rect(B, 1, 1, 2, 3));
            d.gates.Add(Gate(Down, 0, 2, B));
            d.gates.Add(Gate(Left, 4, 2, B));
            d.gates.Add(Gate(Right, 2, 2, B));
            d.gates.Add(Gate(Up, 4, 2, R));
            d.gates.Add(Gate(Left, 1, 2, R));
            d.gates.Add(Gate(Down, 4, 2, Y));
            d.gates.Add(Gate(Up, 0, 2, Y));
            d.parts.Add(Part(PartType.Cassette, 0, B, 6, 1));
            d.parts.Add(Part(PartType.Gear, 0, R, 3, 2));
            d.parts.Add(Part(PartType.Gear, 1, Y, 3, 2));
            d.parts.Add(Part(PartType.Gear, 2, B, 1, 2));
            d.parts.Add(Part(PartType.Tnt, 0, R, 1, 2));
            d.parts.Add(Part(PartType.Tnt, 1, Y, 1, 2));
            ResolveHidden(d, true);
            return d;
        }
    }
}
