using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Chain Reaction (Tourbillon): portal pair, a clockwork-piercing gate and a carriage row set off a Colour-Chain Overdrive - one polarity coil decides which block gets yanked into the chain.</summary>
    public sealed class Level96 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(96, "Chain Reaction", 9, 9, 23, 4, LevelTier.Normal, "Portal in, carriage over, pierce the gate - chain the colours!");
            d.carriages.Add(Carriage(3, 1));
            d.polarities.Add(Polarity(4, 5, true));
            d.portals.Add(Pad(0, 1, 7, 7, 1));
            d.placements.Add(Scrap(1, 1, 5, 3));
            d.placements.Add(Scrap(1, 2, 2, 4));
            d.placements.Add(Rect(C, 2, 2, 0, 0));
            d.placements.Add(Rect(O, 2, 2, 0, 5));
            d.placements.Add(Rect(G, 1, 2, 6, 2));
            d.placements.Add(Rect(P, 2, 1, 4, 8));
            d.placements.Add(Rect(C, 1, 2, 7, 1));
            d.placements.Add(Rect(B, 1, 2, 2, 0));
            d.placements.Add(Rect(Y, 2, 1, 2, 8));
            d.placements.Add(Rect(R, 2, 1, 0, 2));
            d.placements.Add(Rect(Y, 1, 1, 3, 2));
            d.placements.Add(Rect(O, 1, 1, 6, 1));
            d.placements.Add(Rect(R, 1, 1, 2, 6));
            d.placements.Add(Rect(G, 1, 1, 7, 3));
            d.placements.Add(Rect(B, 1, 1, 5, 2));
            d.placements.Add(Rect(P, 1, 1, 3, 6));
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
            d.gates.Add(Piercing(Gate(Left, 4, 2, O)));
            d.gates.Add(Gate(Down, 6, 2, C));
            d.parts.Add(Part(PartType.Bezel, 0, C, 3, 0));
            d.parts.Add(Part(PartType.Bezel, 1, O, 3, 0));
            d.parts.Add(Part(PartType.Crystal, 0, R, 3, 0));
            d.parts.Add(Part(PartType.Bridge, 0, Y, 3, 1));
            d.parts.Add(Part(PartType.Bridge, 1, G, 3, 1));
            d.parts.Add(Part(PartType.TourbillonCage, 0, P, 3, 1));
            d.parts.Add(Part(PartType.MainBarrel, 0, B, 3, 2));
            d.parts.Add(Part(PartType.Rotor, 0, C, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, O, 2, 3));
            ResolveHidden(d, false);
            d.solution = new int[] { 67371011, 83887878, 201785611, 134611212, 67175694, 201720847, 184811523, 17302024, 17302282, 17302533, 17302543, 134610956, 67436811, 17301516, 67110158, 218105870, 218105863, 201328130, 83886089, 83887618, 218302477, 218236932, 184877067 };
            return d;
        }
    }
}
