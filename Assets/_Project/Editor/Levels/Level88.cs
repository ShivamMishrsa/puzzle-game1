using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Exposed Planetary Orrery: the zodiac hoops are already stripped off - 3-layer orbiting planets, four brass screws and twin dynamite bundles in plain view.</summary>
    public sealed class Level88 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(88, "Exposed Orrery", 9, 9, 26, 4, LevelTier.Normal, "No hoops - shoot the orbiting planets. Four bolts, twin dynamite!");
            d.turnstiles.Add(Turnstile(2, 5, Up));
            d.turnstiles.Add(Turnstile(5, 3, Right));
            d.repulsors.Add(Repulsor(3, 3));
            d.repulsors.Add(Repulsor(6, 5));
            d.placements.Add(Scrap(1, 1, 4, 4));
            d.placements.Add(Scrap(1, 2, 1, 6));
            d.placements.Add(Scrap(2, 1, 6, 2));
            d.placements.Add(Rect(P, 2, 2, 4, 7));
            d.placements.Add(Rect(G, 2, 2, 2, 2));
            d.placements.Add(Rect(C, 2, 2, 2, 0));
            d.placements.Add(Rect(O, 2, 2, 1, 4));
            d.placements.Add(Rect(Y, 2, 2, 6, 5));
            d.placements.Add(Rect(B, 2, 2, 6, 0));
            d.placements.Add(Rect(R, 2, 2, 0, 2));
            d.placements.Add(Rect(R, 2, 1, 2, 7));
            d.placements.Add(Rect(O, 1, 2, 0, 5));
            d.placements.Add(Rect(B, 1, 2, 0, 0));
            d.placements.Add(Rect(P, 2, 1, 6, 7));
            d.placements.Add(Rect(C, 2, 1, 4, 5));
            d.placements.Add(Rect(P, 1, 1, 5, 2));
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
            d.parts.Add(Screw(Part(PartType.Planet, 0, C, 5, 0), R));
            d.parts.Add(Screw(Part(PartType.Planet, 1, O, 5, 0), B));
            d.parts.Add(Screw(Part(PartType.Planet, 2, B, 4, 1), G));
            d.parts.Add(Screw(Part(PartType.Planet, 3, R, 4, 1), P));
            d.parts.Add(Part(PartType.Cage, 0, P, 5, 2));
            d.parts.Add(Part(PartType.Tnt, 0, G, 2, 3));
            d.parts.Add(Part(PartType.Tnt, 1, Y, 2, 3));
            ResolveHidden(d, false);
            return d;
        }
    }
}
