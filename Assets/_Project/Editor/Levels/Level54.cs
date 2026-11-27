using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Frozen Prism: an iced prism block keeps cycling Blue->Red->Yellow while frozen. Thaw it on the turn its colour matches the open shutter gate.</summary>
    public sealed class Level54 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(54, "Frozen Prism", 7, 7, 15, 5, LevelTier.Normal, "The frozen prism still shifts colour. Thaw it at the right moment!");
            d.placements.Add(Scrap(1, 2, 4, 2));
            d.placements.Add(Prism(Ice(Rect(B, 1, 1, 3, 4), 1), B, R, Y));
            d.placements.Add(Rect(Y, 1, 2, 3, 5));
            d.placements.Add(Rect(B, 2, 2, 0, 2));
            d.placements.Add(Rect(G, 1, 2, 2, 2));
            d.placements.Add(Rect(R, 1, 2, 5, 3));
            d.placements.Add(Rect(R, 2, 1, 1, 1));
            d.placements.Add(Rect(G, 1, 1, 1, 5));
            d.gates.Add(Shutter(Up, 3, 1, R, true));
            d.gates.Add(Gate(Left, 0, 2, R));
            d.gates.Add(Gate(Right, 4, 2, Y));
            d.gates.Add(Gate(Down, 4, 2, Y));
            d.gates.Add(Gate(Down, 0, 2, B));
            d.gates.Add(Gate(Left, 4, 2, B));
            d.gates.Add(Gate(Up, 0, 2, G));
            d.gates.Add(Gate(Right, 0, 2, G));
            d.parts.Add(Part(PartType.Body, 0, Y, 2, 0));
            d.parts.Add(Part(PartType.Hood, 0, B, 2, 0));
            d.parts.Add(Part(PartType.Lens, 0, R, 2, 0));
            d.parts.Add(Part(PartType.Lens, 1, G, 1, 0));
            d.parts.Add(Part(PartType.Iris, 0, R, 2, 1));
            d.parts.Add(Part(PartType.Crank, 0, Y, 1, 1));
            d.parts.Add(Part(PartType.Spool, 0, B, 2, 2));
            d.parts.Add(Part(PartType.Tnt, 0, G, 2, 2));
            ResolveHidden(d, false);
            return d;
        }
    }
}
