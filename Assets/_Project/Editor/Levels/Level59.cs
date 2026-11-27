using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Pre-Boss Shutter Speed: 2-layer ice, a welded pair, prism blocks and electromagnet pads around central rusted scrap iron. Only 4 track slots.</summary>
    public sealed class Level59 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(59, "Shutter Speed", 7, 7, 15, 4, LevelTier.Normal, "Ice, welds, prisms and magnets - plan two moves ahead!");
            d.placements.Add(Scrap(2, 2, 2, 2));
            d.placements.Add(Scrap(1, 1, 5, 5));
            d.placements.Add(Rect(Y, 2, 2, 0, 3));
            d.placements.Add(Rect(R, 2, 1, 4, 1));
            d.placements.Add(Rect(B, 1, 2, 5, 2));
            d.placements.Add(Rect(G, 1, 2, 1, 0));
            d.placements.Add(Ice(Rect(G, 2, 1, 5, 0), 2));
            d.placements.Add(Link(Rect(R, 1, 1, 0, 6), 1));
            d.placements.Add(Prism(Rect(Y, 1, 1, 5, 6), Y, G));
            d.placements.Add(Link(Rect(B, 1, 1, 1, 6), 1));
            d.magnets.Add(Magnet(4, 2, Right));
            d.magnets.Add(Magnet(1, 4, Up));
            d.gates.Add(Gate(Left, 4, 2, R));
            d.gates.Add(Gate(Down, 4, 2, R));
            d.gates.Add(Gate(Up, 4, 2, B));
            d.gates.Add(Gate(Right, 0, 2, B));
            d.gates.Add(Gate(Right, 4, 2, Y));
            d.gates.Add(Gate(Down, 0, 2, Y));
            d.gates.Add(Gate(Left, 0, 2, G));
            d.gates.Add(Gate(Up, 0, 2, G));
            d.parts.Add(Part(PartType.Body, 0, R, 2, 0));
            d.parts.Add(Part(PartType.Hood, 0, G, 1, 0));
            d.parts.Add(Part(PartType.Lens, 0, B, 2, 0));
            d.parts.Add(Part(PartType.Lens, 1, Y, 2, 0));
            d.parts.Add(Part(PartType.Iris, 0, G, 2, 1));
            d.parts.Add(Part(PartType.Crank, 0, R, 1, 1));
            d.parts.Add(Part(PartType.Spool, 0, B, 2, 2));
            d.parts.Add(Part(PartType.Tnt, 0, Y, 2, 2));
            ResolveHidden(d, false);
            return d;
        }
    }
}
