using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>MINI-BOSS Pinned Lock: coupled screws, two padlocked gates, polarity coils and a portal pair guard the dome. Three slots, a tight clock and zero spare moves.</summary>
    public sealed class Level115 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(115, "Pinned Lock", 11, 11, 31, 3, LevelTier.MiniBoss, "Three slots! Coupled bolts, padlocks, coils and a portal.");
            d.polarities.Add(Polarity(5, 5, true));
            d.polarities.Add(Polarity(2, 8, false));
            d.portals.Add(Pad(0, 1, 9, 9, 1));
            d.placements.Add(Scrap(1, 2, 3, 2));
            d.placements.Add(Scrap(2, 1, 7, 6));
            d.placements.Add(Rect(C, 2, 2, 7, 2));
            d.placements.Add(Rect(B, 2, 2, 9, 1));
            d.placements.Add(Rect(Y, 2, 2, 4, 5));
            d.placements.Add(Rect(O, 2, 2, 0, 5));
            d.placements.Add(Rect(G, 2, 2, 9, 3));
            d.placements.Add(Rect(P, 2, 2, 4, 8));
            d.placements.Add(Rect(R, 2, 2, 1, 8));
            d.placements.Add(Rect(O, 1, 2, 2, 4));
            d.placements.Add(Rect(Y, 1, 2, 3, 0));
            d.placements.Add(Rect(R, 2, 1, 1, 10));
            d.placements.Add(Rect(P, 2, 1, 4, 10));
            d.placements.Add(Rect(O, 1, 1, 3, 10));
            d.placements.Add(Rect(C, 1, 1, 7, 0));
            d.placements.Add(Rect(P, 1, 1, 0, 8));
            d.placements.Add(Rect(R, 1, 1, 1, 0));
            d.placements.Add(Key(Rect(G, 1, 1, 0, 3), 1));
            d.placements.Add(Rect(B, 1, 1, 1, 1));
            d.placements.Add(Rect(G, 1, 1, 6, 6));
            d.placements.Add(Key(Rect(B, 1, 1, 9, 7), 0));
            d.gates.Add(Gate(Up, 0, 2, R));
            d.gates.Add(Gate(Down, 0, 2, R));
            d.gates.Add(Gate(Left, 0, 2, B));
            d.gates.Add(Gate(Right, 0, 2, B));
            d.gates.Add(Gate(Down, 2, 2, Y));
            d.gates.Add(Padlock(Gate(Up, 2, 2, Y), 1));
            d.gates.Add(Gate(Right, 2, 2, G));
            d.gates.Add(Gate(Left, 2, 2, G));
            d.gates.Add(Padlock(Gate(Up, 4, 2, P), 0));
            d.gates.Add(Gate(Down, 4, 2, P));
            d.gates.Add(Gate(Left, 4, 2, O));
            d.gates.Add(Gate(Down, 6, 2, C));
            d.parts.Add(Screws(Part(PartType.Lid, 0, O, 4, 0), R, B));
            d.parts.Add(Part(PartType.Lid, 1, C, 3, 0));
            d.parts.Add(Part(PartType.Crank, 0, Y, 2, 0));
            d.parts.Add(Screws(Part(PartType.Comb, 0, G, 4, 1), P, O));
            d.parts.Add(Part(PartType.Dancer, 0, P, 4, 1));
            d.parts.Add(Part(PartType.Drum, 0, R, 4, 2));
            d.parts.Add(Part(PartType.Drum, 1, B, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, Y, 2, 3));
            ResolveHidden(d, false);
            d.showRotateHint = true;
            return d;
        }
    }
}
