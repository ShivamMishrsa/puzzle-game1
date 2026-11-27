using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>CHAPTER 6 BOSS: the full three-stage Vintage TLR Camera, 42 HP. Rotate it for the rear screws, time two prism blocks through a padlocked gate and a steam gate, then detonate the dynamite bundle in the film chamber.</summary>
    public sealed class Level60 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(60, "TLR Camera Finale", 7, 7, 22, 4, LevelTier.ChapterBoss, "Rotate the camera for the rear bolts. Time the prisms, open the padlock, blow the film chamber!");
            d.showRotateHint = true;
            d.reward = "Chapter 6 Complete: Vintage TLR Camera Dismantled! Camera trophy placed on your workshop shelf.";
            d.placements.Add(Scrap(1, 2, 3, 3));
            d.placements.Add(Rect(R, 2, 2, 4, 2));
            d.placements.Add(Rect(R, 2, 2, 1, 5));
            d.placements.Add(Rect(B, 2, 2, 0, 2));
            d.placements.Add(Rect(G, 2, 2, 0, 0));
            d.placements.Add(Rect(B, 1, 2, 6, 3));
            d.placements.Add(Rect(Y, 1, 2, 6, 0));
            d.placements.Add(Rect(R, 1, 2, 0, 5));
            d.placements.Add(Rect(G, 1, 2, 5, 0));
            d.placements.Add(Prism(Rect(B, 1, 1, 2, 4), B, R));
            d.placements.Add(Key(Rect(Y, 1, 1, 5, 4), 0));
            d.placements.Add(Prism(Rect(G, 1, 1, 5, 5), G, Y));
            d.gates.Add(Padlock(Gate(Down, 0, 2, R), 0));
            d.gates.Add(Steam(Gate(Right, 4, 2, R), 2));
            d.gates.Add(Gate(Left, 5, 2, R));
            d.gates.Add(Gate(Left, 3, 2, B));
            d.gates.Add(Gate(Up, 0, 2, B));
            d.gates.Add(Gate(Up, 4, 2, Y));
            d.gates.Add(Gate(Right, 0, 2, Y));
            d.gates.Add(Gate(Left, 0, 2, G));
            d.gates.Add(Gate(Down, 4, 2, G));
            d.parts.Add(Part(PartType.Body, 0, B, 3, 0));
            d.parts.Add(Part(PartType.Body, 1, Y, 3, 0));
            d.parts.Add(Part(PartType.Hood, 0, G, 2, 0));
            d.parts.Add(Screw(Part(PartType.Lens, 0, R, 4, 0), Y));
            d.parts.Add(Screw(Part(PartType.Lens, 1, B, 4, 0), G));
            d.parts.Add(Part(PartType.Iris, 0, R, 5, 1));
            d.parts.Add(Part(PartType.Crank, 0, Y, 1, 1));
            d.parts.Add(Part(PartType.Spool, 0, B, 4, 2));
            d.parts.Add(Part(PartType.Spool, 1, G, 4, 2));
            d.parts.Add(Part(PartType.ShutterGear, 0, Y, 4, 2));
            d.parts.Add(Part(PartType.Tnt, 0, R, 4, 2));
            d.parts.Add(Part(PartType.Tnt, 1, G, 4, 2));
            ResolveHidden(d, false);
            return d;
        }
    }
}
