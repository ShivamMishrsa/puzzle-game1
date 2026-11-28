using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Welded Polarity (Tourbillon): two welded pairs travel together through polarity coils to a shutter gate - a PUSH shoves one half out of line, a PULL drags the other in.</summary>
    public sealed class Level97 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(97, "Welded Polarity", 9, 9, 31, 4, LevelTier.Normal, "Welded halves react to the coils together. Watch the shutter!");
            d.polarities.Add(Polarity(3, 4, true));
            d.polarities.Add(Polarity(5, 2, false));
            d.placements.Add(Scrap(1, 2, 2, 2));
            d.placements.Add(Scrap(2, 1, 5, 4));
            d.placements.Add(Rect(O, 2, 2, 1, 4));
            d.placements.Add(Rect(C, 2, 2, 3, 6));
            d.placements.Add(Rect(B, 1, 2, 7, 0));
            d.placements.Add(Rect(G, 1, 2, 8, 3));
            d.placements.Add(Rect(Y, 2, 1, 6, 3));
            d.placements.Add(Rect(P, 2, 1, 5, 7));
            d.placements.Add(Rect(R, 2, 1, 2, 0));
            d.placements.Add(Link(Rect(O, 1, 1, 0, 8), 1));
            d.placements.Add(Rect(O, 1, 1, 0, 5));
            d.placements.Add(Rect(G, 1, 1, 1, 3));
            d.placements.Add(Link(Rect(Y, 1, 1, 8, 5), 2));
            d.placements.Add(Link(Rect(C, 1, 1, 2, 8), 1));
            d.placements.Add(Link(Rect(P, 1, 1, 7, 4), 2));
            d.placements.Add(Rect(B, 1, 1, 2, 6));
            d.placements.Add(Rect(R, 1, 1, 1, 8));
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
            d.parts.Add(Part(PartType.Bezel, 0, O, 3, 0));
            d.parts.Add(Part(PartType.Bezel, 1, C, 3, 0));
            d.parts.Add(Part(PartType.Crystal, 0, Y, 3, 0));
            d.parts.Add(Part(PartType.Bridge, 0, P, 3, 1));
            d.parts.Add(Part(PartType.Bridge, 1, G, 3, 1));
            d.parts.Add(Part(PartType.TourbillonCage, 0, R, 3, 1));
            d.parts.Add(Part(PartType.MainBarrel, 0, B, 3, 2));
            d.parts.Add(Part(PartType.Rotor, 0, O, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, C, 2, 3));
            ResolveHidden(d, false);
            d.solution = new int[] { 67175171, 67241989, 134415110, 134677511, 134217736, 151322634, 395276, 134612492, 134611215, 218105860, 201393667, 151257090, 67436553, 83887619, 134480134, 151322633, 17302022, 134610959, 67174415, 151060495, 134611724, 17302284, 218236933, 17302535, 83886088, 17301776, 151191563, 201656077, 83887885, 201851918, 17302542 };
            return d;
        }
    }
}
