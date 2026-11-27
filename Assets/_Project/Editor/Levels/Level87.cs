using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Twin-Linked Repulsor Maze: two welded pairs must slip between repulsors without either half being shoved off line before they reach the shutter gate.</summary>
    public sealed class Level87 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(87, "Linked Maze", 9, 9, 31, 4, LevelTier.Normal, "Welded halves get shoved together by the coils. Watch the shutter!");
            d.repulsors.Add(Repulsor(3, 4));
            d.repulsors.Add(Repulsor(5, 2));
            d.repulsors.Add(Repulsor(5, 6));
            d.placements.Add(Scrap(1, 2, 2, 2));
            d.placements.Add(Scrap(2, 1, 5, 4));
            d.placements.Add(Scrap(1, 1, 7, 7));
            d.placements.Add(Rect(G, 2, 2, 2, 5));
            d.placements.Add(Rect(O, 2, 2, 0, 4));
            d.placements.Add(Rect(B, 2, 2, 0, 0));
            d.placements.Add(Rect(R, 2, 2, 7, 1));
            d.placements.Add(Rect(C, 2, 2, 3, 1));
            d.placements.Add(Rect(P, 1, 2, 5, 6));
            d.placements.Add(Rect(Y, 2, 1, 6, 6));
            d.placements.Add(Rect(C, 2, 1, 7, 0));
            d.placements.Add(Link(Rect(O, 1, 1, 2, 1), 1));
            d.placements.Add(Rect(O, 1, 1, 1, 6));
            d.placements.Add(Rect(P, 1, 1, 4, 8));
            d.placements.Add(Link(Rect(Y, 1, 1, 5, 1), 2));
            d.placements.Add(Link(Rect(C, 1, 1, 3, 0), 1));
            d.placements.Add(Rect(Y, 1, 1, 3, 7));
            d.placements.Add(Link(Rect(P, 1, 1, 5, 2), 2));
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
            d.gates.Add(Shutter(Left, 4, 2, O, true));
            d.gates.Add(Gate(Down, 6, 2, C));
            d.parts.Add(Part(PartType.Ring, 0, O, 3, 0));
            d.parts.Add(Part(PartType.Ring, 1, C, 3, 0));
            d.parts.Add(Part(PartType.Ring, 2, Y, 3, 0));
            d.parts.Add(Part(PartType.Ring, 3, P, 3, 0));
            d.parts.Add(Part(PartType.Planet, 0, G, 3, 1));
            d.parts.Add(Part(PartType.Planet, 1, R, 3, 1));
            d.parts.Add(Part(PartType.Planet, 2, B, 3, 1));
            d.parts.Add(Part(PartType.Cage, 0, C, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, O, 2, 3));
            ResolveHidden(d, false);
            return d;
        }
    }
}
