using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>3-Layer Vault Armour (Orrery): zodiac rings Orange, then Purple planets, then the Cyan gear cage. Exits must follow the sequence; Mega-Turrets pierce a layer.</summary>
    public sealed class Level82 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(82, "Vault Armour", 8, 8, 17, 4, LevelTier.Normal, "Three armour layers: Orange rings, Purple planets, Cyan cage. Merge turrets to pierce!");
            d.placements.Add(Scrap(1, 1, 3, 3));
            d.placements.Add(Scrap(1, 1, 5, 4));
            d.placements.Add(Rect(C, 2, 2, 6, 0));
            d.placements.Add(Rect(P, 2, 2, 4, 5));
            d.placements.Add(Rect(O, 2, 2, 1, 4));
            d.placements.Add(Rect(O, 2, 2, 4, 2));
            d.placements.Add(Rect(B, 2, 2, 2, 1));
            d.placements.Add(Rect(R, 2, 2, 6, 3));
            d.placements.Add(Rect(G, 2, 2, 0, 2));
            d.placements.Add(Rect(Y, 2, 2, 2, 6));
            d.placements.Add(Rect(P, 2, 1, 4, 1));
            d.placements.Add(Rect(C, 1, 1, 7, 2));
            d.placements.Add(Rect(P, 1, 1, 5, 0));
            d.gates.Add(Gate(Up, 0, 2, R));
            d.gates.Add(Gate(Down, 0, 2, R));
            d.gates.Add(Gate(Left, 0, 2, B));
            d.gates.Add(Gate(Right, 0, 2, B));
            d.gates.Add(Gate(Down, 2, 2, Y));
            d.gates.Add(Gate(Up, 2, 2, Y));
            d.gates.Add(Gate(Right, 2, 2, G));
            d.gates.Add(Gate(Left, 2, 2, G));
            d.gates.Add(Gate(Up, 4, 2, P));
            d.gates.Add(Gate(Down, 4, 2, P));
            d.gates.Add(Gate(Left, 4, 2, O));
            d.gates.Add(Gate(Down, 6, 2, C));
            d.parts.Add(Part(PartType.Ring, 0, O, 4, 0));
            d.parts.Add(Part(PartType.Ring, 1, O, 3, 0));
            d.parts.Add(Part(PartType.Planet, 0, P, 3, 1));
            d.parts.Add(Part(PartType.Planet, 1, P, 3, 1));
            d.parts.Add(Part(PartType.Planet, 2, R, 3, 1));
            d.parts.Add(Part(PartType.Cage, 0, C, 4, 2));
            d.parts.Add(Part(PartType.Tnt, 0, G, 2, 3));
            d.parts.Add(Part(PartType.Tnt, 1, B, 2, 3));
            d.parts.Add(Part(PartType.Ring, 2, Y, 2, 0));
            ResolveHidden(d, false);
            return d;
        }
    }
}
