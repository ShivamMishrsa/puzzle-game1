using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Frozen Mystery Crate: an iced crate needs a neighbour to thaw AND open it, with a clockwork piercing gate that punches through the hood into the typebars.</summary>
    public sealed class Level64 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(64, "Frozen Crate", 7, 7, 17, 5, LevelTier.Normal, "Ice and a crate in one: a single neighbouring exit cracks both. Use the piercing gate!");
            d.placements.Add(Scrap(1, 1, 2, 2));
            d.placements.Add(Rect(R, 2, 2, 0, 1));
            d.placements.Add(Rect(Y, 2, 2, 3, 4));
            d.placements.Add(Rect(G, 1, 2, 5, 1));
            d.placements.Add(Rect(G, 2, 1, 1, 3));
            d.placements.Add(Rect(P, 1, 2, 1, 5));
            d.placements.Add(Rect(P, 2, 1, 2, 6));
            d.placements.Add(Rect(Y, 1, 2, 4, 2));
            d.placements.Add(Rect(B, 1, 2, 0, 4));
            d.placements.Add(Rect(R, 1, 2, 3, 2));
            d.placements.Add(Crate(Ice(Rect(B, 2, 1, 1, 4), 1)));
            d.placements.Add(Rect(G, 1, 1, 1, 0));
            d.placements.Add(Rect(R, 1, 1, 2, 1));
            d.gates.Add(Piercing(Gate(Up, 2, 2, R)));
            d.gates.Add(Gate(Down, 0, 2, R));
            d.gates.Add(Gate(Left, 3, 2, B));
            d.gates.Add(Gate(Right, 0, 2, B));
            d.gates.Add(Gate(Right, 4, 2, Y));
            d.gates.Add(Gate(Up, 5, 2, Y));
            d.gates.Add(Gate(Left, 0, 2, G));
            d.gates.Add(Gate(Down, 4, 2, G));
            d.gates.Add(Gate(Up, 0, 2, P));
            d.gates.Add(Gate(Right, 2, 2, P));
            d.parts.Add(Part(PartType.Hood, 0, R, 3, 0));
            d.parts.Add(Part(PartType.Platen, 0, B, 3, 0));
            d.parts.Add(Part(PartType.Keys, 0, Y, 2, 0));
            d.parts.Add(Part(PartType.Typebar, 0, G, 3, 1));
            d.parts.Add(Part(PartType.Ribbon, 0, P, 2, 1));
            d.parts.Add(Part(PartType.Spring, 0, R, 2, 2));
            d.parts.Add(Part(PartType.Tnt, 0, G, 2, 2));
            ResolveHidden(d, false);
            return d;
        }
    }
}
