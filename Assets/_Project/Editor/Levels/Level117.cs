using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Exposed Mechanism (Music Box): the glass dome and collar are already gone - the comb, dancer, discs and twin dynamite stand in plain view, with coupled screws, ratchets and three polarity coils - on three slots.</summary>
    public sealed class Level117 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(117, "Exposed Mechanism", 11, 11, 40, 3, LevelTier.Normal, "No dome - shoot the comb and dancer, mind the coupled bolts!");
            d.turnstiles.Add(Turnstile(2, 6, Up));
            d.turnstiles.Add(Turnstile(6, 3, Right));
            d.polarities.Add(Polarity(3, 3, true));
            d.polarities.Add(Polarity(7, 7, false));
            d.polarities.Add(Polarity(4, 8, true));
            d.placements.Add(Scrap(1, 1, 5, 5));
            d.placements.Add(Scrap(1, 2, 1, 8));
            d.placements.Add(Scrap(2, 1, 8, 2));
            d.placements.Add(Rect(G, 2, 2, 7, 3));
            d.placements.Add(Rect(Y, 2, 2, 2, 0));
            d.placements.Add(Rect(P, 2, 2, 6, 5));
            d.placements.Add(Rect(C, 2, 2, 6, 1));
            d.placements.Add(Rect(O, 2, 2, 0, 5));
            d.placements.Add(Rect(B, 2, 2, 6, 9));
            d.placements.Add(Rect(R, 2, 2, 1, 2));
            d.placements.Add(Rect(P, 2, 1, 0, 0));
            d.placements.Add(Rect(R, 2, 1, 8, 0));
            d.placements.Add(Rect(O, 1, 2, 10, 1));
            d.placements.Add(Rect(Y, 2, 1, 6, 7));
            d.placements.Add(Rect(B, 1, 2, 9, 5));
            d.placements.Add(Rect(C, 2, 1, 4, 2));
            d.placements.Add(Rect(G, 1, 2, 9, 7));
            d.placements.Add(Rect(R, 1, 1, 1, 1));
            d.gates.Add(Gate(Up, 0, 2, R));
            d.gates.Add(Gate(Down, 0, 2, R));
            d.gates.Add(Gate(Left, 0, 2, B));
            d.gates.Add(Gate(Right, 0, 2, B));
            d.gates.Add(Gate(Down, 2, 2, Y));
            d.gates.Add(Gate(Up, 2, 2, Y));
            d.gates.Add(Gate(Right, 2, 2, G));
            d.gates.Add(Gate(Left, 2, 2, G));
            d.gates.Add(Gate(Up, 4, 2, P));
            d.gates.Add(Gate(Down, 4, 2, P));
            d.gates.Add(Gate(Left, 4, 2, O));
            d.gates.Add(Gate(Down, 6, 2, C));
            d.parts.Add(Screws(Part(PartType.Comb, 0, C, 3, 0), R, B));
            d.parts.Add(Screws(Part(PartType.Dancer, 0, O, 3, 0), G, P));
            d.parts.Add(Part(PartType.Drum, 0, Y, 3, 1));
            d.parts.Add(Part(PartType.Drum, 1, R, 3, 1));
            d.parts.Add(Part(PartType.Tnt, 0, G, 2, 2));
            d.parts.Add(Part(PartType.Tnt, 1, P, 2, 2));
            d.parts.Add(Part(PartType.Crank, 0, B, 2, 0));
            ResolveHidden(d, false);
            return d;
        }
    }
}
