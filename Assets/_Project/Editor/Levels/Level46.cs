using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Steam Cascade (flow breather): two steam gates and aligned horizontal / vertical rail blocks chain quickly into a Color-Chain Overdrive and a Twin-Barrel Mega-Turret.</summary>
    public sealed class Level46 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(46, "Steam Cascade", 7, 7, 9, 5, LevelTier.Normal, "Rail blocks slide one way only. Chain three of a colour for an Overdrive surge!");
            d.placements.Add(Rect(Y, 2, 1, 3, 6));
            d.placements.Add(Rect(G, 2, 1, 1, 0));
            d.placements.Add(Rect(G, 1, 2, 2, 1));
            d.placements.Add(Rect(Y, 1, 2, 5, 5));
            d.placements.Add(Arrow(Rect(R, 2, 1, 0, 1), Horizontal));
            d.placements.Add(Arrow(Rect(R, 2, 1, 1, 6), Horizontal));
            d.placements.Add(Arrow(Rect(B, 1, 2, 6, 0), Vertical));
            d.placements.Add(Arrow(Rect(B, 1, 1, 0, 6), Vertical));
            d.placements.Add(Arrow(Rect(R, 1, 1, 4, 4), Horizontal));
            d.gates.Add(Steam(Gate(Left, 0, 7, R), 2));
            d.gates.Add(Gate(Up, 0, 3, B));
            d.gates.Add(Gate(Down, 4, 3, B));
            d.gates.Add(Steam(Gate(Right, 0, 7, Y), 2));
            d.gates.Add(Gate(Up, 4, 3, G));
            d.gates.Add(Gate(Down, 0, 3, G));
            d.parts.Add(Part(PartType.Boiler, 0, R, 3, 0));
            d.parts.Add(Part(PartType.Gauge, 0, B, 1, 0));
            d.parts.Add(Part(PartType.Lever, 0, Y, 2, 0));
            d.parts.Add(Part(PartType.GroupHead, 0, R, 2, 1));
            d.parts.Add(Part(PartType.DripGrid, 0, B, 2, 1));
            d.parts.Add(Part(PartType.Valve, 0, G, 2, 2));
            d.parts.Add(Part(PartType.Tnt, 0, G, 2, 2));
            ResolveHidden(d, false);
            return d;
        }
    }
}
