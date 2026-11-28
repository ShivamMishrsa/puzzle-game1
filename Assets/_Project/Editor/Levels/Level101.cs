using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Pendulum Swing (Clock Tower): first 10x10 tray with 7 colours. Two polarity coils and a repulsor coil fling blocks around while the hood, glass door, dial and pendulum must come off in order. Exact optimal moves.</summary>
    public sealed class Level101 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(101, "Pendulum Swing", 10, 10, 26, 4, LevelTier.Normal, "Bigger tray, more blocks! Coils flip PULL/PUSH after every exit.");
            d.polarities.Add(Polarity(3, 4, true));
            d.polarities.Add(Polarity(6, 5, false));
            d.repulsors.Add(Repulsor(5, 7));
            d.placements.Add(Scrap(1, 1, 1, 1));
            d.placements.Add(Scrap(1, 1, 8, 8));
            d.placements.Add(Scrap(1, 2, 8, 3));
            d.placements.Add(Rect(O, 2, 2, 2, 5));
            d.placements.Add(Rect(P, 2, 2, 4, 7));
            d.placements.Add(Rect(G, 2, 2, 1, 3));
            d.placements.Add(Rect(C, 2, 2, 5, 3));
            d.placements.Add(Rect(C, 2, 2, 6, 0));
            d.placements.Add(Rect(O, 2, 2, 6, 8));
            d.placements.Add(Rect(G, 2, 2, 2, 1));
            d.placements.Add(Rect(R, 2, 2, 0, 5));
            d.placements.Add(Rect(B, 2, 2, 8, 0));
            d.placements.Add(Rect(Y, 2, 2, 2, 7));
            d.placements.Add(Rect(Y, 2, 2, 6, 6));
            d.placements.Add(Rect(P, 2, 1, 4, 9));
            d.placements.Add(Rect(B, 1, 1, 4, 4));
            d.placements.Add(Rect(R, 1, 1, 1, 2));
            d.placements.Add(Rect(O, 1, 1, 7, 4));
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
            d.parts.Add(Part(PartType.Hood, 0, O, 4, 0));
            d.parts.Add(Part(PartType.Hood, 1, C, 3, 0));
            d.parts.Add(Part(PartType.ClockDoor, 0, Y, 4, 0));
            d.parts.Add(Part(PartType.Dial, 0, G, 4, 1));
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
