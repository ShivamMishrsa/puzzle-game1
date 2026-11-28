using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Coupled Chimes (Clock Tower): the hood and the dial are chained down by coupled screws - up to three linked bolts of different colours - while two one-way ratchets guard the middle of the tray.</summary>
    public sealed class Level102 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(102, "Coupled Chimes", 10, 10, 38, 4, LevelTier.Normal, "Linked bolts on the hood and dial - every screw needs its own colour!");
            d.turnstiles.Add(Turnstile(4, 5, Right));
            d.turnstiles.Add(Turnstile(5, 4, Up));
            d.placements.Add(Scrap(1, 2, 3, 3));
            d.placements.Add(Scrap(2, 1, 6, 6));
            d.placements.Add(Scrap(1, 1, 8, 1));
            d.placements.Add(Rect(P, 2, 2, 4, 7));
            d.placements.Add(Rect(C, 2, 2, 6, 0));
            d.placements.Add(Rect(G, 2, 2, 2, 5));
            d.placements.Add(Rect(C, 2, 2, 0, 3));
            d.placements.Add(Rect(O, 2, 2, 0, 0));
            d.placements.Add(Rect(O, 2, 2, 0, 5));
            d.placements.Add(Rect(Y, 2, 2, 2, 8));
            d.placements.Add(Rect(B, 2, 2, 2, 0));
            d.placements.Add(Rect(R, 2, 2, 0, 8));
            d.placements.Add(Rect(G, 1, 2, 2, 3));
            d.placements.Add(Rect(Y, 2, 1, 4, 4));
            d.placements.Add(Rect(P, 2, 1, 4, 9));
            d.placements.Add(Rect(O, 1, 1, 8, 5));
            d.placements.Add(Rect(Y, 1, 1, 6, 4));
            d.placements.Add(Rect(B, 1, 1, 9, 1));
            d.placements.Add(Rect(G, 1, 1, 9, 2));
            d.placements.Add(Rect(R, 1, 1, 5, 1));
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
            d.parts.Add(Screws(Part(PartType.Hood, 0, O, 4, 0), R, B));
            d.parts.Add(Part(PartType.Hood, 1, C, 3, 0));
            d.parts.Add(Part(PartType.ClockDoor, 0, Y, 4, 0));
            d.parts.Add(Screws(Part(PartType.Dial, 0, G, 4, 1), P, C, O));
            d.parts.Add(Part(PartType.Pendulum, 0, P, 4, 1));
            d.parts.Add(Part(PartType.Weight, 0, R, 3, 2));
            d.parts.Add(Part(PartType.Weight, 1, B, 3, 2));
            d.parts.Add(Part(PartType.Chime, 0, O, 3, 2));
            d.parts.Add(Part(PartType.Chime, 1, C, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, Y, 2, 3));
            d.parts.Add(Part(PartType.Tnt, 1, G, 2, 3));
            ResolveHidden(d, false);
            return d;
        }
    }
}
