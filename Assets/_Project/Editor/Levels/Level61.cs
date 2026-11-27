using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Purple and Carriage-Shift introduction (Typewriter): a Purple 2x1 block sits on carriage row 0. Your first exit slides it one tile right, lining it up with the bottom Purple gate.</summary>
    public sealed class Level61 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(61, "Carriage Shift", 7, 7, 17, 5, LevelTier.Normal, "Brass carriage rows slide their blocks one tile after EVERY exit. Time it so the Purple block lines up with its gate!");
            d.carriages.Add(Carriage(0, 1));
            d.placements.Add(Scrap(1, 1, 3, 3));
            d.placements.Add(Scrap(1, 1, 5, 5));
            d.placements.Add(Rect(P, 2, 1, 2, 0));
            d.placements.Add(Rect(R, 2, 2, 0, 4));
            d.placements.Add(Rect(B, 2, 2, 1, 1));
            d.placements.Add(Rect(B, 1, 2, 2, 4));
            d.placements.Add(Rect(Y, 2, 1, 4, 2));
            d.placements.Add(Rect(Y, 1, 2, 4, 4));
            d.placements.Add(Rect(P, 1, 2, 6, 2));
            d.placements.Add(Rect(R, 1, 2, 3, 1));
            d.placements.Add(Rect(P, 1, 1, 1, 6));
            d.placements.Add(Rect(Y, 1, 1, 6, 0));
            d.gates.Add(Gate(Down, 3, 2, P));
            d.gates.Add(Gate(Right, 4, 2, P));
            d.gates.Add(Gate(Up, 0, 2, R));
            d.gates.Add(Gate(Left, 4, 2, R));
            d.gates.Add(Gate(Left, 0, 2, B));
            d.gates.Add(Gate(Up, 4, 2, B));
            d.gates.Add(Gate(Down, 0, 2, Y));
            d.gates.Add(Gate(Right, 1, 2, Y));
            d.parts.Add(Part(PartType.Hood, 0, B, 3, 0));
            d.parts.Add(Part(PartType.Platen, 0, R, 2, 0));
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
