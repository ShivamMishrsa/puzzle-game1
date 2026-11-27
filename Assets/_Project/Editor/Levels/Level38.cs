using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Exposed Ruby Bearings: the lid and porcelain dial are already gone - oscillating balance wheels, ruby bearings and twin TNT mainsprings (20 HP).</summary>
    public sealed class Level38 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(38, "Exposed Ruby Bearings", 7, 7, 20, 5, LevelTier.Normal, "The dial is gone! Free the TNT mainsprings, then smash the wheels.");
            d.placements.Add(Scrap(1, 1, 3, 3));
            d.placements.Add(Scrap(1, 1, 5, 2));
            d.placements.Add(Rect(R, 2, 2, 0, 0));
            d.placements.Add(Rect(G, 2, 1, 0, 5));
            d.placements.Add(Rect(G, 2, 1, 1, 4));
            d.placements.Add(Rect(Y, 1, 2, 6, 4));
            d.placements.Add(Rect(B, 2, 1, 3, 1));
            d.placements.Add(Rect(B, 1, 2, 6, 1));
            d.placements.Add(Rect(Y, 1, 1, 5, 1));
            d.placements.Add(Rect(R, 1, 1, 5, 4));
            d.gates.Add(Gate(Down, 0, 2, R));
            d.gates.Add(Gate(Up, 4, 2, R));
            d.gates.Add(Gate(Left, 4, 2, B));
            d.gates.Add(Gate(Right, 0, 2, B));
            d.gates.Add(Gate(Up, 0, 2, G));
            d.gates.Add(Gate(Down, 4, 2, G));
            d.gates.Add(Gate(Left, 1, 2, Y));
            d.gates.Add(Gate(Right, 4, 2, Y));
            d.parts.Add(Part(PartType.BalanceWheel, 0, R, 5, 1));
            d.parts.Add(Part(PartType.BalanceWheel, 1, B, 5, 1));
            d.parts.Add(Part(PartType.Ruby, 0, R, 2, 1));
            d.parts.Add(Part(PartType.Ruby, 1, G, 3, 1));
            d.parts.Add(Part(PartType.Ruby, 2, Y, 3, 1));
            d.parts.Add(Screw(Part(PartType.Tnt, 0, B, 1, 2), R));
            d.parts.Add(Screw(Part(PartType.Tnt, 1, G, 1, 2), Y));
            ResolveHidden(d, true);
            return d;
        }
    }
}
