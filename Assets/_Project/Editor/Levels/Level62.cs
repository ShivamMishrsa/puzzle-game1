using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Mystery Crate Unboxing: two crates block the corridor. Clearing the Yellow block beside them pops both open, revealing a Red and a Purple block.</summary>
    public sealed class Level62 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(62, "Mystery Crates", 7, 7, 17, 5, LevelTier.Normal, "Crates cannot leave a gate until a neighbouring block has left. Send the Yellow one out!");
            d.placements.Add(Scrap(1, 1, 5, 2));
            d.placements.Add(Rect(Y, 2, 1, 1, 3));
            d.placements.Add(Crate(Rect(R, 1, 1, 3, 3)));
            d.placements.Add(Crate(Rect(P, 1, 1, 2, 4)));
            d.placements.Add(Rect(R, 2, 2, 0, 5));
            d.placements.Add(Rect(B, 2, 2, 1, 1));
            d.placements.Add(Rect(B, 2, 1, 5, 1));
            d.placements.Add(Rect(Y, 1, 2, 5, 4));
            d.placements.Add(Rect(P, 1, 2, 4, 1));
            d.placements.Add(Rect(R, 1, 2, 0, 1));
            d.placements.Add(Rect(P, 2, 1, 4, 6));
            d.placements.Add(Rect(B, 1, 2, 3, 5));
            d.gates.Add(Gate(Left, 3, 2, Y));
            d.gates.Add(Gate(Down, 4, 2, Y));
            d.gates.Add(Gate(Up, 0, 2, R));
            d.gates.Add(Gate(Right, 4, 2, R));
            d.gates.Add(Gate(Down, 0, 2, B));
            d.gates.Add(Gate(Right, 0, 2, B));
            d.gates.Add(Gate(Up, 4, 2, P));
            d.gates.Add(Gate(Left, 0, 2, P));
            d.parts.Add(Part(PartType.Hood, 0, B, 3, 0));
            d.parts.Add(Part(PartType.Platen, 0, R, 3, 0));
            d.parts.Add(Part(PartType.Keys, 0, Y, 2, 0));
            d.parts.Add(Part(PartType.Typebar, 0, P, 3, 1));
            d.parts.Add(Part(PartType.Ribbon, 0, Y, 1, 1));
            d.parts.Add(Part(PartType.Spring, 0, R, 2, 2));
            d.parts.Add(Part(PartType.Tnt, 0, P, 2, 2));
            ResolveHidden(d, false);
            return d;
        }
    }
}
