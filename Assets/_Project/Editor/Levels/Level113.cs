using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Frozen Waltz (Music Box): three ice-sealed 4-colour prisms, mystery crates and a clockwork carriage row with a polarity coil tugging on the thawed blocks - on three slots.</summary>
    public sealed class Level113 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(113, "Frozen Waltz", 11, 11, 38, 3, LevelTier.Normal, "Ice, crates, prisms and a carriage on three slots!");
            d.carriages.Add(Carriage(3, 1));
            d.polarities.Add(Polarity(5, 5, true));
            d.placements.Add(Scrap(1, 1, 2, 7));
            d.placements.Add(Scrap(1, 1, 8, 4));
            d.placements.Add(Scrap(1, 1, 6, 8));
            d.placements.Add(Rect(C, 2, 2, 9, 2));
            d.placements.Add(Rect(B, 2, 2, 2, 3));
            d.placements.Add(Rect(G, 2, 2, 7, 1));
            d.placements.Add(Rect(O, 2, 2, 0, 4));
            d.placements.Add(Rect(P, 2, 2, 8, 7));
            d.placements.Add(Rect(R, 2, 2, 1, 8));
            d.placements.Add(Rect(Y, 2, 2, 3, 8));
            d.placements.Add(Rect(G, 1, 2, 6, 3));
            d.placements.Add(Rect(R, 2, 1, 0, 10));
            d.placements.Add(Rect(P, 1, 1, 5, 9));
            d.placements.Add(Prism(Ice(Rect(O, 1, 1, 2, 5), 2), O, C, P, R));
            d.placements.Add(Rect(O, 1, 1, 2, 6));
            d.placements.Add(Prism(Ice(Rect(C, 1, 1, 8, 0), 2), C, P, R, O));
            d.placements.Add(Rect(Y, 1, 1, 2, 1));
            d.placements.Add(Crate(Rect(B, 1, 1, 1, 1)));
            d.placements.Add(Crate(Rect(Y, 1, 1, 3, 10)));
            d.placements.Add(Prism(Ice(Rect(P, 1, 1, 3, 5), 2), P, R, O, C));
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
            d.parts.Add(Part(PartType.Lid, 0, O, 4, 0));
            d.parts.Add(Part(PartType.Lid, 1, C, 3, 0));
            d.parts.Add(Part(PartType.Crank, 0, Y, 2, 0));
            d.parts.Add(Part(PartType.Comb, 0, G, 4, 1));
            d.parts.Add(Part(PartType.Dancer, 0, P, 4, 1));
            d.parts.Add(Part(PartType.Drum, 0, R, 4, 2));
            d.parts.Add(Part(PartType.Drum, 1, B, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, Y, 2, 3));
            ResolveHidden(d, false);
            return d;
        }
    }
}
