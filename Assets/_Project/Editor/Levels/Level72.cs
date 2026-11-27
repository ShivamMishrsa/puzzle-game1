using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Zero-Margin Colour Order (Gramophone): five colours, four conveyor slots. Exits must follow the armour sequence outer to inner. Exact optimal moves.</summary>
    public sealed class Level72 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(72, "Colour Order", 7, 7, 24, 4, LevelTier.Normal, "Outer shell colours first, inner cores second - without clogging the 4 slots!");
            d.placements.Add(Scrap(1, 1, 2, 2));
            d.placements.Add(Scrap(1, 1, 4, 4));
            d.placements.Add(Rect(B, 2, 2, 2, 4));
            d.placements.Add(Rect(Y, 2, 2, 3, 2));
            d.placements.Add(Rect(P, 2, 2, 4, 0));
            d.placements.Add(Rect(R, 2, 2, 0, 5));
            d.placements.Add(Rect(G, 2, 1, 1, 3));
            d.placements.Add(Rect(G, 1, 2, 0, 2));
            d.placements.Add(Rect(P, 1, 2, 6, 0));
            d.placements.Add(Rect(Y, 1, 2, 5, 4));
            d.placements.Add(Rect(B, 1, 2, 1, 0));
            d.placements.Add(Rect(R, 2, 1, 2, 0));
            d.placements.Add(Rect(R, 1, 2, 5, 2));
            d.placements.Add(Rect(B, 2, 1, 2, 1));
            d.placements.Add(Rect(P, 1, 1, 6, 6));
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
            d.parts.Add(Part(PartType.Horn, 0, Y, 3, 0));
            d.parts.Add(Part(PartType.Horn, 1, R, 3, 0));
            d.parts.Add(Part(PartType.Bracket, 0, B, 3, 1));
            d.parts.Add(Part(PartType.Record, 0, P, 3, 1));
            d.parts.Add(Part(PartType.Governor, 0, G, 3, 2));
            d.parts.Add(Part(PartType.Cabinet, 0, R, 3, 2));
            d.parts.Add(Part(PartType.Mainspring, 0, P, 3, 3));
            d.parts.Add(Part(PartType.Tnt, 0, B, 2, 3));
            ResolveHidden(d, false);
            return d;
        }
    }
}
