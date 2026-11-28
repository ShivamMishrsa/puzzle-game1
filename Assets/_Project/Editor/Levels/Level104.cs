using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Portal Pass (Clock Tower): a portal pair, a clockwork-piercing gate and a shutter gate. One polarity coil decides which block gets pulled through the wormhole.</summary>
    public sealed class Level104 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(104, "Portal Pass", 10, 10, 24, 4, LevelTier.Normal, "Portal in, pierce the gate, mind the shutter - and the coil!");
            d.polarities.Add(Polarity(4, 5, true));
            d.portals.Add(Pad(0, 1, 8, 8, 1));
            d.placements.Add(Scrap(1, 1, 5, 3));
            d.placements.Add(Scrap(1, 2, 2, 4));
            d.placements.Add(Scrap(2, 1, 6, 7));
            d.placements.Add(Rect(P, 2, 2, 5, 1));
            d.placements.Add(Rect(G, 2, 2, 0, 6));
            d.placements.Add(Rect(O, 2, 2, 0, 8));
            d.placements.Add(Rect(C, 2, 2, 7, 2));
            d.placements.Add(Rect(C, 2, 2, 4, 5));
            d.placements.Add(Rect(B, 2, 2, 1, 0));
            d.placements.Add(Rect(Y, 2, 2, 2, 8));
            d.placements.Add(Rect(R, 2, 2, 0, 2));
            d.placements.Add(Rect(R, 2, 1, 7, 4));
            d.placements.Add(Rect(O, 1, 2, 1, 4));
            d.placements.Add(Rect(Y, 1, 1, 3, 7));
            d.placements.Add(Rect(O, 1, 1, 6, 4));
            d.placements.Add(Rect(G, 1, 1, 2, 3));
            d.placements.Add(Rect(R, 1, 1, 2, 6));
            d.gates.Add(Gate(Up, 0, 2, R));
            d.gates.Add(Gate(Down, 0, 2, R));
            d.gates.Add(Gate(Left, 0, 2, B));
            d.gates.Add(Gate(Right, 0, 2, B));
            d.gates.Add(Gate(Down, 2, 2, Y));
            d.gates.Add(Shutter(Up, 2, 2, Y, false));
            d.gates.Add(Gate(Right, 2, 2, G));
            d.gates.Add(Gate(Left, 2, 2, G));
            d.gates.Add(Gate(Up, 4, 2, P));
            d.gates.Add(Gate(Down, 4, 2, P));
            d.gates.Add(Piercing(Gate(Left, 4, 2, O)));
            d.gates.Add(Gate(Down, 6, 2, C));
            d.parts.Add(Part(PartType.Hood, 0, C, 4, 0));
            d.parts.Add(Part(PartType.Hood, 1, O, 3, 0));
            d.parts.Add(Part(PartType.ClockDoor, 0, R, 4, 0));
            d.parts.Add(Part(PartType.Dial, 0, Y, 4, 1));
            d.parts.Add(Part(PartType.Pendulum, 0, G, 4, 1));
            d.parts.Add(Part(PartType.Weight, 0, P, 3, 2));
            d.parts.Add(Part(PartType.Weight, 1, B, 3, 2));
            d.parts.Add(Part(PartType.Chime, 0, C, 3, 2));
            d.parts.Add(Part(PartType.Chime, 1, O, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, R, 2, 3));
            ResolveHidden(d, false);
            return d;
        }
    }
}
