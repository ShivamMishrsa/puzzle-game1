using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Exposed Film Spools & Iris: the TLR Camera starts with its leatherette shell and lenses already shattered, exposing 24 HP of iris blades, spinning film spools, two brass screws and twin TNT bundles.</summary>
    public sealed class Level58 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(58, "Exposed Film Spools", 7, 7, 13, 5, LevelTier.Normal, "The shell is already gone - shoot the iris, spools and gears. Twin TNT inside!");
            d.placements.Add(Scrap(1, 1, 3, 3));
            d.placements.Add(Scrap(1, 1, 5, 1));
            d.placements.Add(Rect(Y, 2, 2, 0, 3));
            d.placements.Add(Rect(G, 2, 2, 2, 1));
            d.placements.Add(Rect(R, 2, 2, 0, 0));
            d.placements.Add(Rect(B, 1, 2, 6, 4));
            d.placements.Add(Rect(B, 2, 1, 5, 3));
            d.placements.Add(Rect(G, 1, 2, 3, 5));
            d.placements.Add(Rect(R, 2, 1, 1, 5));
            d.placements.Add(Rect(R, 1, 2, 4, 5));
            d.placements.Add(Rect(Y, 1, 2, 4, 0));
            d.placements.Add(Rect(Y, 1, 1, 4, 4));
            d.placements.Add(Rect(B, 1, 1, 5, 5));
            d.gates.Add(Gate(Down, 0, 2, R));
            d.gates.Add(Gate(Up, 3, 2, R));
            d.gates.Add(Gate(Left, 4, 2, R));
            d.gates.Add(Gate(Right, 0, 2, Y));
            d.gates.Add(Gate(Down, 4, 2, Y));
            d.gates.Add(Gate(Up, 0, 2, Y));
            d.gates.Add(Gate(Left, 0, 2, B));
            d.gates.Add(Gate(Right, 4, 2, B));
            d.gates.Add(Gate(Up, 5, 2, G));
            d.gates.Add(Gate(Down, 2, 2, G));
            d.parts.Add(Part(PartType.Iris, 0, R, 6, 1));
            d.parts.Add(Part(PartType.Crank, 0, Y, 3, 1));
            d.parts.Add(Screw(Part(PartType.Spool, 0, B, 4, 2), Y));
            d.parts.Add(Part(PartType.Spool, 1, G, 4, 2));
            d.parts.Add(Part(PartType.ShutterGear, 0, Y, 3, 2));
            d.parts.Add(Screw(Part(PartType.Tnt, 0, R, 2, 2), B));
            d.parts.Add(Part(PartType.Tnt, 1, G, 2, 2));
            ResolveHidden(d, true);
            return d;
        }
    }
}
