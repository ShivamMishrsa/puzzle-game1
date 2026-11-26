using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>The Combo: two reds out back-to-back merge into a laser Mega-Turret.</summary>
    public sealed class Level02 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(2, "The Combo", 5, 5, 7, 5, LevelTier.Normal, "Send both red blocks out quickly - the turrets merge into a laser!");
            d.placements.Add(Rect(R, 1, 2, 1, 0));
            d.placements.Add(Rect(R, 1, 2, 1, 2));
            d.placements.Add(Rect(B, 2, 1, 3, 3));
            d.placements.Add(Rect(Y, 1, 1, 3, 0));
            d.gates.Add(Gate(Down, 1, 1, R));
            d.gates.Add(Gate(Right, 3, 1, B));
            d.gates.Add(Gate(Down, 3, 1, Y));
            d.parts.Add(Part(PartType.Casing, 0, R, 4, 0));
            d.parts.Add(Part(PartType.Bell, 0, Y, 1, 0));
            d.parts.Add(Part(PartType.Hand, 0, B, 2, 0));
            ResolveHidden(d, false);
            return d;
        }
    }
}
