using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Portal + Prism Timing: a 1x1 prism block can warp through the cyan portal at any time, but must be Yellow when it lands in front of the right-hand Yellow gate.</summary>
    public sealed class Level57 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(57, "Prism Portal", 7, 7, 12, 5, LevelTier.Normal, "Warp the prism across the wall - but only exit when it glows Yellow!");
            d.placements.Add(Scrap(1, 7, 3, 0));
            d.placements.Add(Prism(Rect(B, 1, 1, 1, 3), B, Y));
            d.placements.Add(Rect(R, 2, 1, 5, 2));
            d.placements.Add(Rect(R, 1, 2, 1, 5));
            d.placements.Add(Rect(G, 2, 1, 4, 0));
            d.placements.Add(Rect(B, 1, 2, 0, 3));
            d.placements.Add(Rect(Y, 1, 2, 4, 2));
            d.placements.Add(Rect(Y, 2, 1, 4, 6));
            d.placements.Add(Rect(G, 1, 1, 5, 1));
            d.portals.Add(Pad(0, 2, 3, 4, 3));
            d.gates.Add(Gate(Right, 3, 1, Y));
            d.gates.Add(Gate(Up, 4, 2, Y));
            d.gates.Add(Gate(Left, 0, 2, B));
            d.gates.Add(Gate(Down, 0, 2, B));
            d.gates.Add(Gate(Left, 4, 2, R));
            d.gates.Add(Gate(Right, 4, 2, R));
            d.gates.Add(Gate(Up, 0, 2, G));
            d.gates.Add(Gate(Down, 4, 2, G));
            d.parts.Add(Part(PartType.Body, 0, B, 2, 0));
            d.parts.Add(Part(PartType.Hood, 0, G, 2, 0));
            d.parts.Add(Part(PartType.Lens, 0, Y, 2, 0));
            d.parts.Add(Part(PartType.Lens, 1, R, 2, 0));
            d.parts.Add(Part(PartType.Iris, 0, Y, 2, 1));
            d.parts.Add(Part(PartType.Crank, 0, B, 1, 1));
            d.parts.Add(Part(PartType.Spool, 0, R, 2, 2));
            d.parts.Add(Part(PartType.Tnt, 0, G, 1, 2));
            ResolveHidden(d, false);
            return d;
        }
    }
}
