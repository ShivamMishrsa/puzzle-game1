using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Pre-Boss Mahogany Vault: 2-layer ice, a welded pair, one-way turnstiles and shutter gates in a dense scrap-iron maze.</summary>
    public sealed class Level79 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(79, "Mahogany Vault", 8, 8, 24, 4, LevelTier.Normal, "Ice, a weld, ratchets and shutters in an iron maze - think five moves ahead!");
            d.turnstiles.Add(Turnstile(2, 4, Right));
            d.turnstiles.Add(Turnstile(5, 3, Up));
            d.placements.Add(Scrap(2, 1, 3, 3));
            d.placements.Add(Scrap(1, 2, 5, 4));
            d.placements.Add(Scrap(1, 1, 2, 6));
            d.placements.Add(Scrap(1, 1, 6, 1));
            d.placements.Add(Rect(R, 2, 2, 0, 6));
            d.placements.Add(Rect(G, 2, 2, 3, 4));
            d.placements.Add(Rect(Y, 2, 2, 3, 1));
            d.placements.Add(Rect(B, 2, 2, 0, 0));
            d.placements.Add(Rect(P, 2, 2, 5, 6));
            d.placements.Add(Rect(P, 1, 2, 5, 1));
            d.placements.Add(Rect(Y, 1, 2, 2, 3));
            d.placements.Add(Rect(G, 1, 2, 6, 2));
            d.placements.Add(Rect(R, 2, 1, 4, 0));
            d.placements.Add(Rect(R, 1, 2, 1, 4));
            d.placements.Add(Rect(B, 1, 2, 2, 1));
            d.placements.Add(Ice(Rect(R, 2, 1, 0, 2), 2));
            d.placements.Add(Link(Rect(B, 1, 1, 3, 7), 1));
            d.placements.Add(Link(Rect(P, 1, 1, 2, 7), 1));
            d.gates.Add(Gate(Up, 0, 2, R));
            d.gates.Add(Gate(Down, 0, 2, R));
            d.gates.Add(Gate(Left, 0, 2, B));
            d.gates.Add(Gate(Right, 0, 2, B));
            d.gates.Add(Gate(Down, 2, 2, Y));
            d.gates.Add(Shutter(Up, 2, 2, Y, false));
            d.gates.Add(Shutter(Right, 2, 2, G, true));
            d.gates.Add(Gate(Left, 2, 2, G));
            d.gates.Add(Gate(Up, 4, 2, P));
            d.gates.Add(Gate(Down, 4, 2, P));
            d.parts.Add(Part(PartType.Horn, 0, Y, 3, 0));
            d.parts.Add(Part(PartType.Horn, 1, G, 3, 0));
            d.parts.Add(Part(PartType.Bracket, 0, B, 5, 1));
            d.parts.Add(Part(PartType.Record, 0, P, 5, 1));
            d.parts.Add(Part(PartType.Governor, 0, R, 6, 2));
            d.parts.Add(Part(PartType.Cabinet, 0, R, 2, 2));
            d.parts.Add(Part(PartType.Mainspring, 0, Y, 3, 3));
            d.parts.Add(Part(PartType.Tnt, 0, B, 1, 3));
            ResolveHidden(d, false);
            return d;
        }
    }
}
