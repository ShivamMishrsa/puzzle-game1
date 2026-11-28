using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Disc Jockey (Music Box): first 11x11 tray and a tight clock. Three polarity coils and two repulsors fling blocks around while the glass dome, collar, comb, dancer and pinned discs come off in order. Exact optimal moves.</summary>
    public sealed class Level111 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(111, "Disc Jockey", 11, 11, 31, 4, LevelTier.Normal, "Bigger tray, tighter clock! Coils flip PULL/PUSH after every exit.");
            d.polarities.Add(Polarity(3, 4, true));
            d.polarities.Add(Polarity(7, 6, false));
            d.polarities.Add(Polarity(5, 8, true));
            d.repulsors.Add(Repulsor(5, 5));
            d.repulsors.Add(Repulsor(8, 3));
            d.placements.Add(Scrap(1, 1, 1, 1));
            d.placements.Add(Scrap(1, 1, 9, 9));
            d.placements.Add(Scrap(1, 2, 9, 3));
            d.placements.Add(Rect(G, 2, 2, 0, 5));
            d.placements.Add(Rect(G, 2, 2, 5, 9));
            d.placements.Add(Rect(P, 2, 2, 4, 1));
            d.placements.Add(Rect(C, 2, 2, 0, 3));
            d.placements.Add(Rect(O, 2, 2, 7, 7));
            d.placements.Add(Rect(B, 2, 2, 2, 1));
            d.placements.Add(Rect(Y, 2, 2, 7, 5));
            d.placements.Add(Rect(R, 2, 2, 3, 8));
            d.placements.Add(Rect(R, 2, 1, 5, 5));
            d.placements.Add(Rect(O, 1, 2, 2, 4));
            d.placements.Add(Rect(Y, 1, 2, 10, 1));
            d.placements.Add(Rect(P, 2, 1, 5, 0));
            d.placements.Add(Rect(C, 1, 1, 7, 2));
            d.placements.Add(Rect(B, 1, 1, 3, 3));
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
            d.parts.Add(Part(PartType.Tnt, 1, G, 2, 3));
            ResolveHidden(d, false);
            return d;
        }
    }
}
