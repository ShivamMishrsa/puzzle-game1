using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>CHAPTER 7 BOSS: the full three-stage Vintage Typewriter, 46 HP. Rotate it for the rear screws, ride two carriage rows to unbox a hidden brass key, merge a Purple Mega-Turret and detonate the dynamite in the chassis.</summary>
    public sealed class Level70 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(70, "Typewriter Finale", 7, 7, 27, 4, LevelTier.ChapterBoss, "Rotate the typewriter for the rear bolts. Ride the carriages, unbox the key, merge Purple, blow the chassis!");
            d.showRotateHint = true;
            d.reward = "Chapter 7 Complete: Vintage Typewriter Dismantled! Typewriter trophy placed on your workshop shelf.";
            d.carriages.Add(Carriage(1, 1));
            d.carriages.Add(Carriage(5, -1));
            d.placements.Add(Scrap(1, 2, 3, 3));
            d.placements.Add(Scrap(2, 1, 4, 2));
            d.placements.Add(Rect(Y, 2, 2, 5, 3));
            d.placements.Add(Rect(R, 2, 2, 1, 3));
            d.placements.Add(Rect(B, 2, 2, 1, 0));
            d.placements.Add(Rect(G, 2, 2, 3, 5));
            d.placements.Add(Rect(P, 2, 2, 1, 5));
            d.placements.Add(Rect(Y, 1, 2, 0, 1));
            d.placements.Add(Rect(B, 1, 2, 3, 0));
            d.placements.Add(Rect(P, 2, 1, 4, 1));
            d.placements.Add(Rect(R, 2, 1, 5, 0));
            d.placements.Add(Rect(P, 1, 2, 0, 5));
            d.placements.Add(Rect(G, 1, 2, 6, 1));
            d.placements.Add(Rect(R, 1, 2, 4, 3));
            d.placements.Add(Crate(Key(Rect(B, 1, 1, 5, 6), 0)));
            d.gates.Add(Padlock(Gate(Down, 4, 2, P), 0));
            d.gates.Add(Gate(Left, 4, 2, P));
            d.gates.Add(Gate(Up, 0, 2, R));
            d.gates.Add(Gate(Right, 0, 2, R));
            d.gates.Add(Gate(Down, 0, 2, B));
            d.gates.Add(Gate(Left, 0, 2, B));
            d.gates.Add(Gate(Up, 4, 2, Y));
            d.gates.Add(Gate(Right, 4, 2, Y));
            d.gates.Add(Gate(Left, 2, 2, G));
            d.gates.Add(Gate(Up, 2, 2, G));
            d.parts.Add(Part(PartType.Hood, 0, B, 3, 0));
            d.parts.Add(Part(PartType.Hood, 1, Y, 2, 0));
            d.parts.Add(Part(PartType.Platen, 0, R, 3, 0));
            d.parts.Add(Part(PartType.Lever, 0, P, 2, 0));
            d.parts.Add(Screw(Part(PartType.Keys, 0, G, 3, 0), P));
            d.parts.Add(Part(PartType.Typebar, 0, B, 4, 1));
            d.parts.Add(Part(PartType.Ribbon, 0, Y, 2, 1));
            d.parts.Add(Part(PartType.Ribbon, 1, P, 3, 1));
            d.parts.Add(Part(PartType.Rack, 0, Y, 2, 2));
            d.parts.Add(Part(PartType.Spring, 0, R, 2, 2));
            d.parts.Add(Part(PartType.Spring, 1, G, 3, 2));
            d.parts.Add(Part(PartType.Spring, 2, P, 2, 2));
            d.parts.Add(Screw(Part(PartType.Tnt, 0, R, 2, 2), R));
            ResolveHidden(d, false);
            return d;
        }
    }
}
