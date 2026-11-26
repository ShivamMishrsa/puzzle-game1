using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>TNT Discovery: one red shot on the TNT box blows the casing apart.</summary>
    public sealed class Level04 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(4, "TNT Discovery", 5, 5, 8, 5, LevelTier.Normal, "The sparking TNT box is red - a single red shot sets it off!");
            d.placements.Add(Rect(B, 2, 2, 0, 3));
            d.placements.Add(Rect(Y, 1, 2, 4, 3));
            d.placements.Add(Rect(R, 1, 1, 2, 1));
            d.placements.Add(Rect(Y, 1, 1, 2, 0));
            d.gates.Add(Gate(Up, 0, 2, B));
            d.gates.Add(Gate(Up, 4, 1, Y));
            d.gates.Add(Gate(Down, 2, 1, R));
            d.gates.Add(Gate(Left, 0, 1, Y));
            d.parts.Add(Part(PartType.Tnt, 0, R, 1, 0));
            d.parts.Add(Part(PartType.Casing, 0, B, 5, 0));
            d.parts.Add(Part(PartType.Bell, 0, Y, 2, 0));
            ResolveHidden(d, false);
            return d;
        }
    }
}
