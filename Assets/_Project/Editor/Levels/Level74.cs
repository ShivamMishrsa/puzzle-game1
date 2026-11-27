using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Piercing Through 2-Layer Armour (Gramophone): 2-layer ice, a prism block and a clockwork piercing gate that punches through the horn into the bracket in half the shots.</summary>
    public sealed class Level74 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(74, "Piercing Armour", 7, 7, 26, 4, LevelTier.Normal, "Time the prism shift, then launch through the clockwork gate!");
            d.placements.Add(Scrap(1, 1, 3, 3));
            d.placements.Add(Scrap(1, 1, 5, 2));
            d.placements.Add(Rect(Y, 2, 2, 2, 0));
            d.placements.Add(Rect(G, 2, 2, 2, 4));
            d.placements.Add(Rect(R, 2, 2, 1, 2));
            d.placements.Add(Ice(Rect(B, 2, 2, 0, 0), 2));
            d.placements.Add(Rect(Y, 1, 2, 6, 2));
            d.placements.Add(Rect(P, 2, 1, 5, 4));
            d.placements.Add(Rect(G, 1, 2, 4, 5));
            d.placements.Add(Rect(P, 1, 2, 5, 5));
            d.placements.Add(Rect(R, 2, 1, 5, 0));
            d.placements.Add(Rect(R, 1, 2, 1, 4));
            d.placements.Add(Rect(B, 2, 1, 5, 1));
            d.placements.Add(Rect(B, 1, 2, 0, 4));
            d.placements.Add(Rect(P, 1, 1, 6, 5));
            d.placements.Add(Prism(Rect(Y, 1, 1, 2, 6), Y, P));
            d.gates.Add(Piercing(Gate(Up, 0, 2, R)));
            d.gates.Add(Gate(Down, 0, 2, R));
            d.gates.Add(Gate(Left, 0, 2, B));
            d.gates.Add(Gate(Right, 0, 2, B));
            d.gates.Add(Gate(Down, 2, 2, Y));
            d.gates.Add(Gate(Up, 2, 2, Y));
            d.gates.Add(Gate(Right, 2, 2, G));
            d.gates.Add(Gate(Left, 2, 2, G));
            d.gates.Add(Gate(Up, 4, 2, P));
            d.gates.Add(Gate(Down, 4, 2, P));
            d.parts.Add(Part(PartType.Horn, 0, R, 4, 0));
            d.parts.Add(Part(PartType.Bracket, 0, B, 3, 1));
            d.parts.Add(Part(PartType.Record, 0, Y, 3, 1));
            d.parts.Add(Part(PartType.Governor, 0, G, 3, 2));
            d.parts.Add(Part(PartType.Cabinet, 0, P, 3, 2));
            d.parts.Add(Part(PartType.Mainspring, 0, R, 3, 3));
            d.parts.Add(Part(PartType.Tnt, 0, G, 2, 3));
            ResolveHidden(d, false);
            return d;
        }
    }
}
