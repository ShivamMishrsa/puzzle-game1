using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Pre-Boss Ink and Iron: 2-layer ice, mystery crates, a carriage row and an electromagnet around a rusted scrap-iron core. Only 4 track slots.</summary>
    public sealed class Level69 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(69, "Ink and Iron", 7, 7, 25, 4, LevelTier.Normal, "Ice, crates, carriages and magnets - read the whole board before you move!");
            d.carriages.Add(Carriage(1, 1));
            d.magnets.Add(Magnet(4, 3, Right));
            d.placements.Add(Scrap(2, 2, 2, 3));
            d.placements.Add(Scrap(1, 1, 5, 5));
            d.placements.Add(Rect(B, 2, 2, 5, 2));
            d.placements.Add(Rect(Y, 1, 2, 4, 0));
            d.placements.Add(Rect(Y, 2, 1, 0, 2));
            d.placements.Add(Rect(P, 1, 2, 0, 4));
            d.placements.Add(Rect(G, 2, 1, 0, 1));
            d.placements.Add(Ice(Rect(P, 2, 1, 4, 6), 2));
            d.placements.Add(Crate(Rect(G, 1, 2, 3, 5)));
            d.placements.Add(Rect(R, 1, 2, 5, 0));
            d.placements.Add(Rect(R, 2, 1, 1, 5));
            d.placements.Add(Crate(Rect(R, 1, 1, 6, 0)));
            d.placements.Add(Rect(B, 1, 1, 5, 4));
            d.gates.Add(Gate(Left, 4, 2, R));
            d.gates.Add(Gate(Down, 4, 2, R));
            d.gates.Add(Gate(Up, 4, 2, B));
            d.gates.Add(Gate(Right, 0, 2, B));
            d.gates.Add(Gate(Right, 4, 2, Y));
            d.gates.Add(Gate(Down, 0, 2, Y));
            d.gates.Add(Gate(Left, 0, 2, G));
            d.gates.Add(Gate(Up, 0, 2, G));
            d.gates.Add(Gate(Up, 2, 2, P));
            d.gates.Add(Gate(Down, 2, 2, P));
            d.parts.Add(Part(PartType.Hood, 0, B, 3, 0));
            d.parts.Add(Part(PartType.Platen, 0, R, 3, 0));
            d.parts.Add(Part(PartType.Keys, 0, G, 2, 0));
            d.parts.Add(Part(PartType.Typebar, 0, Y, 3, 1));
            d.parts.Add(Part(PartType.Ribbon, 0, P, 2, 1));
            d.parts.Add(Part(PartType.Spring, 0, R, 2, 2));
            d.parts.Add(Part(PartType.Tnt, 0, B, 2, 2));
            ResolveHidden(d, false);
            return d;
        }
    }
}
