using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Steam Pressure Boost: the Blue 1x2 can only crack the copper boiler (4 HP) if it leaves through the steam gate while it is Pressurized (+2 bonus ammo).</summary>
    public sealed class Level42 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(42, "Steam Boost", 6, 6, 8, 5, LevelTier.Normal, "The brass gauge shows the pressure. Leave the steam gate while it is Pressurized for +2 ammo!");
            d.placements.Add(Scrap(1, 2, 3, 3));
            d.placements.Add(Scrap(1, 1, 1, 1));
            d.placements.Add(Rect(Y, 2, 2, 4, 4));
            d.placements.Add(Rect(G, 1, 2, 1, 2));
            d.placements.Add(Rect(R, 1, 2, 2, 4));
            d.placements.Add(Rect(B, 1, 2, 4, 0));
            d.gates.Add(Steam(Gate(Left, 2, 2, B), 2));
            d.gates.Add(Gate(Down, 3, 2, G));
            d.gates.Add(Gate(Up, 0, 2, G));
            d.gates.Add(Gate(Right, 0, 2, R));
            d.gates.Add(Gate(Down, 0, 2, R));
            d.gates.Add(Gate(Up, 3, 2, Y));
            d.gates.Add(Gate(Right, 3, 2, Y));
            d.parts.Add(Part(PartType.Boiler, 0, B, 4, 0));
            d.parts.Add(Part(PartType.Lever, 0, R, 2, 0));
            d.parts.Add(Part(PartType.GroupHead, 0, Y, 2, 1));
            d.parts.Add(Part(PartType.SightTube, 0, G, 1, 1));
            d.parts.Add(Part(PartType.Valve, 0, G, 1, 2));
            d.parts.Add(Part(PartType.Tnt, 0, Y, 2, 2));
            ResolveHidden(d, false);
            return d;
        }
    }
}
