using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Armour and Turnstile introduction (Gramophone): a Yellow outer horn shell hides the Blue inner bracket; two one-way ratchets stop you backtracking. Optimal + 1 move.</summary>
    public sealed class Level71 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(71, "Nested Armour", 7, 7, 20, 4, LevelTier.Normal, "Strip the OUTER shell first - the inner core is shielded. Ratchets only turn one way!");
            d.turnstiles.Add(Turnstile(2, 3, Right));
            d.turnstiles.Add(Turnstile(4, 4, Up));
            d.placements.Add(Scrap(1, 1, 3, 3));
            d.placements.Add(Rect(B, 2, 2, 5, 0));
            d.placements.Add(Rect(Y, 2, 2, 3, 0));
            d.placements.Add(Rect(R, 2, 2, 0, 1));
            d.placements.Add(Rect(Y, 1, 2, 3, 5));
            d.placements.Add(Rect(G, 1, 2, 5, 3));
            d.placements.Add(Rect(G, 2, 1, 0, 3));
            d.placements.Add(Rect(Y, 2, 1, 2, 4));
            d.placements.Add(Rect(B, 1, 2, 0, 5));
            d.placements.Add(Rect(R, 1, 2, 1, 5));
            d.placements.Add(Rect(B, 2, 1, 3, 2));
            d.placements.Add(Rect(G, 1, 1, 1, 4));
            d.placements.Add(Rect(R, 1, 1, 2, 3));
            d.placements.Add(Rect(Y, 1, 1, 2, 2));
            d.gates.Add(Gate(Up, 0, 2, R));
            d.gates.Add(Gate(Down, 0, 2, R));
            d.gates.Add(Gate(Left, 0, 2, B));
            d.gates.Add(Gate(Right, 0, 2, B));
            d.gates.Add(Gate(Down, 2, 2, Y));
            d.gates.Add(Gate(Up, 2, 2, Y));
            d.gates.Add(Gate(Right, 2, 2, G));
            d.gates.Add(Gate(Left, 2, 2, G));
            d.parts.Add(Part(PartType.Horn, 0, Y, 3, 0));
            d.parts.Add(Part(PartType.Bracket, 0, B, 3, 1));
            d.parts.Add(Part(PartType.Record, 0, G, 3, 1));
            d.parts.Add(Part(PartType.Governor, 0, R, 2, 2));
            d.parts.Add(Part(PartType.Cabinet, 0, Y, 3, 2));
            d.parts.Add(Part(PartType.Mainspring, 0, B, 3, 3));
            d.parts.Add(Part(PartType.Tnt, 0, G, 2, 3));
            ResolveHidden(d, false);
            return d;
        }
    }
}
