using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Exposed Typebar Basket: the typewriter starts with its hood and platen already blasted off, exposing steel typebars, ink-ribbon spools, two brass screws and twin dynamite bundles.</summary>
    public sealed class Level68 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(68, "Exposed Typebars", 7, 7, 18, 5, LevelTier.Normal, "The hood is gone - shoot the flicking typebars and ribbon spools. Twin dynamite inside!");
            d.placements.Add(Scrap(1, 1, 3, 3));
            d.placements.Add(Scrap(1, 1, 1, 5));
            d.placements.Add(Rect(Y, 2, 2, 4, 2));
            d.placements.Add(Rect(P, 2, 2, 1, 2));
            d.placements.Add(Rect(B, 2, 2, 2, 5));
            d.placements.Add(Rect(R, 2, 2, 4, 4));
            d.placements.Add(Rect(G, 1, 2, 6, 3));
            d.placements.Add(Rect(P, 1, 2, 3, 0));
            d.placements.Add(Rect(G, 2, 1, 1, 0));
            d.placements.Add(Rect(B, 1, 2, 0, 4));
            d.placements.Add(Rect(R, 2, 1, 5, 1));
            d.placements.Add(Rect(Y, 1, 2, 0, 1));
            d.gates.Add(Gate(Down, 0, 2, R));
            d.gates.Add(Gate(Up, 3, 2, R));
            d.gates.Add(Gate(Left, 4, 2, B));
            d.gates.Add(Gate(Right, 0, 2, B));
            d.gates.Add(Gate(Up, 0, 2, Y));
            d.gates.Add(Gate(Down, 4, 2, Y));
            d.gates.Add(Gate(Left, 0, 2, G));
            d.gates.Add(Gate(Right, 4, 2, G));
            d.gates.Add(Gate(Up, 5, 2, P));
            d.gates.Add(Gate(Down, 2, 2, P));
            d.parts.Add(Part(PartType.Typebar, 0, B, 7, 1));
            d.parts.Add(Screw(Part(PartType.Ribbon, 0, Y, 4, 1), P));
            d.parts.Add(Part(PartType.Ribbon, 1, P, 4, 1));
            d.parts.Add(Part(PartType.Rack, 0, R, 3, 2));
            d.parts.Add(Part(PartType.Spring, 0, G, 2, 2));
            d.parts.Add(Part(PartType.Spring, 1, Y, 2, 2));
            d.parts.Add(Screw(Part(PartType.Tnt, 0, R, 2, 2), G));
            d.parts.Add(Part(PartType.Tnt, 1, B, 2, 2));
            ResolveHidden(d, true);
            return d;
        }
    }
}
