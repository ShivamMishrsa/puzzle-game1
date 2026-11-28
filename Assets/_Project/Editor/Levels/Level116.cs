using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Welded Waltz (Music Box): four welded pairs travel together past polarity coils and two repulsors to a shutter gate - on three slots.</summary>
    public sealed class Level116 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(116, "Welded Waltz", 11, 11, 34, 3, LevelTier.Normal, "Welded halves react to the coils together. Watch the shutter!");
            d.polarities.Add(Polarity(3, 5, true));
            d.polarities.Add(Polarity(7, 4, false));
            d.repulsors.Add(Repulsor(5, 7));
            d.placements.Add(Scrap(1, 2, 2, 2));
            d.placements.Add(Scrap(2, 1, 7, 8));
            d.placements.Add(Rect(R, 2, 2, 9, 9));
            d.placements.Add(Rect(P, 2, 2, 4, 7));
            d.placements.Add(Rect(B, 2, 2, 7, 0));
            d.placements.Add(Rect(Y, 2, 2, 3, 4));
            d.placements.Add(Rect(G, 2, 2, 1, 7));
            d.placements.Add(Rect(O, 2, 2, 5, 0));
            d.placements.Add(Rect(C, 2, 2, 6, 2));
            d.placements.Add(Link(Rect(O, 1, 1, 2, 5), 1));
            d.placements.Add(Rect(P, 1, 1, 5, 2));
            d.placements.Add(Rect(G, 1, 1, 9, 3));
            d.placements.Add(Rect(O, 1, 1, 0, 5));
            d.placements.Add(Rect(Y, 1, 1, 10, 5));
            d.placements.Add(Link(Rect(P, 1, 1, 6, 8), 2));
            d.placements.Add(Link(Rect(Y, 1, 1, 7, 7), 2));
            d.placements.Add(Link(Rect(C, 1, 1, 1, 4), 1));
            d.placements.Add(Link(Rect(G, 1, 1, 10, 3), 3));
            d.placements.Add(Rect(B, 1, 1, 2, 1));
            d.placements.Add(Rect(R, 1, 1, 1, 0));
            d.placements.Add(Link(Rect(R, 1, 1, 10, 1), 3));
            d.gates.Add(Gate(Up, 0, 2, R));
            d.gates.Add(Gate(Down, 0, 2, R));
            d.gates.Add(Gate(Left, 0, 2, B));
            d.gates.Add(Gate(Right, 0, 2, B));
            d.gates.Add(Gate(Down, 2, 2, Y));
            d.gates.Add(Shutter(Up, 2, 2, Y, false));
            d.gates.Add(Gate(Right, 2, 2, G));
            d.gates.Add(Gate(Left, 2, 2, G));
            d.gates.Add(Gate(Up, 4, 2, P));
            d.gates.Add(Gate(Down, 4, 2, P));
            d.gates.Add(Shutter(Left, 4, 2, O, true));
            d.gates.Add(Gate(Down, 6, 2, C));
            d.parts.Add(Part(PartType.Lid, 0, O, 4, 0));
            d.parts.Add(Part(PartType.Lid, 1, C, 3, 0));
            d.parts.Add(Part(PartType.Crank, 0, Y, 2, 0));
            d.parts.Add(Part(PartType.Comb, 0, P, 4, 1));
            d.parts.Add(Part(PartType.Dancer, 0, G, 4, 1));
            d.parts.Add(Part(PartType.Drum, 0, R, 4, 2));
            d.parts.Add(Part(PartType.Drum, 1, B, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, Y, 2, 3));
            ResolveHidden(d, false);
            return d;
        }
    }
}
