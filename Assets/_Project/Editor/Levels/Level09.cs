using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>The Wall: a horizontal scrap strip splits the tray; the TNT chain is the shortcut.</summary>
    public sealed class Level09 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(9, "The Wall", 6, 6, 14, 3, LevelTier.Normal, "Two TNT boxes chain together - find the order that lights the fuse.");
            d.placements.Add(Scrap(4, 1, 1, 3));
            d.placements.Add(Rect(R, 2, 1, 2, 0));
            d.placements.Add(Rect(Y, 2, 1, 4, 0));
            d.placements.Add(Rect(B, 1, 2, 1, 4));
            d.placements.Add(Rect(R, 1, 1, 4, 2));
            d.placements.Add(Rect(B, 1, 1, 4, 5));
            d.gates.Add(Gate(Left, 1, 1, R));
            d.gates.Add(Gate(Down, 2, 2, Y));
            d.gates.Add(Gate(Right, 4, 2, B));
            d.gates.Add(Gate(Up, 2, 1, B));
            d.gates.Add(Gate(Up, 3, 2, R));
            d.parts.Add(Part(PartType.Tnt, 0, R, 1, 0));
            d.parts.Add(Part(PartType.Tnt, 1, Y, 1, 0));
            d.parts.Add(Part(PartType.Casing, 0, R, 5, 0));
            d.parts.Add(Part(PartType.Bell, 0, Y, 3, 0));
            d.parts.Add(Part(PartType.Bell, 1, B, 3, 0));
            ResolveHidden(d, false);
            return d;
        }
    }
}
