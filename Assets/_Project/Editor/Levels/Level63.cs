using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Carriage and Portal combo: carriage row 2 pushes a Green 1x1 onto a cyan pad on the second exit; it warps in front of the opposite Green gate.</summary>
    public sealed class Level63 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(63, "Carriage Portal", 7, 7, 16, 5, LevelTier.Normal, "The carriage rolls the green block onto the pad - it warps across the tray!");
            d.carriages.Add(Carriage(2, 1));
            d.portals.Add(Pad(0, 3, 2, 5, 6));
            d.placements.Add(Scrap(1, 1, 1, 4));
            d.placements.Add(Scrap(1, 1, 4, 3));
            d.placements.Add(Rect(G, 1, 1, 2, 2));
            d.placements.Add(Rect(B, 2, 2, 4, 5));
            d.placements.Add(Rect(R, 2, 2, 0, 1));
            d.placements.Add(Rect(Y, 1, 2, 6, 4));
            d.placements.Add(Rect(P, 1, 2, 4, 1));
            d.placements.Add(Rect(P, 2, 1, 2, 4));
            d.placements.Add(Rect(Y, 2, 1, 3, 0));
            d.placements.Add(Rect(G, 2, 1, 0, 5));
            d.placements.Add(Rect(R, 1, 2, 6, 0));
            d.placements.Add(Rect(B, 1, 2, 2, 5));
            d.gates.Add(Gate(Up, 5, 1, G));
            d.gates.Add(Gate(Left, 4, 2, G));
            d.gates.Add(Gate(Up, 0, 2, R));
            d.gates.Add(Gate(Right, 0, 2, R));
            d.gates.Add(Gate(Down, 0, 2, B));
            d.gates.Add(Gate(Left, 0, 2, B));
            d.gates.Add(Gate(Down, 4, 2, Y));
            d.gates.Add(Gate(Right, 4, 2, Y));
            d.gates.Add(Gate(Up, 2, 2, P));
            d.gates.Add(Gate(Down, 2, 2, P));
            d.parts.Add(Part(PartType.Hood, 0, B, 3, 0));
            d.parts.Add(Part(PartType.Platen, 0, R, 3, 0));
            d.parts.Add(Part(PartType.Keys, 0, G, 2, 0));
            d.parts.Add(Part(PartType.Typebar, 0, P, 3, 1));
            d.parts.Add(Part(PartType.Ribbon, 0, Y, 2, 1));
            d.parts.Add(Part(PartType.Spring, 0, G, 2, 2));
            d.parts.Add(Part(PartType.Tnt, 0, R, 2, 2));
            ResolveHidden(d, false);
            return d;
        }
    }
}
