using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Frozen Pendulum (Clock Tower): ice, mystery crates, 4-colour prisms and a clockwork carriage row, with polarity coils tugging on the thawed blocks.</summary>
    public sealed class Level103 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(103, "Frozen Pendulum", 10, 10, 33, 4, LevelTier.Normal, "Ice, crates, prisms and a carriage - thaw them in the right order!");
            d.carriages.Add(Carriage(3, 1));
            d.polarities.Add(Polarity(5, 5, true));
            d.placements.Add(Scrap(1, 1, 2, 6));
            d.placements.Add(Scrap(1, 1, 7, 3));
            d.placements.Add(Rect(Y, 2, 2, 3, 3));
            d.placements.Add(Rect(Y, 2, 2, 2, 7));
            d.placements.Add(Rect(O, 2, 2, 0, 4));
            d.placements.Add(Rect(O, 2, 2, 8, 3));
            d.placements.Add(Rect(P, 2, 2, 5, 1));
            d.placements.Add(Rect(B, 2, 2, 8, 6));
            d.placements.Add(Rect(G, 2, 2, 0, 2));
            d.placements.Add(Rect(R, 2, 2, 8, 0));
            d.placements.Add(Rect(C, 2, 2, 2, 0));
            d.placements.Add(Rect(P, 1, 2, 5, 7));
            d.placements.Add(Rect(G, 2, 1, 3, 2));
            d.placements.Add(Rect(C, 2, 1, 6, 6));
            d.placements.Add(Rect(C, 1, 1, 7, 2));
            d.placements.Add(Crate(Rect(B, 1, 1, 7, 5)));
            d.placements.Add(Prism(Ice(Rect(C, 1, 1, 7, 1), 2), C, P, R, O));
            d.placements.Add(Prism(Ice(Rect(O, 1, 1, 2, 5), 2), O, C, P, R));
            d.placements.Add(Rect(R, 1, 1, 1, 0));
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
            d.parts.Add(Part(PartType.Hood, 0, O, 4, 0));
            d.parts.Add(Part(PartType.Hood, 1, C, 3, 0));
            d.parts.Add(Part(PartType.ClockDoor, 0, Y, 4, 0));
            d.parts.Add(Part(PartType.Dial, 0, G, 4, 1));
            d.parts.Add(Part(PartType.Pendulum, 0, P, 4, 1));
            d.parts.Add(Part(PartType.Weight, 0, R, 3, 2));
            d.parts.Add(Part(PartType.Weight, 1, B, 3, 2));
            d.parts.Add(Part(PartType.Chime, 0, O, 3, 2));
            d.parts.Add(Part(PartType.Chime, 1, C, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, Y, 2, 3));
            ResolveHidden(d, false);
            d.solution = new int[] { 134414850, 17302019, 151257092, 264197, 134612237, 151126024, 151126028, 134284294, 83887110, 17302795, 67110157, 201328141, 134546959, 67110159, 83887629, 17367314, 134350350, 83887630, 67593, 201328399, 218106127, 132880, 201328138, 83887626, 83886594, 151322641, 151257093, 134349072, 17367312, 134283273, 17301513, 67110919, 150994951 };
            return d;
        }
    }
}
