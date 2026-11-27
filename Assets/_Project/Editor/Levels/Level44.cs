using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Frozen Weld: the Blue half of a welded pair is encased in ice, locking the whole pair until the Red 1x2 next to it leaves and thaws it.</summary>
    public sealed class Level44 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(44, "Frozen Weld", 7, 7, 13, 5, LevelTier.Normal, "A frozen half locks the whole welded pair. Send the neighbour out to thaw it!");
            d.placements.Add(Scrap(2, 1, 5, 5));
            d.placements.Add(Link(Ice(Rect(B, 1, 1, 3, 3), 1), 1));
            d.placements.Add(Link(Rect(Y, 1, 1, 4, 3), 1));
            d.placements.Add(Rect(R, 1, 2, 3, 4));
            d.placements.Add(Rect(Y, 2, 2, 1, 1));
            d.placements.Add(Rect(G, 2, 1, 4, 2));
            d.placements.Add(Rect(B, 1, 2, 0, 4));
            d.placements.Add(Rect(R, 2, 1, 1, 4));
            d.placements.Add(Rect(G, 1, 1, 0, 2));
            d.gates.Add(Gate(Up, 3, 1, R));
            d.gates.Add(Gate(Left, 0, 2, R));
            d.gates.Add(Gate(Right, 4, 2, Y));
            d.gates.Add(Gate(Down, 4, 2, Y));
            d.gates.Add(Gate(Left, 3, 2, B));
            d.gates.Add(Gate(Down, 0, 2, B));
            d.gates.Add(Gate(Up, 0, 2, G));
            d.gates.Add(Gate(Right, 0, 2, G));
            d.parts.Add(Part(PartType.Boiler, 0, R, 3, 0));
            d.parts.Add(Part(PartType.Gauge, 0, G, 2, 0));
            d.parts.Add(Part(PartType.Lever, 0, Y, 1, 0));
            d.parts.Add(Part(PartType.GroupHead, 0, B, 2, 1));
            d.parts.Add(Part(PartType.SightTube, 0, Y, 2, 1));
            d.parts.Add(Part(PartType.Valve, 0, G, 1, 2));
            d.parts.Add(Part(PartType.Tnt, 0, Y, 2, 2));
            d.parts.Add(Part(PartType.Valve, 1, B, 1, 2));
            ResolveHidden(d, false);
            return d;
        }
    }
}
