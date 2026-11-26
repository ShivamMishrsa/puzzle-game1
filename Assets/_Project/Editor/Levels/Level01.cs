using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>The Hook: a red bar slides straight down into its gate and launches the first turret. Unlimited track.</summary>
    public sealed class Level01 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(1, "The Hook", 5, 5, 6, 0, LevelTier.Normal, "Slide the red bar down into the red gate!");
            d.placements.Add(Rect(R, 1, 3, 2, 2));
            d.placements.Add(Rect(B, 2, 1, 0, 0));
            d.placements.Add(Rect(B, 1, 1, 4, 3));
            d.gates.Add(Gate(Down, 2, 1, R));
            d.gates.Add(Gate(Left, 0, 1, B));
            d.gates.Add(Gate(Right, 3, 1, B));
            d.parts.Add(Part(PartType.Casing, 0, R, 3, 0));
            d.parts.Add(Part(PartType.Hand, 0, B, 2, 0));
            ResolveHidden(d, false);
            return d;
        }
    }
}
