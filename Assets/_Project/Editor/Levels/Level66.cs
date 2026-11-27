using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Carriage Express (flow breather): two parallel carriage rows and prism blocks shift into matching gates back-to-back for a Color-Chain Overdrive.</summary>
    public sealed class Level66 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(66, "Carriage Express", 7, 7, 13, 5, LevelTier.Normal, "Two carriages and colour-shifting prisms: chain the exits for an Overdrive surge!");
            d.carriages.Add(Carriage(2, 1));
            d.carriages.Add(Carriage(4, -1));
            d.placements.Add(Rect(G, 2, 2, 1, 1));
            d.placements.Add(Rect(Y, 1, 2, 5, 0));
            d.placements.Add(Rect(Y, 2, 1, 4, 2));
            d.placements.Add(Rect(P, 1, 2, 1, 5));
            d.placements.Add(Rect(P, 2, 1, 2, 0));
            d.placements.Add(Rect(B, 1, 2, 2, 3));
            d.placements.Add(Rect(R, 2, 1, 3, 3));
            d.placements.Add(Rect(R, 1, 2, 0, 5));
            d.placements.Add(Rect(B, 2, 1, 4, 5));
            d.placements.Add(Prism(Rect(R, 1, 1, 0, 4), R, B));
            d.placements.Add(Prism(Rect(Y, 1, 1, 6, 6), Y, P));
            d.gates.Add(Gate(Left, 0, 7, R));
            d.gates.Add(Gate(Up, 0, 3, B));
            d.gates.Add(Gate(Down, 4, 3, B));
            d.gates.Add(Gate(Right, 0, 7, Y));
            d.gates.Add(Gate(Up, 4, 3, P));
            d.gates.Add(Gate(Down, 0, 3, P));
            d.gates.Add(Gate(Left, 0, 2, G));
            d.parts.Add(Part(PartType.Hood, 0, R, 3, 0));
            d.parts.Add(Part(PartType.Platen, 0, B, 3, 0));
            d.parts.Add(Part(PartType.Keys, 0, Y, 2, 0));
            d.parts.Add(Part(PartType.Typebar, 0, P, 3, 1));
            d.parts.Add(Part(PartType.Ribbon, 0, G, 2, 1));
            d.parts.Add(Part(PartType.Spring, 0, B, 2, 2));
            d.parts.Add(Part(PartType.Tnt, 0, R, 2, 2));
            ResolveHidden(d, false);
            return d;
        }
    }
}
