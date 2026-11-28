using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Frozen Express (Steam Locomotive): EXTREME+. Three ice-sealed 4-colour prisms, three mystery crates, two clockwork carriage rows and two polarity coils. One wrong thaw and the route is gone.</summary>
    public sealed class Level132 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(132, "Frozen Express", 11, 11, 69, 3, LevelTier.Normal, "Extreme+! Ice, crates, prisms and two carriages - the order is everything.");
            d.carriages.Add(Carriage(3, 1));
            d.polarities.Add(Polarity(5, 5, true));
            d.polarities.Add(Polarity(8, 7, false));
            d.placements.Add(Scrap(1, 1, 2, 7));
            d.placements.Add(Scrap(1, 1, 8, 4));
            d.placements.Add(Scrap(1, 1, 6, 8));
            d.placements.Add(Rect(G, 2, 2, 9, 1));
            d.placements.Add(Rect(R, 2, 2, 4, 5));
            d.placements.Add(Rect(B, 2, 2, 9, 7));
            d.placements.Add(Rect(Y, 2, 2, 6, 5));
            d.placements.Add(Rect(G, 2, 2, 8, 9));
            d.placements.Add(Rect(C, 2, 2, 0, 9));
            d.placements.Add(Rect(C, 2, 2, 3, 1));
            d.placements.Add(Rect(O, 2, 2, 3, 3));
            d.placements.Add(Rect(P, 2, 2, 7, 7));
            d.placements.Add(Rect(O, 2, 2, 2, 5));
            d.placements.Add(Rect(Y, 2, 1, 5, 9));
            d.placements.Add(Crate(Rect(B, 1, 1, 10, 5)));
            d.placements.Add(Rect(P, 1, 1, 8, 3));
            d.placements.Add(Prism(Ice(Rect(P, 1, 1, 9, 6), 2), P, R, O, C));
            d.placements.Add(Prism(Ice(Rect(O, 1, 1, 1, 8), 2), O, C, P, R));
            d.placements.Add(Prism(Ice(Rect(C, 1, 1, 4, 0), 2), C, P, R, O));
            d.placements.Add(Rect(B, 1, 1, 7, 9));
            d.placements.Add(Rect(Y, 1, 1, 5, 0));
            d.placements.Add(Rect(R, 1, 1, 10, 6));
            d.placements.Add(Crate(Rect(Y, 1, 1, 3, 9)));
            d.placements.Add(Rect(G, 1, 1, 8, 5));
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
            d.parts.Add(Part(PartType.Boiler, 0, O, 4, 0));
            d.parts.Add(Part(PartType.Boiler, 1, C, 4, 0));
            d.parts.Add(Part(PartType.Smokestack, 0, Y, 3, 0));
            d.parts.Add(Part(PartType.Cab, 0, G, 4, 1));
            d.parts.Add(Part(PartType.Piston, 0, P, 4, 1));
            d.parts.Add(Part(PartType.Piston, 1, R, 3, 1));
            d.parts.Add(Part(PartType.Driver, 0, B, 4, 2));
            d.parts.Add(Part(PartType.Driver, 1, O, 3, 2));
            d.parts.Add(Part(PartType.Coal, 0, C, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, Y, 3, 3));
            d.parts.Add(Part(PartType.Tnt, 1, G, 3, 3));
            ResolveHidden(d, false);
            d.solution = new int[] { 133379, 201918727, 201393673, 83887625, 134415631, 201656599, 134350851, 67176974, 134350595, 67307799, 134350339, 218302999, 218237187, 134415631, 67307014, 67438347, 67175942, 134283782, 134414346, 262154, 67371532, 67372036, 134611979, 67176211, 151257098, 134415375, 151257100, 134479876, 656653, 134873613, 17433101, 329231, 17433366, 83886084, 17368075, 17433871, 83886598, 151060499, 134808583, 67240967, 151126023, 201917192, 67437320, 201655560, 83887624, 66836, 134284052, 67439125, 134545685, 459029, 83886357, 525076, 134742548, 17433108, 67436817, 201656593, 151322641, 592133, 134807557, 67108869, 201327109, 134284302, 150994949, 151060494, 328722, 201656594, 151322642, 134612240, 17433872 };
            return d;
        }
    }
}
