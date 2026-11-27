using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Twin Padlocks & Screwed Armor (mini-boss): two padlocked gates seal the Red and Green exits; two screws guard the dial and the TNT.</summary>
    public sealed class Level25 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(25, "Twin Padlocks", 7, 7, 18, 4, LevelTier.MiniBoss, "Two padlocks, two screws, four slots. Stuck? Use the hint!");
            d.placements.Add(Scrap(1, 1, 3, 3));
            d.placements.Add(Scrap(1, 1, 2, 4));
            d.placements.Add(Rect(R, 2, 2, 5, 5));
            d.placements.Add(Rect(Y, 1, 2, 5, 1));
            d.placements.Add(Rect(B, 2, 1, 2, 1));
            d.placements.Add(Rect(G, 2, 1, 0, 5));
            d.placements.Add(Rect(G, 1, 1, 1, 4));
            d.placements.Add(Key(Rect(B, 1, 1, 0, 1), 0));
            d.placements.Add(Key(Rect(Y, 1, 1, 6, 0), 1));
            d.placements.Add(Rect(B, 1, 1, 1, 6));
            d.gates.Add(Padlock(Gate(Down, 0, 2, R), 0));
            d.gates.Add(Padlock(Gate(Up, 4, 2, R), 0));
            d.gates.Add(Padlock(Gate(Left, 3, 2, G), 1));
            d.gates.Add(Padlock(Gate(Right, 0, 2, G), 1));
            d.gates.Add(Gate(Up, 0, 2, B));
            d.gates.Add(Gate(Left, 0, 2, B));
            d.gates.Add(Gate(Right, 4, 2, Y));
            d.gates.Add(Gate(Down, 4, 2, Y));
            d.parts.Add(Part(PartType.Handset, 0, R, 3, 0));
            d.parts.Add(Part(PartType.Handset, 1, G, 3, 0));
            d.parts.Add(Part(PartType.Cradle, 0, Y, 2, 0));
            d.parts.Add(Screw(Part(PartType.Dial, 0, B, 3, 1), Y));
            d.parts.Add(Screw(Part(PartType.Tnt, 0, R, 1, 2), B));
            ResolveHidden(d, false);
            return d;
        }
    }
}
