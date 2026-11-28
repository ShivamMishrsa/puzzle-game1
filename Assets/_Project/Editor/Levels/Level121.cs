using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Organ Loft Gambit (Pipe Organ): EXTREME. Three polarity coils, two repulsors and a ratchet ring bend every route while coupled screws chain down the main pipe bank. Hints strongly recommended.</summary>
    public sealed class Level121 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(121, "Loft Gambit", 11, 11, 42, 3, LevelTier.Normal, "Extreme! Use the hint - coils, repulsors and a ratchet ring bend every route.");
            d.turnstiles.Add(Turnstile(2, 6, Up));
            d.turnstiles.Add(Turnstile(8, 7, Left));
            d.polarities.Add(Polarity(3, 4, true));
            d.polarities.Add(Polarity(7, 6, false));
            d.polarities.Add(Polarity(5, 8, true));
            d.repulsors.Add(Repulsor(5, 5));
            d.repulsors.Add(Repulsor(8, 3));
            d.placements.Add(Scrap(1, 1, 1, 1));
            d.placements.Add(Scrap(1, 1, 9, 9));
            d.placements.Add(Scrap(1, 2, 9, 3));
            d.placements.Add(Scrap(2, 1, 2, 9));
            d.placements.Add(Rect(P, 2, 2, 5, 1));
            d.placements.Add(Rect(G, 2, 2, 4, 7));
            d.placements.Add(Rect(G, 2, 2, 0, 2));
            d.placements.Add(Rect(O, 2, 2, 3, 2));
            d.placements.Add(Rect(C, 2, 2, 7, 0));
            d.placements.Add(Rect(C, 2, 2, 4, 9));
            d.placements.Add(Rect(O, 2, 2, 2, 5));
            d.placements.Add(Rect(B, 2, 2, 9, 7));
            d.placements.Add(Rect(R, 2, 2, 7, 4));
            d.placements.Add(Rect(Y, 2, 2, 4, 4));
            d.placements.Add(Rect(R, 2, 2, 7, 6));
            d.placements.Add(Rect(O, 2, 1, 0, 5));
            d.placements.Add(Rect(P, 1, 2, 6, 7));
            d.placements.Add(Rect(Y, 1, 2, 3, 7));
            d.placements.Add(Rect(G, 2, 1, 3, 0));
            d.placements.Add(Rect(B, 2, 1, 9, 2));
            d.placements.Add(Rect(C, 1, 1, 9, 1));
            d.placements.Add(Rect(B, 1, 1, 2, 10));
            d.placements.Add(Rect(Y, 1, 1, 3, 10));
            d.placements.Add(Rect(O, 1, 1, 0, 9));
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
            d.parts.Add(Part(PartType.Facade, 0, O, 5, 0));
            d.parts.Add(Part(PartType.Facade, 1, C, 4, 0));
            d.parts.Add(Screws(Part(PartType.Pipes, 0, G, 5, 1), R, B, Y));
            d.parts.Add(Part(PartType.Pipes, 1, P, 4, 1));
            d.parts.Add(Part(PartType.Keyboard, 0, Y, 4, 1));
            d.parts.Add(Part(PartType.Bellows, 0, R, 5, 2));
            d.parts.Add(Part(PartType.Stops, 0, B, 4, 2));
            d.parts.Add(Part(PartType.Stops, 1, O, 4, 2));
            d.parts.Add(Part(PartType.Tnt, 0, C, 3, 3));
            d.parts.Add(Part(PartType.Tnt, 1, G, 3, 3));
            ResolveHidden(d, false);
            d.solution = new int[] { 201917961, 67371530, 460558, 151322639, 134873109, 67436567, 151322647, 151126022, 67174421, 151257098, 134480141, 328964, 134219272, 83887624, 134350611, 151060501, 17433366, 134349063, 131858, 218237202, 83887108, 67306501, 134677776, 17368336, 134348807, 262151, 151126021, 151257095, 83886605, 134480140, 17367052, 83886865, 134676494, 134677771, 67110155, 17367054, 67110675, 218106131, 134285076, 83887892, 218106123, 83887625 };
            return d;
        }
    }
}
