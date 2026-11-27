using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Pre-Boss Barista Gridlock: 2-layer ice, welded pairs, shutter gates and steam gates around a rusted scrap-iron core. Only 4 track slots.</summary>
    public sealed class Level49 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(49, "Barista Gridlock", 7, 7, 15, 4, LevelTier.Normal, "Ice, welds, shutters and steam - read the board before you move!");
            d.placements.Add(Scrap(2, 2, 2, 2));
            d.placements.Add(Scrap(1, 1, 5, 5));
            d.placements.Add(Rect(G, 2, 2, 0, 5));
            d.placements.Add(Rect(Y, 1, 2, 1, 2));
            d.placements.Add(Rect(R, 2, 1, 4, 6));
            d.placements.Add(Rect(B, 1, 2, 6, 3));
            d.placements.Add(Ice(Rect(B, 2, 1, 3, 0), 2));
            d.placements.Add(Rect(G, 1, 1, 0, 4));
            d.placements.Add(Link(Rect(Y, 1, 1, 5, 1), 1));
            d.placements.Add(Link(Rect(R, 1, 1, 5, 4), 1));
            d.gates.Add(Steam(Gate(Left, 0, 3, R), 2));
            d.gates.Add(Gate(Up, 4, 2, R));
            d.gates.Add(Shutter(Down, 0, 2, Y, true));
            d.gates.Add(Gate(Right, 4, 2, Y));
            d.gates.Add(Shutter(Left, 4, 2, B, false));
            d.gates.Add(Gate(Down, 4, 2, B));
            d.gates.Add(Gate(Up, 0, 2, G));
            d.gates.Add(Steam(Gate(Right, 0, 2, G), 2));
            d.parts.Add(Part(PartType.Boiler, 0, R, 3, 0));
            d.parts.Add(Part(PartType.Gauge, 0, Y, 1, 0));
            d.parts.Add(Part(PartType.Lever, 0, B, 2, 0));
            d.parts.Add(Part(PartType.GroupHead, 0, G, 3, 1));
            d.parts.Add(Part(PartType.SightTube, 0, R, 1, 1));
            d.parts.Add(Part(PartType.Valve, 0, Y, 2, 2));
            d.parts.Add(Part(PartType.Tnt, 0, B, 2, 2));
            ResolveHidden(d, false);
            return d;
        }
    }
}
