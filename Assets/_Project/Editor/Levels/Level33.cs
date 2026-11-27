using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Unblock the Exit Pad: the portal's exit pad is covered by a frozen blue block - thaw it, slide it off, then warp the key block through.</summary>
    public sealed class Level33 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(33, "Unblock the Exit Pad", 7, 7, 12, 5, LevelTier.Normal, "The exit pad is blocked by ice. Free the red block below it to thaw the ice!");
            d.placements.Add(Scrap(1, 1, 3, 3));
            d.placements.Add(Key(Rect(Y, 1, 1, 0, 1), 0));
            d.placements.Add(Ice(Rect(B, 1, 1, 5, 5), 1));
            d.placements.Add(Rect(R, 1, 1, 5, 4));
            d.placements.Add(Rect(R, 2, 1, 4, 0));
            d.placements.Add(Rect(Y, 1, 2, 1, 2));
            d.placements.Add(Rect(B, 2, 1, 2, 1));
            d.placements.Add(Rect(G, 2, 1, 1, 5));
            d.placements.Add(Rect(G, 1, 1, 5, 1));
            d.portals.Add(Pad(0, 1, 1, 5, 5));
            d.gates.Add(Gate(Right, 5, 1, Y));
            d.gates.Add(Gate(Down, 0, 2, Y));
            d.gates.Add(Gate(Down, 4, 2, R));
            d.gates.Add(Gate(Right, 1, 2, R));
            d.gates.Add(Gate(Up, 5, 2, B));
            d.gates.Add(Gate(Left, 0, 2, B));
            d.gates.Add(Padlock(Gate(Up, 0, 3, G), 0));
            d.gates.Add(Padlock(Gate(Left, 3, 3, G), 0));
            d.parts.Add(Part(PartType.Casing, 0, Y, 2, 0));
            d.parts.Add(Part(PartType.Casing, 1, R, 2, 0));
            d.parts.Add(Part(PartType.Glass, 0, B, 2, 1));
            d.parts.Add(Part(PartType.Dial, 0, G, 2, 1));
            ResolveHidden(d, false);
            return d;
        }
    }
}
