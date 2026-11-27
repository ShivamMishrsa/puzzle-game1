using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>All-Mechanics Crucible: polarity coils, ratchets, coupled screws, ice, crates, a prism, a portal and a key-lock inside a tight scrap-iron maze.</summary>
    public sealed class Level99 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(99, "The Crucible", 9, 9, 29, 4, LevelTier.Normal, "Everything at once - coils, ratchets, ice, crates, prism, portal, key!");
            d.turnstiles.Add(Turnstile(2, 4, Right));
            d.polarities.Add(Polarity(3, 5, true));
            d.polarities.Add(Polarity(5, 3, false));
            d.portals.Add(Pad(0, 1, 7, 7, 1));
            d.placements.Add(Scrap(1, 1, 4, 4));
            d.placements.Add(Rect(C, 2, 2, 4, 2));
            d.placements.Add(Rect(G, 1, 2, 8, 2));
            d.placements.Add(Rect(Y, 2, 1, 3, 0));
            d.placements.Add(Rect(O, 1, 2, 1, 2));
            d.placements.Add(Rect(P, 2, 1, 5, 8));
            d.placements.Add(Rect(B, 1, 2, 6, 1));
            d.placements.Add(Rect(R, 2, 1, 7, 5));
            d.placements.Add(Ice(Rect(R, 2, 1, 0, 1), 1));
            d.placements.Add(Rect(P, 1, 1, 5, 6));
            d.placements.Add(Crate(Rect(B, 1, 1, 2, 1)));
            d.placements.Add(Rect(C, 1, 1, 5, 1));
            d.placements.Add(Prism(Rect(O, 1, 1, 1, 5), O, C, P, R));
            d.placements.Add(Rect(B, 1, 1, 3, 2));
            d.placements.Add(Key(Rect(Y, 1, 1, 4, 6), 0));
            d.placements.Add(Rect(G, 1, 1, 6, 3));
            d.gates.Add(Gate(Up, 0, 2, R));
            d.gates.Add(Gate(Down, 0, 2, R));
            d.gates.Add(Gate(Left, 0, 2, B));
            d.gates.Add(Gate(Right, 0, 2, B));
            d.gates.Add(Gate(Down, 2, 2, Y));
            d.gates.Add(Gate(Up, 2, 2, Y));
            d.gates.Add(Padlock(Gate(Right, 2, 2, G), 0));
            d.gates.Add(Gate(Left, 2, 2, G));
            d.gates.Add(Gate(Up, 4, 2, P));
            d.gates.Add(Gate(Down, 4, 2, P));
            d.gates.Add(Gate(Left, 4, 2, O));
            d.gates.Add(Gate(Down, 6, 2, C));
            d.parts.Add(Screws(Part(PartType.Bezel, 0, O, 3, 0), R, B));
            d.parts.Add(Part(PartType.Crystal, 0, C, 3, 0));
            d.parts.Add(Part(PartType.Bridge, 0, Y, 3, 1));
            d.parts.Add(Part(PartType.Bridge, 1, G, 3, 1));
            d.parts.Add(Part(PartType.TourbillonCage, 0, P, 3, 1));
            d.parts.Add(Part(PartType.MainBarrel, 0, B, 3, 2));
            d.parts.Add(Part(PartType.Rotor, 0, R, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, C, 2, 3));
            ResolveHidden(d, false);
            return d;
        }
    }
}
