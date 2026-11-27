using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Dual-Key Dominoes: key #1 unlocks gate #1, which frees key #2 to unlock gate #2; a rear TNT capacitor is exposed on the phone.</summary>
    public sealed class Level22 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(22, "Dual-Key Dominoes", 6, 6, 10, 5, LevelTier.Normal, "Key one opens gate one. Key two leaves through it and opens gate two!");
            d.placements.Add(Scrap(1, 1, 2, 3));
            d.placements.Add(Rect(R, 2, 1, 3, 5));
            d.placements.Add(Rect(G, 2, 1, 2, 4));
            d.placements.Add(Key(Rect(B, 1, 2, 1, 2), 1));
            d.placements.Add(Rect(Y, 1, 1, 2, 0));
            d.placements.Add(Key(Rect(Y, 1, 1, 4, 1), 0));
            d.placements.Add(Rect(B, 1, 1, 3, 2));
            d.gates.Add(Gate(Down, 0, 3, Y));
            d.gates.Add(Gate(Left, 3, 3, Y));
            d.gates.Add(Padlock(Gate(Right, 1, 2, B), 0));
            d.gates.Add(Gate(Up, 0, 1, B));
            d.gates.Add(Padlock(Gate(Up, 2, 2, G), 1));
            d.gates.Add(Gate(Right, 4, 2, R));
            d.gates.Add(Gate(Down, 3, 3, R));
            d.parts.Add(Part(PartType.Handset, 0, R, 2, 0));
            d.parts.Add(Part(PartType.Handset, 1, G, 2, 0));
            d.parts.Add(Part(PartType.Cradle, 0, Y, 2, 0));
            d.parts.Add(Part(PartType.Dial, 0, B, 2, 1));
            d.parts.Add(Part(PartType.Tnt, 0, B, 1, 1));
            ResolveHidden(d, false);
            Unhide(d, PartType.Tnt, 0);
            return d;
        }
    }
}
