using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Grand Crucible (Clock Tower): polarity coils, ratchets, coupled screws, ice, crates, prisms, a portal and a key-lock all in one 10x10 maze - the last test before the boss.</summary>
    public sealed class Level109 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(109, "Grand Crucible", 10, 10, 30, 4, LevelTier.Normal, "Everything at once - coils, ratchets, ice, crates, prism, portal, key!");
            d.turnstiles.Add(Turnstile(2, 4, Right));
            d.polarities.Add(Polarity(3, 6, true));
            d.polarities.Add(Polarity(6, 3, false));
            d.portals.Add(Pad(0, 1, 8, 8, 1));
            d.placements.Add(Scrap(1, 1, 4, 4));
            d.placements.Add(Rect(P, 2, 2, 5, 2));
            d.placements.Add(Rect(G, 2, 2, 2, 8));
            d.placements.Add(Rect(B, 2, 2, 0, 1));
            d.placements.Add(Rect(O, 2, 2, 7, 7));
            d.placements.Add(Rect(Y, 2, 2, 2, 3));
            d.placements.Add(Rect(C, 2, 2, 7, 1));
            d.placements.Add(Rect(C, 1, 2, 9, 6));
            d.placements.Add(Ice(Rect(R, 2, 1, 0, 9), 1));
            d.placements.Add(Rect(R, 2, 1, 5, 7));
            d.placements.Add(Key(Rect(Y, 1, 1, 3, 2), 0));
            d.placements.Add(Rect(C, 1, 1, 7, 0));
            d.placements.Add(Crate(Rect(B, 1, 1, 0, 5)));
            d.placements.Add(Rect(R, 1, 1, 6, 5));
            d.placements.Add(Rect(G, 1, 1, 4, 3));
            d.placements.Add(Prism(Rect(O, 1, 1, 5, 6), O, C, P, R));
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
            d.parts.Add(Screws(Part(PartType.Hood, 0, O, 4, 0), R, B));
            d.parts.Add(Part(PartType.ClockDoor, 0, C, 4, 0));
            d.parts.Add(Part(PartType.Dial, 0, Y, 4, 1));
            d.parts.Add(Part(PartType.Pendulum, 0, G, 4, 1));
            d.parts.Add(Part(PartType.Weight, 0, P, 3, 2));
            d.parts.Add(Part(PartType.Weight, 1, B, 3, 2));
            d.parts.Add(Part(PartType.Chime, 0, R, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, C, 2, 3));
            ResolveHidden(d, false);
            d.solution = new int[] { 201852162, 67108867, 134676489, 83887883, 134545677, 150994947, 67174412, 83886349, 17302021, 17367818, 151191566, 67175681, 134218753, 134284806, 67241732, 134348804, 262148, 151257092, 83887622, 83887105, 134612743, 83887879, 83887375, 67175682, 151126018, 201851404, 524297, 83886089, 17367048, 151060492 };
            return d;
        }
    }
}
