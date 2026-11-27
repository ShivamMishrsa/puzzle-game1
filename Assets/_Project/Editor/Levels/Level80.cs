using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>CHAPTER 8 BOSS: the full three-stage Vintage Gramophone, 2-layer nested armour. Rotate it for the rear bolts, work a one-way turnstile and key-lock sequence with zero wasted moves, strip outer armour before inner cores, blow the dynamite in the cabinet.</summary>
    public sealed class Level80 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(80, "Gramophone Finale", 8, 8, 26, 4, LevelTier.ChapterBoss, "Rotate for the rear bolts. Ratchets, a key, outer armour first - exact moves only!");
            d.reward = "Chapter 8 Complete: Vintage Gramophone Dismantled! Gramophone trophy placed on your workshop shelf.";
            d.turnstiles.Add(Turnstile(3, 3, Right));
            d.turnstiles.Add(Turnstile(5, 4, Up));
            d.turnstiles.Add(Turnstile(2, 5, Down));
            d.turnstiles.Add(Turnstile(6, 2, Left));
            d.placements.Add(Scrap(1, 2, 3, 2));
            d.placements.Add(Scrap(2, 1, 5, 5));
            d.placements.Add(Scrap(1, 1, 1, 6));
            d.placements.Add(Rect(B, 2, 2, 2, 0));
            d.placements.Add(Rect(Y, 2, 2, 1, 2));
            d.placements.Add(Rect(P, 2, 2, 4, 0));
            d.placements.Add(Rect(G, 2, 2, 5, 2));
            d.placements.Add(Rect(R, 2, 2, 0, 0));
            d.placements.Add(Rect(P, 1, 2, 5, 6));
            d.placements.Add(Rect(G, 2, 1, 5, 4));
            d.placements.Add(Rect(G, 1, 2, 7, 2));
            d.placements.Add(Rect(Y, 2, 1, 3, 7));
            d.placements.Add(Rect(R, 1, 2, 2, 4));
            d.placements.Add(Rect(Y, 1, 2, 4, 4));
            d.placements.Add(Rect(R, 2, 1, 6, 6));
            d.placements.Add(Rect(P, 2, 1, 0, 5));
            d.placements.Add(Rect(B, 1, 2, 6, 0));
            d.placements.Add(Key(Rect(B, 1, 1, 0, 3), 0));
            d.gates.Add(Gate(Up, 0, 2, R));
            d.gates.Add(Gate(Down, 0, 2, R));
            d.gates.Add(Gate(Left, 0, 2, B));
            d.gates.Add(Gate(Right, 0, 2, B));
            d.gates.Add(Gate(Down, 2, 2, Y));
            d.gates.Add(Gate(Up, 2, 2, Y));
            d.gates.Add(Gate(Right, 2, 2, G));
            d.gates.Add(Gate(Left, 2, 2, G));
            d.gates.Add(Padlock(Gate(Up, 4, 2, P), 0));
            d.gates.Add(Gate(Down, 4, 2, P));
            d.parts.Add(Screw(Part(PartType.Horn, 0, Y, 4, 0), G));
            d.parts.Add(Screw(Part(PartType.Horn, 1, R, 3, 0), P));
            d.parts.Add(Part(PartType.Record, 0, P, 4, 1));
            d.parts.Add(Part(PartType.Record, 1, B, 4, 1));
            d.parts.Add(Part(PartType.Cabinet, 0, R, 3, 2));
            d.parts.Add(Part(PartType.Cabinet, 1, P, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, R, 1, 3));
            d.parts.Add(Part(PartType.Tnt, 1, G, 2, 3));
            d.parts.Add(Screw(Part(PartType.Bracket, 0, B, 8, 1), R));
            d.parts.Add(Part(PartType.Governor, 0, G, 8, 2));
            d.parts.Add(Part(PartType.Mainspring, 0, B, 8, 3));
            ResolveHidden(d, false);
            d.showRotateHint = true;
            return d;
        }
    }
}
