using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>MINI-BOSS Darkroom Aperture Trap: two prism blocks, a padlocked gate and two brass screws pinning the iris blades and the TNT bundle. Only 4 track slots.</summary>
    public sealed class Level55 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(55, "Aperture Trap", 7, 7, 17, 4, LevelTier.MiniBoss, "Two prisms, a padlock and two bolts. Stuck? Use the hint or the pliers!");
            d.placements.Add(Scrap(1, 2, 3, 3));
            d.placements.Add(Scrap(2, 1, 1, 1));
            d.placements.Add(Rect(B, 2, 2, 3, 0));
            d.placements.Add(Rect(R, 1, 2, 3, 5));
            d.placements.Add(Rect(Y, 1, 2, 5, 4));
            d.placements.Add(Rect(R, 1, 2, 5, 1));
            d.placements.Add(Rect(G, 2, 1, 0, 3));
            d.placements.Add(Rect(G, 1, 1, 1, 2));
            d.placements.Add(Rect(Y, 1, 1, 5, 6));
            d.placements.Add(Prism(Rect(G, 1, 1, 1, 0), G, Y));
            d.placements.Add(Key(Rect(Y, 1, 1, 0, 5), 0));
            d.placements.Add(Prism(Rect(B, 1, 1, 1, 5), B, R));
            d.gates.Add(Padlock(Gate(Down, 4, 2, B), 0));
            d.gates.Add(Gate(Left, 4, 2, B));
            d.gates.Add(Gate(Right, 0, 2, R));
            d.gates.Add(Gate(Up, 0, 2, R));
            d.gates.Add(Gate(Up, 4, 2, Y));
            d.gates.Add(Gate(Left, 0, 2, Y));
            d.gates.Add(Gate(Down, 0, 2, G));
            d.gates.Add(Gate(Right, 4, 2, G));
            d.parts.Add(Part(PartType.Body, 0, Y, 2, 0));
            d.parts.Add(Part(PartType.Hood, 0, G, 1, 0));
            d.parts.Add(Part(PartType.Lens, 0, B, 2, 0));
            d.parts.Add(Part(PartType.Lens, 1, R, 2, 0));
            d.parts.Add(Screw(Part(PartType.Iris, 0, R, 2, 1), Y));
            d.parts.Add(Part(PartType.Crank, 0, Y, 1, 1));
            d.parts.Add(Part(PartType.ShutterGear, 0, G, 2, 2));
            d.parts.Add(Screw(Part(PartType.Tnt, 0, B, 3, 2), G));
            ResolveHidden(d, false);
            d.showRotateHint = true;
            return d;
        }
    }
}
