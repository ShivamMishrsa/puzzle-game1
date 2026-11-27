using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>MINI-BOSS Typewriter Jam: two carriage rows run in opposite directions, a padlocked gate and two brass screws pin the typebar basket and the dynamite. Only 4 track slots.</summary>
    public sealed class Level65 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(65, "Typewriter Jam", 7, 7, 19, 4, LevelTier.MiniBoss, "Two carriages, a padlock and two bolts. Stuck? Use the hint or the pliers!");
            d.carriages.Add(Carriage(1, 1));
            d.carriages.Add(Carriage(5, -1));
            d.placements.Add(Scrap(1, 2, 3, 2));
            d.placements.Add(Scrap(2, 1, 4, 3));
            d.placements.Add(Rect(R, 2, 2, 1, 1));
            d.placements.Add(Rect(B, 2, 2, 1, 5));
            d.placements.Add(Rect(B, 1, 2, 4, 5));
            d.placements.Add(Rect(G, 2, 1, 0, 3));
            d.placements.Add(Rect(Y, 2, 1, 5, 5));
            d.placements.Add(Rect(G, 1, 2, 3, 0));
            d.placements.Add(Rect(P, 2, 1, 4, 0));
            d.placements.Add(Rect(P, 1, 2, 6, 0));
            d.placements.Add(Rect(R, 1, 2, 2, 3));
            d.placements.Add(Key(Rect(B, 1, 1, 0, 1), 0));
            d.placements.Add(Rect(Y, 1, 1, 0, 2));
            d.gates.Add(Padlock(Gate(Down, 4, 2, P), 0));
            d.gates.Add(Gate(Left, 4, 2, P));
            d.gates.Add(Gate(Up, 0, 2, R));
            d.gates.Add(Gate(Right, 0, 2, R));
            d.gates.Add(Gate(Down, 0, 2, B));
            d.gates.Add(Gate(Left, 0, 2, B));
            d.gates.Add(Gate(Up, 4, 2, Y));
            d.gates.Add(Gate(Right, 4, 2, Y));
            d.gates.Add(Gate(Left, 2, 2, G));
            d.gates.Add(Gate(Down, 2, 2, G));
            d.parts.Add(Part(PartType.Hood, 0, B, 3, 0));
            d.parts.Add(Part(PartType.Platen, 0, R, 3, 0));
            d.parts.Add(Part(PartType.Keys, 0, G, 2, 0));
            d.parts.Add(Screw(Part(PartType.Typebar, 0, P, 3, 1), Y));
            d.parts.Add(Part(PartType.Ribbon, 0, Y, 2, 1));
            d.parts.Add(Part(PartType.Spring, 0, R, 2, 2));
            d.parts.Add(Screw(Part(PartType.Tnt, 0, G, 2, 2), B));
            ResolveHidden(d, false);
            d.showRotateHint = true;
            return d;
        }
    }
}
