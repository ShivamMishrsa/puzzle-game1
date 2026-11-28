using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Exposed Movement (Clock Tower): the hood and glass door are already stripped away - the dial, swinging pendulum, coupled-screw weights, chimes and twin dynamite sticks stand in plain view.</summary>
    public sealed class Level107 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(107, "Exposed Movement", 10, 10, 36, 4, LevelTier.Normal, "No case - shoot the dial and the swinging pendulum, then the weights!");
            d.turnstiles.Add(Turnstile(2, 6, Up));
            d.turnstiles.Add(Turnstile(6, 3, Right));
            d.polarities.Add(Polarity(3, 3, true));
            d.polarities.Add(Polarity(7, 6, false));
            d.placements.Add(Scrap(1, 1, 4, 4));
            d.placements.Add(Scrap(1, 2, 1, 7));
            d.placements.Add(Rect(P, 2, 2, 5, 0));
            d.placements.Add(Rect(G, 2, 2, 8, 6));
            d.placements.Add(Rect(C, 2, 2, 2, 6));
            d.placements.Add(Rect(O, 2, 2, 1, 2));
            d.placements.Add(Rect(O, 2, 2, 0, 4));
            d.placements.Add(Rect(Y, 2, 2, 3, 0));
            d.placements.Add(Rect(R, 2, 2, 1, 0));
            d.placements.Add(Rect(B, 2, 2, 7, 4));
            d.placements.Add(Rect(P, 2, 1, 3, 9));
            d.placements.Add(Rect(R, 2, 1, 4, 8));
            d.placements.Add(Rect(Y, 2, 1, 7, 8));
            d.placements.Add(Rect(C, 1, 2, 7, 0));
            d.placements.Add(Rect(G, 1, 2, 7, 2));
            d.placements.Add(Rect(B, 1, 2, 8, 1));
            d.placements.Add(Rect(C, 1, 1, 3, 2));
            d.placements.Add(Rect(O, 1, 1, 4, 2));
            d.placements.Add(Rect(R, 1, 1, 0, 6));
            d.placements.Add(Rect(G, 1, 1, 9, 3));
            d.placements.Add(Rect(P, 1, 1, 5, 7));
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
            d.parts.Add(Screws(Part(PartType.Dial, 0, C, 4, 0), R, B));
            d.parts.Add(Screws(Part(PartType.Pendulum, 0, O, 4, 0), G, P));
            d.parts.Add(Part(PartType.Weight, 0, Y, 3, 1));
            d.parts.Add(Part(PartType.Weight, 1, R, 3, 1));
            d.parts.Add(Part(PartType.Chime, 0, G, 3, 1));
            d.parts.Add(Part(PartType.Chime, 1, P, 3, 1));
            d.parts.Add(Part(PartType.Tnt, 0, B, 2, 2));
            d.parts.Add(Part(PartType.Tnt, 1, O, 2, 2));
            ResolveHidden(d, false);
            d.solution = new int[] { 134217736, 134218247, 134218754, 201917450, 67110927, 197649, 201459216, 328465, 17367058, 218302739, 83887106, 151257094, 262405, 17368074, 590859, 134807563, 134742540, 17368340, 151257093, 151322641, 83887885, 67240452, 83887632, 218106127, 83886599, 17367564, 67174916, 201328132, 83887620, 83886088, 17367051, 218237198, 67110665, 67241987, 218236931, 150994953 };
            return d;
        }
    }
}
