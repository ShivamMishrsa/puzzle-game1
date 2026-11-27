using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Exposed Vinyl and Mainspring: the horn is already gone - 30 HP of 2-layer spinning record, governor weights, three brass screws and twin dynamite bundles.</summary>
    public sealed class Level78 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(78, "Exposed Vinyl", 8, 8, 28, 4, LevelTier.Normal, "No horn - shoot the spinning record. Outer layer first, inner second!");
            d.turnstiles.Add(Turnstile(2, 3, Up));
            d.turnstiles.Add(Turnstile(5, 4, Left));
            d.placements.Add(Scrap(1, 1, 3, 3));
            d.placements.Add(Scrap(1, 1, 4, 5));
            d.placements.Add(Scrap(1, 1, 6, 2));
            d.placements.Add(Rect(Y, 2, 2, 4, 2));
            d.placements.Add(Rect(G, 2, 2, 1, 0));
            d.placements.Add(Rect(P, 2, 2, 3, 0));
            d.placements.Add(Rect(B, 2, 2, 3, 6));
            d.placements.Add(Rect(R, 2, 2, 0, 5));
            d.placements.Add(Rect(G, 1, 2, 0, 3));
            d.placements.Add(Rect(P, 2, 1, 5, 0));
            d.placements.Add(Rect(P, 1, 2, 5, 6));
            d.placements.Add(Rect(R, 1, 2, 6, 5));
            d.placements.Add(Rect(B, 2, 1, 6, 1));
            d.placements.Add(Rect(B, 1, 2, 2, 6));
            d.placements.Add(Rect(Y, 1, 2, 3, 4));
            d.placements.Add(Rect(R, 2, 1, 1, 2));
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
            d.parts.Add(Screw(Part(PartType.Record, 0, P, 4, 0), G));
            d.parts.Add(Part(PartType.Record, 1, B, 3, 0));
            d.parts.Add(Part(PartType.Governor, 0, G, 4, 1));
            d.parts.Add(Screw(Part(PartType.Cabinet, 0, R, 4, 2), R));
            d.parts.Add(Part(PartType.Cabinet, 1, Y, 3, 2));
            d.parts.Add(Part(PartType.Mainspring, 0, B, 4, 3));
            d.parts.Add(Screw(Part(PartType.Tnt, 0, R, 2, 3), Y));
            d.parts.Add(Part(PartType.Tnt, 1, P, 2, 3));
            ResolveHidden(d, false);
            return d;
        }
    }
}
