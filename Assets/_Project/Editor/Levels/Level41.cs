using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Welded Twin-Linked introduction (Espresso Machine): a Red 1x1 and a Blue 1x1 are welded by a steel bridge bar; they slide as one unit. Sending the Red half out snaps the bar and frees the Blue block.</summary>
    public sealed class Level41 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(41, "Welded Pair", 6, 6, 9, 5, LevelTier.Normal, "Welded blocks slide together. Push the Red half into its gate - the weld snaps and frees the Blue one!");
            d.placements.Add(Rect(R, 2, 2, 1, 4));
            d.placements.Add(Rect(B, 1, 2, 3, 0));
            d.placements.Add(Rect(Y, 1, 2, 4, 2));
            d.placements.Add(Link(Rect(R, 1, 1, 0, 5), 1));
            d.placements.Add(Link(Rect(B, 1, 1, 0, 3), 1));
            d.gates.Add(Gate(Down, 0, 2, R));
            d.gates.Add(Gate(Up, 4, 2, R));
            d.gates.Add(Gate(Left, 2, 2, B));
            d.gates.Add(Gate(Right, 0, 2, B));
            d.gates.Add(Gate(Right, 3, 2, Y));
            d.gates.Add(Gate(Up, 1, 2, Y));
            d.parts.Add(Part(PartType.Boiler, 0, R, 2, 0));
            d.parts.Add(Part(PartType.Gauge, 0, Y, 1, 0));
            d.parts.Add(Part(PartType.GroupHead, 0, B, 2, 1));
            d.parts.Add(Part(PartType.DripGrid, 0, Y, 1, 1));
            d.parts.Add(Part(PartType.Valve, 0, R, 1, 2));
            d.parts.Add(Part(PartType.Valve, 1, B, 1, 2));
            d.parts.Add(Part(PartType.Tnt, 0, R, 2, 2));
            ResolveHidden(d, false);
            return d;
        }
    }
}
