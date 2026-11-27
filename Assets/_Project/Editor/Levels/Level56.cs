using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Magnetic Conveyor (flow breather): paired electromagnet pads and horizontal / vertical rail blocks chain-pull blocks into their gates for a quick Color-Chain Overdrive.</summary>
    public sealed class Level56 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(56, "Magnetic Conveyor", 7, 7, 11, 5, LevelTier.Normal, "Rail blocks plus magnet pads: two moves can send three blocks out!");
            d.placements.Add(Rect(Y, 1, 2, 4, 5));
            d.placements.Add(Rect(G, 2, 1, 5, 6));
            d.placements.Add(Rect(G, 1, 2, 2, 0));
            d.placements.Add(Rect(Y, 2, 1, 3, 3));
            d.placements.Add(Arrow(Rect(B, 1, 2, 2, 3), Vertical));
            d.placements.Add(Arrow(Rect(R, 2, 1, 3, 4), Horizontal));
            d.placements.Add(Arrow(Rect(R, 1, 1, 0, 6), Horizontal));
            d.placements.Add(Arrow(Rect(B, 1, 1, 5, 1), Vertical));
            d.placements.Add(Arrow(Rect(R, 1, 1, 5, 2), Horizontal));
            d.magnets.Add(Magnet(2, 3, Left));
            d.magnets.Add(Magnet(4, 3, Right));
            d.magnets.Add(Magnet(3, 2, Down));
            d.magnets.Add(Magnet(3, 4, Up));
            d.gates.Add(Gate(Left, 0, 7, R));
            d.gates.Add(Gate(Up, 0, 3, B));
            d.gates.Add(Gate(Down, 4, 3, B));
            d.gates.Add(Gate(Right, 0, 7, Y));
            d.gates.Add(Gate(Up, 4, 3, G));
            d.gates.Add(Gate(Down, 0, 3, G));
            d.parts.Add(Part(PartType.Body, 0, R, 2, 0));
            d.parts.Add(Part(PartType.Hood, 0, B, 1, 0));
            d.parts.Add(Part(PartType.Lens, 0, Y, 2, 0));
            d.parts.Add(Part(PartType.Iris, 0, B, 2, 1));
            d.parts.Add(Part(PartType.Crank, 0, R, 2, 1));
            d.parts.Add(Part(PartType.ShutterGear, 0, G, 2, 2));
            d.parts.Add(Part(PartType.Tnt, 0, G, 2, 2));
            ResolveHidden(d, false);
            return d;
        }
    }
}
