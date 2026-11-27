using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Clockwork Rebound Cascade: bounce one Orange block off a repulsor into a carriage row to set off a Colour-Chain Overdrive and a clockwork-piercing Mega-Turret.</summary>
    public sealed class Level86 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(86, "Rebound Cascade", 8, 8, 19, 4, LevelTier.Normal, "One rebound sets the carriage sliding - then chain the colours!");
            d.carriages.Add(Carriage(3, 1));
            d.repulsors.Add(Repulsor(2, 3));
            d.placements.Add(Scrap(1, 1, 5, 5));
            d.placements.Add(Rect(P, 2, 2, 3, 3));
            d.placements.Add(Rect(O, 2, 2, 0, 4));
            d.placements.Add(Rect(C, 2, 2, 6, 2));
            d.placements.Add(Rect(G, 2, 2, 4, 1));
            d.placements.Add(Rect(R, 2, 2, 1, 1));
            d.placements.Add(Rect(B, 2, 2, 1, 6));
            d.placements.Add(Rect(Y, 2, 2, 6, 6));
            d.placements.Add(Rect(C, 1, 2, 7, 0));
            d.placements.Add(Rect(O, 2, 1, 2, 5));
            d.placements.Add(Rect(C, 1, 1, 0, 7));
            d.placements.Add(Rect(O, 1, 1, 0, 1));
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
            d.gates.Add(Piercing(Gate(Left, 4, 2, O)));
            d.gates.Add(Gate(Down, 6, 2, C));
            d.parts.Add(Part(PartType.Ring, 0, O, 3, 0));
            d.parts.Add(Part(PartType.Ring, 1, C, 3, 0));
            d.parts.Add(Part(PartType.Ring, 2, Y, 3, 0));
            d.parts.Add(Part(PartType.Ring, 3, G, 3, 0));
            d.parts.Add(Part(PartType.Planet, 0, C, 3, 1));
            d.parts.Add(Part(PartType.Planet, 1, P, 3, 1));
            d.parts.Add(Part(PartType.Planet, 2, R, 3, 1));
            d.parts.Add(Part(PartType.Cage, 0, O, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, B, 2, 3));
            ResolveHidden(d, false);
            return d;
        }
    }
}
