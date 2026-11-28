using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Locksmith's Organ (Pipe Organ): EXTREME. Two TWO-slot belts of keys: ice, crates, two key-lock gates, repulsors and one-way ratchets inside an iron labyrinth. One misstep and the belt jams.</summary>
    public sealed class Level127 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(127, "Locksmith's Organ", 11, 11, 44, 2, LevelTier.Normal, "Two slots! Two locks, ice, crates and ratchets - use the hint.");
            d.turnstiles.Add(Turnstile(2, 4, Right));
            d.turnstiles.Add(Turnstile(8, 6, Left));
            d.polarities.Add(Polarity(3, 6, true));
            d.repulsors.Add(Repulsor(6, 4));
            d.repulsors.Add(Repulsor(4, 8));
            d.placements.Add(Scrap(1, 1, 4, 4));
            d.placements.Add(Scrap(1, 1, 8, 8));
            d.placements.Add(Rect(O, 2, 2, 0, 4));
            d.placements.Add(Rect(B, 2, 2, 9, 0));
            d.placements.Add(Rect(Y, 2, 2, 7, 6));
            d.placements.Add(Rect(G, 2, 2, 7, 4));
            d.placements.Add(Rect(P, 2, 2, 5, 1));
            d.placements.Add(Rect(O, 2, 2, 1, 2));
            d.placements.Add(Rect(C, 2, 2, 7, 1));
            d.placements.Add(Rect(G, 2, 2, 0, 7));
            d.placements.Add(Rect(C, 2, 2, 4, 9));
            d.placements.Add(Rect(R, 2, 2, 2, 4));
            d.placements.Add(Rect(P, 1, 2, 9, 2));
            d.placements.Add(Rect(O, 2, 1, 1, 9));
            d.placements.Add(Rect(G, 2, 1, 5, 7));
            d.placements.Add(Ice(Rect(R, 2, 1, 7, 9), 1));
            d.placements.Add(Key(Rect(Y, 1, 1, 3, 9), 0));
            d.placements.Add(Crate(Rect(B, 1, 1, 1, 0)));
            d.placements.Add(Rect(C, 1, 1, 3, 6));
            d.placements.Add(Key(Rect(O, 1, 1, 6, 4), 1));
            d.placements.Add(Rect(Y, 1, 1, 3, 8));
            d.placements.Add(Rect(B, 1, 1, 0, 1));
            d.placements.Add(Rect(R, 1, 1, 4, 2));
            d.gates.Add(Gate(Up, 0, 2, R));
            d.gates.Add(Gate(Down, 0, 2, R));
            d.gates.Add(Gate(Left, 0, 2, B));
            d.gates.Add(Gate(Right, 0, 2, B));
            d.gates.Add(Gate(Down, 2, 2, Y));
            d.gates.Add(Gate(Up, 2, 2, Y));
            d.gates.Add(Padlock(Gate(Right, 2, 2, G), 0));
            d.gates.Add(Gate(Left, 2, 2, G));
            d.gates.Add(Padlock(Gate(Up, 4, 2, P), 1));
            d.gates.Add(Gate(Down, 4, 2, P));
            d.gates.Add(Gate(Left, 4, 2, O));
            d.gates.Add(Gate(Down, 6, 2, C));
            d.parts.Add(Part(PartType.Facade, 0, O, 5, 0));
            d.parts.Add(Part(PartType.Facade, 1, C, 4, 0));
            d.parts.Add(Part(PartType.Pipes, 0, G, 5, 1));
            d.parts.Add(Part(PartType.Pipes, 1, P, 4, 1));
            d.parts.Add(Part(PartType.Keyboard, 0, Y, 4, 1));
            d.parts.Add(Part(PartType.Bellows, 0, R, 5, 2));
            d.parts.Add(Part(PartType.Stops, 0, B, 4, 2));
            d.parts.Add(Part(PartType.Stops, 1, O, 4, 2));
            d.parts.Add(Part(PartType.Tnt, 0, C, 3, 3));
            d.parts.Add(Part(PartType.Tnt, 1, G, 3, 3));
            ResolveHidden(d, false);
            d.solution = new int[] { 151257090, 134348807, 262151, 201721362, 329235, 134349078, 134284294, 134284808, 83887624, 67241733, 83887110, 151257095, 134479883, 17433360, 17433364, 134349573, 134350092, 83887372, 218237189, 67241732, 134349316, 83886596, 201787662, 67307790, 151191566, 218106115, 151060501, 67305483, 151322643, 201786633, 67436813, 151322637, 17367051, 17432854, 83887634, 67307017, 151126025, 67634186, 201786634, 83887626, 134807567, 67174415, 17432591, 150994961 };
            return d;
        }
    }
}
