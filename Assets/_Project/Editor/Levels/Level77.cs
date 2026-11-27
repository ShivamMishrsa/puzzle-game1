using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Ratchet and Portal Trap: a Key block must cross a one-way ratchet into the cyan portal - entering too early strands it on the wrong side.</summary>
    public sealed class Level77 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(77, "Ratchet Trap", 8, 8, 22, 4, LevelTier.Normal, "The key crosses the ratchet ONCE, then the portal. Do not rush it!");
            d.turnstiles.Add(Turnstile(3, 2, Right));
            d.turnstiles.Add(Turnstile(5, 5, Down));
            d.portals.Add(Pad(0, 2, 5, 5, 2));
            d.placements.Add(Scrap(1, 3, 3, 3));
            d.placements.Add(Scrap(1, 1, 6, 6));
            d.placements.Add(Rect(P, 2, 2, 4, 2));
            d.placements.Add(Rect(B, 2, 2, 1, 0));
            d.placements.Add(Rect(G, 2, 2, 2, 6));
            d.placements.Add(Rect(R, 2, 2, 0, 4));
            d.placements.Add(Rect(Y, 2, 2, 6, 1));
            d.placements.Add(Rect(P, 1, 2, 5, 5));
            d.placements.Add(Rect(G, 1, 2, 1, 2));
            d.placements.Add(Rect(G, 2, 1, 6, 3));
            d.placements.Add(Rect(R, 2, 1, 6, 4));
            d.placements.Add(Rect(R, 1, 2, 4, 4));
            d.placements.Add(Rect(B, 1, 2, 0, 0));
            d.placements.Add(Rect(Y, 1, 2, 3, 1));
            d.placements.Add(Rect(P, 2, 1, 4, 0));
            d.placements.Add(Key(Rect(B, 1, 1, 5, 1), 0));
            d.gates.Add(Gate(Up, 0, 2, R));
            d.gates.Add(Gate(Down, 0, 2, R));
            d.gates.Add(Gate(Left, 0, 2, B));
            d.gates.Add(Gate(Right, 0, 2, B));
            d.gates.Add(Gate(Down, 2, 2, Y));
            d.gates.Add(Gate(Up, 2, 2, Y));
            d.gates.Add(Gate(Right, 2, 2, G));
            d.gates.Add(Gate(Left, 2, 2, G));
            d.gates.Add(Padlock(Gate(Up, 4, 2, P), 0));
            d.gates.Add(Gate(Down, 4, 2, P));
            d.parts.Add(Part(PartType.Horn, 0, R, 3, 0));
            d.parts.Add(Part(PartType.Bracket, 0, Y, 3, 1));
            d.parts.Add(Part(PartType.Record, 0, P, 3, 1));
            d.parts.Add(Part(PartType.Governor, 0, G, 3, 2));
            d.parts.Add(Part(PartType.Cabinet, 0, B, 3, 2));
            d.parts.Add(Part(PartType.Mainspring, 0, R, 3, 3));
            d.parts.Add(Part(PartType.Tnt, 0, P, 2, 3));
            ResolveHidden(d, false);
            return d;
        }
    }
}
