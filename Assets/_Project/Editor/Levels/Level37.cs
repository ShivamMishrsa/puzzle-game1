using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Peeled Block Through the Portal: the 2x1 yellow block must peel (leave) so its 1x1 red core is small enough to use the portal.</summary>
    public sealed class Level37 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(37, "Peeled Through the Portal", 7, 7, 19, 5, LevelTier.Normal, "A 2x1 block cannot warp. Peel it first - its 1x1 core can!");
            d.placements.Add(Scrap(1, 1, 3, 3));
            d.placements.Add(Peeled(Rect(Y, 2, 1, 1, 5), R));
            d.placements.Add(Rect(R, 2, 2, 3, 0));
            d.placements.Add(Rect(G, 2, 1, 5, 2));
            d.placements.Add(Rect(B, 2, 1, 2, 2));
            d.placements.Add(Rect(B, 1, 2, 0, 1));
            d.placements.Add(Rect(Y, 1, 1, 0, 0));
            d.placements.Add(Rect(G, 1, 1, 6, 4));
            d.portals.Add(Pad(0, 3, 5, 5, 1));
            d.gates.Add(Gate(Left, 5, 1, Y));
            d.gates.Add(Gate(Down, 0, 1, Y));
            d.gates.Add(Gate(Right, 1, 1, R));
            d.gates.Add(Gate(Down, 3, 2, R));
            d.gates.Add(Gate(Up, 4, 2, B));
            d.gates.Add(Gate(Left, 1, 2, B));
            d.gates.Add(Gate(Up, 0, 2, G));
            d.gates.Add(Gate(Right, 3, 2, G));
            d.parts.Add(Part(PartType.Casing, 0, Y, 3, 0));
            d.parts.Add(Part(PartType.Casing, 1, R, 3, 0));
            d.parts.Add(Part(PartType.Dial, 0, B, 3, 1));
            d.parts.Add(Part(PartType.Glass, 0, G, 2, 1));
            ResolveHidden(d, false);
            return d;
        }
    }
}
