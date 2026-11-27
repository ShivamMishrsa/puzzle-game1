using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Prism Shift Introduction (TLR Camera): a Blue->Red prism block sits in front of the bottom red gate. Sending the Yellow starter out shifts it to Red so it can leave at once.</summary>
    public sealed class Level51 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(51, "Prism Shift", 6, 6, 10, 5, LevelTier.Normal, "The glass prism changes colour every time ANY block leaves. Wait for the right colour!");
            d.placements.Add(Prism(Rect(B, 1, 1, 2, 1), B, R));
            d.placements.Add(Rect(R, 2, 2, 1, 3));
            d.placements.Add(Rect(B, 1, 2, 4, 1));
            d.placements.Add(Rect(Y, 1, 2, 3, 0));
            d.placements.Add(Rect(Y, 1, 1, 2, 2));
            d.placements.Add(Rect(R, 1, 1, 4, 0));
            d.gates.Add(Gate(Down, 2, 1, R));
            d.gates.Add(Gate(Up, 0, 2, R));
            d.gates.Add(Gate(Left, 3, 2, B));
            d.gates.Add(Gate(Right, 0, 2, B));
            d.gates.Add(Gate(Right, 3, 2, Y));
            d.gates.Add(Gate(Up, 3, 2, Y));
            d.parts.Add(Part(PartType.Body, 0, Y, 2, 0));
            d.parts.Add(Part(PartType.Hood, 0, B, 1, 0));
            d.parts.Add(Part(PartType.Lens, 0, R, 2, 0));
            d.parts.Add(Part(PartType.Iris, 0, R, 2, 1));
            d.parts.Add(Part(PartType.Crank, 0, Y, 1, 1));
            d.parts.Add(Part(PartType.ShutterGear, 0, B, 1, 2));
            d.parts.Add(Part(PartType.Tnt, 0, R, 2, 2));
            ResolveHidden(d, false);
            return d;
        }
    }
}
