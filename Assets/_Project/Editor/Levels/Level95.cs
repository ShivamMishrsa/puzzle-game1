using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>MINI-BOSS Skeleton Lock: only three track slots. Coupled screws, two padlocked gates, polarity coils and a portal pair guard the armour. Zero spare moves.</summary>
    public sealed class Level95 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(95, "Skeleton Lock", 9, 9, 29, 3, LevelTier.MiniBoss, "Three slots only! Coupled bolts, padlocks, coils and a portal.");
            d.polarities.Add(Polarity(4, 4, true));
            d.polarities.Add(Polarity(2, 6, false));
            d.portals.Add(Pad(0, 1, 7, 7, 1));
            d.placements.Add(Scrap(1, 2, 3, 2));
            d.placements.Add(Scrap(2, 1, 5, 6));
            d.placements.Add(Rect(O, 2, 2, 0, 5));
            d.placements.Add(Rect(B, 2, 2, 7, 0));
            d.placements.Add(Rect(P, 2, 2, 5, 0));
            d.placements.Add(Rect(C, 2, 2, 0, 1));
            d.placements.Add(Rect(R, 2, 2, 6, 7));
            d.placements.Add(Rect(Y, 2, 2, 2, 5));
            d.placements.Add(Rect(G, 1, 2, 2, 1));
            d.placements.Add(Rect(C, 2, 1, 7, 2));
            d.placements.Add(Rect(Y, 2, 1, 3, 8));
            d.placements.Add(Rect(C, 1, 1, 5, 2));
            d.placements.Add(Rect(O, 1, 1, 3, 4));
            d.placements.Add(Rect(R, 1, 1, 1, 8));
            d.placements.Add(Key(Rect(G, 1, 1, 8, 5), 1));
            d.placements.Add(Key(Rect(B, 1, 1, 5, 3), 0));
            d.placements.Add(Rect(P, 1, 1, 2, 0));
            d.placements.Add(Rect(G, 1, 1, 7, 3));
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
            d.parts.Add(Screws(Part(PartType.Bezel, 0, O, 3, 0), R, B));
            d.parts.Add(Part(PartType.Bezel, 1, C, 3, 0));
            d.parts.Add(Part(PartType.Crystal, 0, Y, 3, 0));
            d.parts.Add(Screws(Part(PartType.Bridge, 0, G, 3, 1), P, O));
            d.parts.Add(Part(PartType.Bridge, 1, P, 3, 1));
            d.parts.Add(Part(PartType.TourbillonCage, 0, R, 3, 1));
            d.parts.Add(Part(PartType.MainBarrel, 0, B, 3, 2));
            d.parts.Add(Part(PartType.Rotor, 0, C, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, Y, 2, 3));
            ResolveHidden(d, false);
            d.showRotateHint = true;
            return d;
        }
    }
}
