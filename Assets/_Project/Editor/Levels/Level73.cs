using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>One-Way Labyrinth (Gramophone): four ratchet turnstiles form a one-way loop around a central scrap-iron block; the wrong branch is a dead end.</summary>
    public sealed class Level73 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(73, "One-Way Labyrinth", 7, 7, 21, 4, LevelTier.Normal, "Four one-way ratchets circle the scrap iron. Plan the whole route before you slide!");
            d.turnstiles.Add(Turnstile(2, 2, Right));
            d.turnstiles.Add(Turnstile(4, 2, Up));
            d.turnstiles.Add(Turnstile(4, 4, Left));
            d.turnstiles.Add(Turnstile(2, 4, Down));
            d.placements.Add(Scrap(2, 2, 3, 3));
            d.placements.Add(Rect(B, 2, 2, 0, 0));
            d.placements.Add(Rect(Y, 2, 2, 5, 1));
            d.placements.Add(Rect(P, 2, 2, 0, 2));
            d.placements.Add(Rect(R, 2, 2, 0, 4));
            d.placements.Add(Rect(G, 2, 1, 5, 3));
            d.placements.Add(Rect(P, 1, 2, 5, 5));
            d.placements.Add(Rect(G, 1, 2, 2, 2));
            d.placements.Add(Rect(R, 1, 2, 4, 0));
            d.placements.Add(Rect(R, 2, 1, 2, 0));
            d.placements.Add(Rect(B, 1, 2, 3, 1));
            d.placements.Add(Rect(Y, 1, 2, 3, 5));
            d.placements.Add(Rect(P, 1, 1, 5, 4));
            d.placements.Add(Rect(Y, 1, 1, 6, 6));
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
            d.parts.Add(Part(PartType.Horn, 0, Y, 3, 0));
            d.parts.Add(Part(PartType.Bracket, 0, B, 3, 1));
            d.parts.Add(Part(PartType.Record, 0, P, 3, 1));
            d.parts.Add(Part(PartType.Governor, 0, G, 3, 2));
            d.parts.Add(Part(PartType.Cabinet, 0, R, 3, 2));
            d.parts.Add(Part(PartType.Mainspring, 0, B, 3, 3));
            d.parts.Add(Part(PartType.Tnt, 0, P, 2, 3));
            ResolveHidden(d, false);
            return d;
        }
    }
}
