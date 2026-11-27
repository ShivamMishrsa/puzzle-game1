using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Shutter Trap (mini-boss): every red exit is a mechanical shutter; a TNT capacitor sits behind the cassette.</summary>
    public sealed class Level15 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(15, "Shutter Trap", 7, 7, 22, 4, LevelTier.MiniBoss, "Shutters toggle on EVERY exit. Time it so the red gate is open! Stuck? Use the hint.");
            d.placements.Add(Scrap(1, 1, 2, 3));
            d.placements.Add(Scrap(1, 1, 3, 2));
            d.placements.Add(Rect(Y, 2, 2, 3, 0));
            d.placements.Add(Rect(B, 2, 2, 3, 3));
            d.placements.Add(Ell(B, 1, 1, 0));
            d.placements.Add(Rect(R, 2, 1, 5, 3));
            d.placements.Add(Rect(Y, 2, 1, 4, 5));
            d.placements.Add(Rect(Y, 2, 1, 4, 6));
            d.placements.Add(Rect(R, 1, 2, 1, 3));
            d.placements.Add(Rect(R, 1, 1, 2, 5));
            d.placements.Add(Rect(B, 1, 1, 5, 1));
            d.placements.Add(Rect(Y, 1, 1, 0, 1));
            d.placements.Add(Rect(B, 1, 1, 1, 6));
            d.gates.Add(Shutter(Right, 2, 2, R, false));
            d.gates.Add(Shutter(Up, 0, 2, R, true));
            d.gates.Add(Gate(Down, 2, 2, B));
            d.gates.Add(Gate(Left, 4, 2, B));
            d.gates.Add(Gate(Left, 0, 2, Y));
            d.gates.Add(Gate(Up, 4, 2, Y));
            d.parts.Add(Part(PartType.Door, 0, R, 3, 0));
            d.parts.Add(Part(PartType.Casing, 0, B, 3, 0));
            d.parts.Add(Part(PartType.Casing, 1, Y, 2, 0));
            d.parts.Add(Part(PartType.Cassette, 0, R, 2, 1));
            d.parts.Add(Part(PartType.Tnt, 0, B, 1, 2));
            ResolveHidden(d, false);
            return d;
        }
    }
}
