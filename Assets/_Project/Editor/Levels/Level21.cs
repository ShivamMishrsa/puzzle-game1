using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Emerald, Key-Lock & 360 Inspect: Green blocks arrive; a brass key opens the sealed Green gate; one brass screw pins the phone's front cradle.</summary>
    public sealed class Level21 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(21, "Emerald Key", 6, 6, 15, 5, LevelTier.Normal, "Green leaves only through the padlocked gate - send the brass key block out first! Drag the phone to spin it.");
            d.showRotateHint = true;
            d.placements.Add(Scrap(1, 1, 2, 3));
            d.placements.Add(Scrap(1, 1, 3, 2));
            d.placements.Add(Rect(R, 2, 2, 2, 4));
            d.placements.Add(Rect(B, 2, 2, 4, 2));
            d.placements.Add(Rect(G, 2, 1, 2, 1));
            d.placements.Add(Key(Rect(B, 1, 1, 1, 5), 0));
            d.placements.Add(Rect(G, 1, 1, 0, 4));
            d.gates.Add(Gate(Down, 0, 2, R));
            d.gates.Add(Gate(Right, 3, 2, R));
            d.gates.Add(Gate(Up, 0, 2, B));
            d.gates.Add(Gate(Left, 3, 2, B));
            d.gates.Add(Padlock(Gate(Right, 0, 2, G), 0));
            d.gates.Add(Padlock(Gate(Up, 3, 3, G), 0));
            d.parts.Add(Part(PartType.Handset, 0, R, 3, 0));
            d.parts.Add(Part(PartType.Handset, 1, G, 3, 0));
            d.parts.Add(Screw(Part(PartType.Cradle, 0, B, 2, 0), R));
            d.parts.Add(Part(PartType.Dial, 0, B, 3, 1));
            ResolveHidden(d, false);
            return d;
        }
    }
}
