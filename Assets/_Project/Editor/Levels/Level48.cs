using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Exposed Copper Coils: the Espresso Machine starts with its boiler and group head already stripped away, exposing 24 HP of coiled copper pipes, brass valves, two brass screws and twin TNT bundles.</summary>
    public sealed class Level48 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(48, "Exposed Coils", 7, 7, 15, 5, LevelTier.Normal, "The boiler is already gone - blast the coils and valves. Twin TNT inside!");
            d.placements.Add(Scrap(1, 1, 3, 3));
            d.placements.Add(Scrap(1, 1, 1, 5));
            d.placements.Add(Rect(G, 2, 2, 4, 2));
            d.placements.Add(Rect(Y, 2, 2, 0, 3));
            d.placements.Add(Rect(R, 2, 2, 3, 4));
            d.placements.Add(Rect(B, 1, 2, 3, 0));
            d.placements.Add(Rect(G, 1, 2, 0, 0));
            d.placements.Add(Rect(B, 1, 2, 6, 4));
            d.placements.Add(Rect(R, 1, 2, 2, 5));
            d.placements.Add(Rect(R, 2, 1, 1, 1));
            d.placements.Add(Rect(Y, 1, 2, 6, 1));
            d.placements.Add(Rect(Y, 1, 1, 5, 1));
            d.gates.Add(Gate(Down, 0, 2, R));
            d.gates.Add(Gate(Up, 3, 2, R));
            d.gates.Add(Gate(Left, 4, 2, R));
            d.gates.Add(Gate(Right, 0, 2, Y));
            d.gates.Add(Gate(Down, 4, 2, Y));
            d.gates.Add(Gate(Up, 0, 2, Y));
            d.gates.Add(Gate(Left, 0, 2, G));
            d.gates.Add(Gate(Right, 4, 2, G));
            d.gates.Add(Gate(Up, 5, 2, B));
            d.gates.Add(Gate(Down, 2, 2, B));
            d.parts.Add(Part(PartType.Coil, 0, R, 6, 1));
            d.parts.Add(Screw(Part(PartType.Valve, 0, Y, 4, 2), B));
            d.parts.Add(Part(PartType.Valve, 1, G, 4, 2));
            d.parts.Add(Part(PartType.Valve, 2, B, 3, 2));
            d.parts.Add(Screw(Part(PartType.Tnt, 0, R, 2, 2), G));
            d.parts.Add(Part(PartType.Tnt, 1, G, 2, 2));
            d.parts.Add(Part(PartType.Valve, 3, Y, 3, 2));
            ResolveHidden(d, true);
            return d;
        }
    }
}
