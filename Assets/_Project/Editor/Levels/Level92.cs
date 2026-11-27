using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Coupled-Screw Vault (Tourbillon): the bezel and a skeleton bridge are chained down by coupled screws - two or three linked bolts of different colours that must all be shot out before the plate can be hurt.</summary>
    public sealed class Level92 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(92, "Coupled Screws", 9, 9, 21, 4, LevelTier.Normal, "Linked bolts: every screw on a plate needs its own colour turret!");
            d.placements.Add(Scrap(1, 2, 3, 3));
            d.placements.Add(Scrap(2, 1, 5, 5));
            d.placements.Add(Scrap(1, 1, 7, 2));
            d.placements.Add(Rect(G, 2, 2, 7, 3));
            d.placements.Add(Rect(O, 2, 2, 0, 4));
            d.placements.Add(Rect(P, 2, 2, 2, 6));
            d.placements.Add(Rect(Y, 2, 2, 2, 0));
            d.placements.Add(Rect(R, 2, 2, 5, 2));
            d.placements.Add(Rect(B, 2, 2, 7, 0));
            d.placements.Add(Rect(C, 1, 2, 4, 1));
            d.placements.Add(Rect(G, 2, 1, 1, 3));
            d.placements.Add(Rect(C, 1, 1, 4, 0));
            d.placements.Add(Rect(R, 1, 1, 4, 8));
            d.placements.Add(Rect(O, 1, 1, 2, 5));
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
            d.parts.Add(Screws(Part(PartType.Bezel, 0, O, 3, 0), R, B));
            d.parts.Add(Part(PartType.Bezel, 1, C, 3, 0));
            d.parts.Add(Part(PartType.Crystal, 0, Y, 3, 0));
            d.parts.Add(Screws(Part(PartType.Bridge, 0, G, 3, 1), P, Y, R));
            d.parts.Add(Part(PartType.Bridge, 1, P, 3, 1));
            d.parts.Add(Part(PartType.TourbillonCage, 0, B, 3, 1));
            d.parts.Add(Part(PartType.MainBarrel, 0, R, 3, 2));
            d.parts.Add(Part(PartType.Rotor, 0, G, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, O, 2, 3));
            ResolveHidden(d, false);
            return d;
        }
    }
}
