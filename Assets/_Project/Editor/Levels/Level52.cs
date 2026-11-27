using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Electromagnet Assist: pull-pads tug blocks across the central gap into line with the clockwork piercing gate that cracks the camera's twin lenses.</summary>
    public sealed class Level52 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(52, "Electromagnet Assist", 6, 6, 8, 5, LevelTier.Normal, "Magnet pads pull a block that rests next to them one tile - for free!");
            d.placements.Add(Scrap(1, 2, 2, 2));
            d.placements.Add(Rect(R, 2, 2, 2, 0));
            d.placements.Add(Rect(B, 1, 2, 0, 3));
            d.placements.Add(Rect(B, 2, 1, 1, 5));
            d.placements.Add(Rect(G, 1, 2, 4, 3));
            d.placements.Add(Rect(Y, 2, 1, 2, 4));
            d.placements.Add(Rect(G, 1, 1, 1, 0));
            d.magnets.Add(Magnet(3, 3, Up));
            d.magnets.Add(Magnet(3, 2, Right));
            d.gates.Add(Piercing(Gate(Up, 2, 2, R)));
            d.gates.Add(Gate(Down, 0, 2, R));
            d.gates.Add(Gate(Left, 3, 2, B));
            d.gates.Add(Gate(Right, 0, 2, B));
            d.gates.Add(Gate(Left, 0, 2, G));
            d.gates.Add(Gate(Down, 4, 2, G));
            d.gates.Add(Gate(Right, 3, 2, Y));
            d.gates.Add(Gate(Up, 0, 2, Y));
            d.parts.Add(Part(PartType.Body, 0, G, 2, 0));
            d.parts.Add(Part(PartType.Hood, 0, Y, 1, 0));
            d.parts.Add(Part(PartType.Lens, 0, R, 2, 0));
            d.parts.Add(Part(PartType.Lens, 1, B, 2, 0));
            d.parts.Add(Part(PartType.Iris, 0, R, 2, 1));
            d.parts.Add(Part(PartType.Crank, 0, G, 1, 1));
            d.parts.Add(Part(PartType.ShutterGear, 0, Y, 1, 2));
            d.parts.Add(Part(PartType.Tnt, 0, B, 2, 2));
            ResolveHidden(d, false);
            return d;
        }
    }
}
