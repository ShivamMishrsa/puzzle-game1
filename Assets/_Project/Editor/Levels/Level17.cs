using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Alternating Rhythms: shutters on opposite walls in opposite phase plus two peeled blocks.</summary>
    public sealed class Level17 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(17, "Alternating Rhythms", 6, 6, 18, 5, LevelTier.Normal, "The left and right shutters alternate - plan the order of your exits.");
            d.placements.Add(Rect(R, 2, 1, 3, 3));
            d.placements.Add(Peeled(Rect(B, 1, 2, 0, 2), Y));
            d.placements.Add(Peeled(Rect(Y, 2, 1, 1, 1), R));
            d.placements.Add(Rect(Y, 1, 1, 1, 2));
            d.placements.Add(Rect(B, 1, 1, 0, 1));
            d.placements.Add(Rect(R, 1, 1, 4, 2));
            d.gates.Add(Shutter(Left, 1, 2, R, true));
            d.gates.Add(Shutter(Right, 3, 2, B, false));
            d.gates.Add(Gate(Up, 4, 2, R));
            d.gates.Add(Gate(Down, 2, 2, B));
            d.gates.Add(Gate(Up, 0, 2, Y));
            d.gates.Add(Gate(Down, 4, 2, Y));
            d.parts.Add(Part(PartType.Door, 0, R, 4, 0));
            d.parts.Add(Part(PartType.Casing, 0, B, 3, 0));
            d.parts.Add(Part(PartType.Casing, 1, Y, 3, 0));
            ResolveHidden(d, false);
            return d;
        }
    }
}
