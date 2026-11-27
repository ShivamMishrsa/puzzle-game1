using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Exposed Brass Bells: the handset and dial are already stripped off - twin ringer bells, copper coil, two screw-bolts and twin TNT (18 HP).</summary>
    public sealed class Level28 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(28, "Exposed Brass Bells", 7, 7, 15, 5, LevelTier.Normal, "The handset is gone! Unscrew the TNT bolts, then ring the bells.");
            d.placements.Add(Scrap(1, 1, 3, 3));
            d.placements.Add(Scrap(1, 1, 5, 2));
            d.placements.Add(Rect(R, 2, 2, 0, 0));
            d.placements.Add(Rect(G, 2, 1, 5, 1));
            d.placements.Add(Rect(G, 2, 1, 1, 5));
            d.placements.Add(Rect(Y, 1, 2, 4, 5));
            d.placements.Add(Rect(B, 2, 1, 3, 1));
            d.placements.Add(Rect(B, 1, 2, 0, 4));
            d.placements.Add(Rect(Y, 1, 1, 2, 2));
            d.placements.Add(Rect(R, 1, 1, 5, 4));
            d.placements.Add(Rect(G, 1, 1, 1, 4));
            d.gates.Add(Gate(Down, 0, 2, R));
            d.gates.Add(Gate(Up, 4, 2, R));
            d.gates.Add(Gate(Left, 4, 2, B));
            d.gates.Add(Gate(Right, 0, 2, B));
            d.gates.Add(Gate(Up, 0, 2, G));
            d.gates.Add(Gate(Down, 4, 2, G));
            d.gates.Add(Gate(Left, 1, 2, Y));
            d.gates.Add(Gate(Right, 4, 2, Y));
            d.parts.Add(Part(PartType.Ringer, 0, R, 5, 0));
            d.parts.Add(Part(PartType.Ringer, 1, B, 5, 0));
            d.parts.Add(Part(PartType.Coil, 0, G, 6, 1));
            d.parts.Add(Screw(Part(PartType.Tnt, 0, Y, 1, 2), G));
            d.parts.Add(Screw(Part(PartType.Tnt, 1, R, 1, 2), B));
            ResolveHidden(d, true);
            return d;
        }
    }
}
