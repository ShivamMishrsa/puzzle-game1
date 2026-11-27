using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Frozen Key & Peeled Core: a peeled yellow block hides a brass key in its inner core, right beside an iced blue bar.</summary>
    public sealed class Level27 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(27, "Frozen Key", 7, 7, 12, 5, LevelTier.Normal, "Peel the yellow block: its blue core is the key - and it thaws the ice beside it.");
            d.placements.Add(Scrap(1, 1, 3, 4));
            d.placements.Add(PeeledKey(Rect(Y, 2, 1, 1, 1), B, 0));
            d.placements.Add(Ice(Rect(B, 1, 2, 3, 1), 1));
            d.placements.Add(Rect(G, 2, 2, 5, 1));
            d.placements.Add(Rect(R, 2, 1, 5, 4));
            d.placements.Add(Rect(Y, 1, 1, 2, 2));
            d.placements.Add(Rect(G, 1, 1, 4, 6));
            d.gates.Add(Gate(Down, 1, 2, Y));
            d.gates.Add(Gate(Left, 0, 3, Y));
            d.gates.Add(Gate(Up, 3, 1, B));
            d.gates.Add(Gate(Left, 3, 3, B));
            d.gates.Add(Gate(Right, 0, 3, R));
            d.gates.Add(Gate(Up, 5, 2, R));
            d.gates.Add(Padlock(Gate(Down, 4, 3, G), 0));
            d.gates.Add(Padlock(Gate(Right, 4, 3, G), 0));
            d.parts.Add(Part(PartType.Handset, 0, R, 2, 0));
            d.parts.Add(Part(PartType.Handset, 1, G, 4, 0));
            d.parts.Add(Part(PartType.Cradle, 0, Y, 2, 0));
            d.parts.Add(Part(PartType.Dial, 0, B, 3, 1));
            ResolveHidden(d, false);
            return d;
        }
    }
}
