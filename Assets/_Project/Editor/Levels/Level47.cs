using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Peeled & Linked: a dual-layer peeled block and a welded pair guard the route to the brass key that unlocks the red gate.</summary>
    public sealed class Level47 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(47, "Peeled and Linked", 7, 7, 13, 5, LevelTier.Normal, "Peel the block, snap the weld, grab the key!");
            d.placements.Add(Scrap(1, 1, 3, 3));
            d.placements.Add(Scrap(1, 2, 5, 4));
            d.placements.Add(Key(Rect(B, 1, 1, 0, 6), 0));
            d.placements.Add(Rect(R, 2, 2, 3, 0));
            d.placements.Add(Rect(G, 2, 1, 1, 1));
            d.placements.Add(Rect(B, 1, 2, 0, 4));
            d.placements.Add(Peeled(Rect(Y, 2, 1, 5, 1), R));
            d.placements.Add(Rect(Y, 1, 1, 5, 0));
            d.placements.Add(Link(Rect(R, 1, 1, 2, 6), 1));
            d.placements.Add(Link(Rect(G, 1, 1, 1, 4), 1));
            d.gates.Add(Padlock(Gate(Down, 0, 2, R), 0));
            d.gates.Add(Padlock(Gate(Right, 4, 2, R), 0));
            d.gates.Add(Gate(Left, 4, 2, B));
            d.gates.Add(Gate(Up, 4, 2, B));
            d.gates.Add(Gate(Up, 0, 2, G));
            d.gates.Add(Gate(Left, 0, 2, G));
            d.gates.Add(Gate(Down, 4, 2, Y));
            d.gates.Add(Gate(Right, 0, 2, Y));
            d.parts.Add(Part(PartType.Boiler, 0, Y, 2, 0));
            d.parts.Add(Part(PartType.Gauge, 0, G, 1, 0));
            d.parts.Add(Part(PartType.GroupHead, 0, R, 3, 1));
            d.parts.Add(Part(PartType.SightTube, 0, B, 2, 1));
            d.parts.Add(Part(PartType.Valve, 0, R, 2, 2));
            d.parts.Add(Part(PartType.Tnt, 0, G, 2, 2));
            ResolveHidden(d, false);
            return d;
        }
    }
}
