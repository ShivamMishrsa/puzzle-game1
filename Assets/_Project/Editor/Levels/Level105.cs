using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>MINI-BOSS Pendulum Lock: only three track slots. Coupled screws, two padlocked gates, polarity coils and a portal pair guard the hood, dial and chimes. Zero spare moves.</summary>
    public sealed class Level105 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(105, "Pendulum Lock", 10, 10, 28, 3, LevelTier.MiniBoss, "Three slots only! Coupled bolts, padlocks, coils and a portal.");
            d.polarities.Add(Polarity(4, 4, true));
            d.polarities.Add(Polarity(2, 7, false));
            d.portals.Add(Pad(0, 1, 8, 8, 1));
            d.placements.Add(Scrap(1, 2, 3, 2));
            d.placements.Add(Scrap(2, 1, 6, 6));
            d.placements.Add(Rect(C, 2, 2, 2, 8));
            d.placements.Add(Rect(Y, 2, 2, 2, 6));
            d.placements.Add(Rect(O, 2, 2, 0, 5));
            d.placements.Add(Rect(P, 2, 2, 6, 0));
            d.placements.Add(Rect(B, 2, 2, 8, 1));
            d.placements.Add(Rect(G, 2, 2, 0, 1));
            d.placements.Add(Rect(R, 2, 2, 5, 7));
            d.placements.Add(Rect(O, 1, 2, 2, 4));
            d.placements.Add(Rect(C, 2, 1, 5, 4));
            d.placements.Add(Rect(Y, 2, 1, 7, 8));
            d.placements.Add(Rect(C, 1, 1, 5, 2));
            d.placements.Add(Rect(R, 1, 1, 5, 3));
            d.placements.Add(Key(Rect(G, 1, 1, 0, 3), 1));
            d.placements.Add(Rect(P, 1, 1, 5, 9));
            d.placements.Add(Key(Rect(B, 1, 1, 4, 0), 0));
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
            d.parts.Add(Screws(Part(PartType.Hood, 0, O, 4, 0), R, B));
            d.parts.Add(Part(PartType.Hood, 1, C, 3, 0));
            d.parts.Add(Part(PartType.ClockDoor, 0, Y, 3, 0));
            d.parts.Add(Screws(Part(PartType.Dial, 0, G, 4, 1), P, O));
            d.parts.Add(Part(PartType.Pendulum, 0, P, 3, 1));
            d.parts.Add(Part(PartType.Weight, 0, R, 3, 2));
            d.parts.Add(Part(PartType.Weight, 1, B, 3, 2));
            d.parts.Add(Part(PartType.Chime, 0, C, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, Y, 2, 3));
            ResolveHidden(d, false);
            d.showRotateHint = true;
            return d;
        }
    }
}
