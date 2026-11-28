using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Welded Passacaglia (Pipe Organ): EXTREME. Four welded pairs, three polarity coils, two repulsors and two ratchets all pushing and pulling at once on the way to a shutter gate.</summary>
    public sealed class Level124 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(124, "Welded Passacaglia", 11, 11, 31, 3, LevelTier.Normal, "Extreme! Welded halves, coils and repulsors all at once.");
            d.polarities.Add(Polarity(3, 5, true));
            d.polarities.Add(Polarity(7, 4, false));
            d.repulsors.Add(Repulsor(5, 7));
            d.repulsors.Add(Repulsor(6, 3));
            d.placements.Add(Scrap(1, 2, 2, 2));
            d.placements.Add(Scrap(2, 1, 7, 8));
            d.placements.Add(Rect(G, 2, 2, 0, 2));
            d.placements.Add(Rect(Y, 2, 2, 2, 0));
            d.placements.Add(Rect(B, 2, 2, 4, 6));
            d.placements.Add(Rect(R, 2, 2, 8, 0));
            d.placements.Add(Rect(P, 2, 2, 4, 8));
            d.placements.Add(Rect(C, 2, 2, 6, 1));
            d.placements.Add(Rect(O, 2, 2, 2, 4));
            d.placements.Add(Rect(Y, 2, 1, 2, 8));
            d.placements.Add(Rect(P, 1, 2, 4, 0));
            d.placements.Add(Rect(C, 2, 1, 1, 10));
            d.placements.Add(Link(Rect(O, 1, 1, 6, 9), 1));
            d.placements.Add(Rect(Y, 1, 1, 1, 6));
            d.placements.Add(Link(Rect(P, 1, 1, 0, 7), 2));
            d.placements.Add(Link(Rect(Y, 1, 1, 1, 8), 2));
            d.placements.Add(Link(Rect(C, 1, 1, 8, 9), 1));
            d.placements.Add(Rect(B, 1, 1, 5, 0));
            d.placements.Add(Link(Rect(R, 1, 1, 1, 1), 3));
            d.placements.Add(Link(Rect(G, 1, 1, 1, 0), 3));
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
            d.parts.Add(Part(PartType.Facade, 0, O, 3, 0));
            d.parts.Add(Part(PartType.Facade, 1, C, 3, 0));
            d.parts.Add(Part(PartType.Pipes, 0, P, 3, 1));
            d.parts.Add(Part(PartType.Pipes, 1, G, 3, 1));
            d.parts.Add(Part(PartType.Keyboard, 0, Y, 3, 1));
            d.parts.Add(Part(PartType.Bellows, 0, R, 3, 2));
            d.parts.Add(Part(PartType.Stops, 0, B, 3, 2));
            d.parts.Add(Part(PartType.Stops, 1, Y, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, C, 2, 3));
            d.parts.Add(Part(PartType.Tnt, 1, P, 2, 3));
            ResolveHidden(d, false);
            d.solution = new int[] { 151257096, 201720333, 151126018, 83887623, 83887114, 196882, 83886595, 17368070, 134808588, 150994961, 67109892, 150994948, 134217733, 83886085, 67437580, 134873099, 17433097, 17433101, 201785870, 67567627, 17433359, 201786635, 201982990, 17433614, 151322636, 83887627, 201655824, 83887632, 17432850, 67305747, 151191571 };
            return d;
        }
    }
}
