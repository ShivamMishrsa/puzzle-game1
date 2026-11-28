using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Wormhole Toccata (Pipe Organ): EXTREME. Two portal pairs, two welded pairs, a clockwork-piercing gate and two shutter gates with a polarity coil and a repulsor choosing who gets pulled through.</summary>
    public sealed class Level123 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(123, "Wormhole Toccata", 11, 11, 44, 3, LevelTier.Normal, "Extreme! Two wormholes, welded pairs, a piercing gate and shutters.");
            d.polarities.Add(Polarity(5, 6, true));
            d.repulsors.Add(Repulsor(4, 4));
            d.portals.Add(Pad(0, 1, 2, 9, 8));
            d.portals.Add(Pad(1, 9, 2, 2, 8));
            d.placements.Add(Scrap(1, 1, 5, 3));
            d.placements.Add(Scrap(1, 2, 3, 5));
            d.placements.Add(Scrap(2, 1, 7, 7));
            d.placements.Add(Rect(B, 2, 2, 2, 1));
            d.placements.Add(Rect(R, 2, 2, 0, 1));
            d.placements.Add(Rect(P, 2, 2, 1, 7));
            d.placements.Add(Rect(G, 2, 2, 9, 3));
            d.placements.Add(Rect(Y, 2, 2, 9, 7));
            d.placements.Add(Rect(O, 2, 2, 5, 9));
            d.placements.Add(Rect(C, 2, 2, 7, 2));
            d.placements.Add(Rect(C, 2, 2, 6, 0));
            d.placements.Add(Rect(R, 2, 2, 4, 7));
            d.placements.Add(Rect(O, 2, 2, 1, 5));
            d.placements.Add(Rect(P, 2, 1, 4, 0));
            d.placements.Add(Rect(B, 2, 1, 9, 1));
            d.placements.Add(Rect(Y, 1, 2, 6, 2));
            d.placements.Add(Rect(C, 1, 2, 8, 0));
            d.placements.Add(Link(Rect(Y, 1, 1, 3, 9), 2));
            d.placements.Add(Link(Rect(C, 1, 1, 10, 6), 1));
            d.placements.Add(Link(Rect(O, 1, 1, 8, 6), 1));
            d.placements.Add(Rect(R, 1, 1, 5, 2));
            d.placements.Add(Rect(G, 1, 1, 0, 3));
            d.placements.Add(Link(Rect(G, 1, 1, 3, 7), 2));
            d.placements.Add(Rect(P, 1, 1, 2, 0));
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
            d.gates.Add(Piercing(Gate(Left, 4, 2, O)));
            d.gates.Add(Gate(Down, 6, 2, C));
            d.parts.Add(Part(PartType.Facade, 0, C, 5, 0));
            d.parts.Add(Part(PartType.Facade, 1, O, 4, 0));
            d.parts.Add(Part(PartType.Pipes, 0, Y, 5, 1));
            d.parts.Add(Part(PartType.Pipes, 1, G, 4, 1));
            d.parts.Add(Part(PartType.Keyboard, 0, R, 4, 1));
            d.parts.Add(Part(PartType.Bellows, 0, P, 5, 2));
            d.parts.Add(Part(PartType.Stops, 0, B, 4, 2));
            d.parts.Add(Part(PartType.Stops, 1, C, 4, 2));
            d.parts.Add(Part(PartType.Tnt, 0, R, 3, 3));
            d.parts.Add(Part(PartType.Tnt, 1, O, 3, 3));
            ResolveHidden(d, false);
            d.solution = new int[] { 67242246, 526599, 83887626, 67371276, 184811532, 460303, 134350345, 134219536, 656145, 67373586, 134481682, 83886084, 83887888, 218237190, 83887625, 151191573, 83887117, 201327639, 67109379, 134349332, 83887127, 17433361, 150994947, 151060494, 134807574, 134807816, 67305494, 151191574, 67111175, 134218247, 83886599, 590859, 201786373, 67371272, 184811528, 134807563, 17368069, 17367051, 134677263, 17367823, 17432852, 184811539, 201590034, 83887634 };
            return d;
        }
    }
}
