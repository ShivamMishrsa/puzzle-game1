using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Welded Weights (Clock Tower): three welded pairs travel together past polarity coils and a repulsor to a shutter gate - a PUSH shoves one half out of line, a PULL drags the other in.</summary>
    public sealed class Level106 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(106, "Welded Weights", 10, 10, 38, 4, LevelTier.Normal, "Welded halves react to the coils together. Watch the shutter!");
            d.polarities.Add(Polarity(3, 4, true));
            d.polarities.Add(Polarity(6, 3, false));
            d.repulsors.Add(Repulsor(5, 6));
            d.placements.Add(Scrap(1, 2, 2, 2));
            d.placements.Add(Scrap(2, 1, 6, 5));
            d.placements.Add(Rect(P, 2, 2, 5, 1));
            d.placements.Add(Rect(G, 2, 2, 3, 2));
            d.placements.Add(Rect(Y, 2, 2, 7, 1));
            d.placements.Add(Rect(O, 2, 2, 2, 4));
            d.placements.Add(Rect(B, 2, 2, 8, 8));
            d.placements.Add(Rect(C, 2, 2, 6, 8));
            d.placements.Add(Rect(O, 2, 1, 8, 6));
            d.placements.Add(Rect(Y, 1, 2, 3, 8));
            d.placements.Add(Rect(C, 1, 2, 5, 3));
            d.placements.Add(Rect(R, 2, 1, 3, 7));
            d.placements.Add(Rect(O, 1, 1, 7, 6));
            d.placements.Add(Link(Rect(Y, 1, 1, 3, 1), 2));
            d.placements.Add(Link(Rect(O, 1, 1, 0, 5), 1));
            d.placements.Add(Link(Rect(C, 1, 1, 1, 6), 1));
            d.placements.Add(Link(Rect(R, 1, 1, 7, 4), 3));
            d.placements.Add(Rect(R, 1, 1, 1, 2));
            d.placements.Add(Link(Rect(P, 1, 1, 3, 0), 2));
            d.placements.Add(Link(Rect(G, 1, 1, 8, 3), 3));
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
            d.parts.Add(Part(PartType.Hood, 0, O, 4, 0));
            d.parts.Add(Part(PartType.Hood, 1, C, 3, 0));
            d.parts.Add(Part(PartType.ClockDoor, 0, Y, 4, 0));
            d.parts.Add(Part(PartType.Dial, 0, P, 4, 1));
            d.parts.Add(Part(PartType.Pendulum, 0, G, 4, 1));
            d.parts.Add(Part(PartType.Weight, 0, R, 3, 2));
            d.parts.Add(Part(PartType.Weight, 1, B, 3, 2));
            d.parts.Add(Part(PartType.Chime, 0, O, 3, 2));
            d.parts.Add(Part(PartType.Chime, 1, C, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, Y, 2, 3));
            ResolveHidden(d, false);
            return d;
        }
    }
}
