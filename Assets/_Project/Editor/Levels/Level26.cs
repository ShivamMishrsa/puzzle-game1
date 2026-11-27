using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Rail Curtain (flow breather): one central key block leaves and three rail blocks pour out through the unlocked gates - a 4-block rush with a Mega-Turret laser.</summary>
    public sealed class Level26 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(26, "Rail Curtain", 7, 7, 10, 5, LevelTier.Normal, "Slide the central key down. The curtain of rail blocks follows!");
            d.placements.Add(Key(Rect(B, 1, 1, 3, 3), 0));
            d.placements.Add(Arrow(Rect(R, 2, 1, 1, 3), Horizontal));
            d.placements.Add(Arrow(Rect(R, 2, 1, 4, 3), Horizontal));
            d.placements.Add(Arrow(Rect(R, 1, 2, 3, 4), Vertical));
            d.gates.Add(Gate(Down, 3, 1, B));
            d.gates.Add(Padlock(Gate(Left, 3, 1, R), 0));
            d.gates.Add(Padlock(Gate(Right, 3, 1, R), 0));
            d.gates.Add(Padlock(Gate(Up, 3, 1, R), 0));
            d.parts.Add(Part(PartType.Handset, 0, R, 3, 0));
            d.parts.Add(Part(PartType.Handset, 1, R, 3, 0));
            d.parts.Add(Part(PartType.Cradle, 0, B, 1, 0));
            ResolveHidden(d, false);
            return d;
        }
    }
}
