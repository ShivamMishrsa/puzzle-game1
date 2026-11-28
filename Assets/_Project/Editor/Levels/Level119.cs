using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Grand Finale Crucible (Music Box): polarity coils, ratchets, coupled screws, ice, crates, prisms, two portal pairs and a key-lock in one 11x11 maze - on three slots. The last test before the boss.</summary>
    public sealed class Level119 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(119, "Finale Crucible", 11, 11, 29, 3, LevelTier.Normal, "Everything at once on three slots - coils, ratchets, ice, crates, prism, portals, key!");
            d.turnstiles.Add(Turnstile(2, 4, Right));
            d.polarities.Add(Polarity(3, 7, true));
            d.polarities.Add(Polarity(7, 3, false));
            d.portals.Add(Pad(0, 1, 2, 9, 8));
            d.portals.Add(Pad(1, 9, 2, 2, 8));
            d.placements.Add(Scrap(1, 1, 5, 5));
            d.placements.Add(Rect(O, 2, 2, 1, 4));
            d.placements.Add(Rect(P, 2, 2, 4, 9));
            d.placements.Add(Rect(G, 2, 2, 0, 2));
            d.placements.Add(Rect(Y, 2, 2, 2, 2));
            d.placements.Add(Rect(C, 2, 2, 2, 6));
            d.placements.Add(Rect(B, 2, 2, 6, 6));
            d.placements.Add(Rect(P, 2, 1, 5, 1));
            d.placements.Add(Rect(G, 1, 2, 10, 7));
            d.placements.Add(Ice(Rect(R, 2, 1, 1, 8), 1));
            d.placements.Add(Rect(R, 2, 1, 1, 1));
            d.placements.Add(Rect(C, 1, 2, 7, 2));
            d.placements.Add(Rect(C, 1, 1, 6, 9));
            d.placements.Add(Rect(O, 1, 1, 4, 0));
            d.placements.Add(Prism(Rect(O, 1, 1, 9, 4), O, C, P, R));
            d.placements.Add(Key(Rect(Y, 1, 1, 8, 2), 0));
            d.placements.Add(Crate(Rect(B, 1, 1, 10, 1)));
            d.placements.Add(Rect(Y, 1, 1, 3, 10));
            d.placements.Add(Rect(B, 1, 1, 3, 0));
            d.placements.Add(Rect(R, 1, 1, 1, 9));
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
            d.parts.Add(Screws(Part(PartType.Lid, 0, O, 4, 0), R, B));
            d.parts.Add(Part(PartType.Lid, 1, C, 3, 0));
            d.parts.Add(Part(PartType.Comb, 0, Y, 4, 1));
            d.parts.Add(Part(PartType.Dancer, 0, G, 4, 1));
            d.parts.Add(Part(PartType.Drum, 0, P, 4, 2));
            d.parts.Add(Part(PartType.Drum, 1, B, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, R, 2, 3));
            d.parts.Add(Part(PartType.Tnt, 1, C, 2, 3));
            ResolveHidden(d, false);
            return d;
        }
    }
}
