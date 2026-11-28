using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Exposed Tourbillon: the bezel and crystal are already stripped off - the spinning tourbillon cage, skeleton bridges with coupled screws, twin barrels, micro-rotor and dynamite stand in plain view.</summary>
    public sealed class Level98 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(98, "Exposed Tourbillon", 9, 9, 31, 4, LevelTier.Normal, "No case - shoot the bridges, the spinning cage and the barrels!");
            d.polarities.Add(Polarity(3, 3, true));
            d.polarities.Add(Polarity(6, 5, false));
            d.placements.Add(Scrap(1, 1, 4, 4));
            d.placements.Add(Scrap(1, 2, 1, 6));
            d.placements.Add(Rect(P, 2, 2, 5, 6));
            d.placements.Add(Rect(G, 2, 2, 1, 0));
            d.placements.Add(Rect(C, 2, 2, 7, 2));
            d.placements.Add(Rect(O, 2, 2, 2, 5));
            d.placements.Add(Rect(Y, 2, 2, 3, 2));
            d.placements.Add(Rect(R, 2, 2, 2, 7));
            d.placements.Add(Rect(B, 2, 2, 7, 4));
            d.placements.Add(Rect(P, 2, 1, 5, 8));
            d.placements.Add(Rect(C, 2, 1, 6, 0));
            d.placements.Add(Rect(O, 1, 2, 6, 4));
            d.placements.Add(Rect(C, 1, 1, 4, 7));
            d.placements.Add(Rect(O, 1, 1, 2, 2));
            d.placements.Add(Rect(B, 1, 1, 0, 1));
            d.placements.Add(Rect(G, 1, 1, 2, 3));
            d.placements.Add(Rect(R, 1, 1, 1, 2));
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
            d.gates.Add(Gate(Left, 4, 2, O));
            d.gates.Add(Gate(Down, 6, 2, C));
            d.parts.Add(Screws(Part(PartType.Bridge, 0, C, 4, 0), R, B));
            d.parts.Add(Screws(Part(PartType.Bridge, 1, O, 4, 0), G, P));
            d.parts.Add(Part(PartType.Bridge, 2, Y, 3, 0));
            d.parts.Add(Part(PartType.TourbillonCage, 0, P, 4, 1));
            d.parts.Add(Part(PartType.MainBarrel, 0, R, 3, 2));
            d.parts.Add(Part(PartType.MainBarrel, 1, G, 3, 2));
            d.parts.Add(Part(PartType.Rotor, 0, B, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, O, 2, 3));
            d.parts.Add(Part(PartType.Tnt, 1, C, 2, 3));
            ResolveHidden(d, false);
            d.solution = new int[] { 134350340, 67371525, 134743049, 17302537, 151060494, 151191567, 151257093, 67371527, 134479879, 262669, 134349318, 83887626, 83887620, 67110664, 67307019, 67437826, 201786892, 17236994, 218105608, 134217731, 83886598, 134349579, 197387, 83887628, 201327107, 66051, 218236675, 83886352, 83886087, 151257101, 151257099 };
            return d;
        }
    }
}
