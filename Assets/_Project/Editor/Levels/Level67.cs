using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Unbox the Key: a mystery crate hides the brass key. Open it, send the key out, and the sealed Purple gates unlock.</summary>
    public sealed class Level67 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(67, "Unbox the Key", 7, 7, 23, 5, LevelTier.Normal, "The brass key is crated! Pop the crate first, then unlock the Purple gate.");
            d.placements.Add(Scrap(1, 2, 3, 3));
            d.placements.Add(Scrap(1, 1, 5, 1));
            d.placements.Add(Crate(Key(Rect(B, 1, 1, 1, 5), 1)));
            d.placements.Add(Rect(G, 1, 1, 2, 5));
            d.placements.Add(Rect(R, 2, 2, 3, 5));
            d.placements.Add(Rect(Y, 2, 2, 1, 3));
            d.placements.Add(Rect(P, 2, 2, 4, 3));
            d.placements.Add(Rect(G, 2, 1, 1, 2));
            d.placements.Add(Rect(Y, 1, 2, 3, 0));
            d.placements.Add(Rect(P, 1, 2, 5, 5));
            d.placements.Add(Rect(R, 1, 2, 4, 1));
            d.placements.Add(Rect(B, 2, 1, 1, 1));
            d.gates.Add(Padlock(Gate(Up, 4, 2, P), 1));
            d.gates.Add(Padlock(Gate(Down, 4, 2, P), 1));
            d.gates.Add(Gate(Left, 4, 2, R));
            d.gates.Add(Gate(Right, 0, 2, R));
            d.gates.Add(Gate(Left, 0, 2, B));
            d.gates.Add(Gate(Down, 0, 2, B));
            d.gates.Add(Gate(Up, 0, 2, Y));
            d.gates.Add(Gate(Right, 4, 2, Y));
            d.gates.Add(Gate(Up, 2, 2, G));
            d.gates.Add(Gate(Left, 2, 2, G));
            d.parts.Add(Part(PartType.Hood, 0, Y, 3, 0));
            d.parts.Add(Part(PartType.Platen, 0, R, 3, 0));
            d.parts.Add(Part(PartType.Keys, 0, G, 2, 0));
            d.parts.Add(Part(PartType.Typebar, 0, P, 4, 1));
            d.parts.Add(Part(PartType.Ribbon, 0, Y, 2, 1));
            d.parts.Add(Part(PartType.Spring, 0, R, 2, 2));
            d.parts.Add(Part(PartType.Tnt, 0, P, 2, 2));
            ResolveHidden(d, false);
            return d;
        }
    }
}
