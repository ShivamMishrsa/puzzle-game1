using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Piston Portals (Steam Locomotive): EXTREME+. Two portal pairs, three welded pairs, a clockwork-piercing gate and two shutter gates, with two polarity coils and a repulsor deciding who is pulled through.</summary>
    public sealed class Level133 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(133, "Piston Portals", 11, 11, 78, 3, LevelTier.Normal, "Extreme+! Two wormholes, welded pairs, a piercing gate and shutters.");
            d.polarities.Add(Polarity(5, 6, true));
            d.polarities.Add(Polarity(3, 3, false));
            d.repulsors.Add(Repulsor(4, 4));
            d.portals.Add(Pad(0, 1, 2, 9, 8));
            d.portals.Add(Pad(1, 9, 2, 2, 8));
            d.placements.Add(Scrap(1, 1, 5, 3));
            d.placements.Add(Scrap(1, 2, 3, 5));
            d.placements.Add(Scrap(2, 1, 7, 7));
            d.placements.Add(Rect(Y, 2, 2, 6, 2));
            d.placements.Add(Rect(G, 2, 2, 1, 5));
            d.placements.Add(Rect(C, 2, 2, 2, 3));
            d.placements.Add(Rect(Y, 2, 2, 7, 5));
            d.placements.Add(Rect(O, 2, 2, 5, 9));
            d.placements.Add(Rect(C, 2, 2, 6, 0));
            d.placements.Add(Rect(P, 2, 2, 0, 3));
            d.placements.Add(Rect(O, 2, 2, 5, 5));
            d.placements.Add(Rect(B, 2, 2, 2, 9));
            d.placements.Add(Rect(R, 2, 2, 8, 3));
            d.placements.Add(Rect(B, 1, 2, 6, 7));
            d.placements.Add(Rect(R, 2, 1, 3, 0));
            d.placements.Add(Link(Rect(Y, 1, 1, 3, 2), 2));
            d.placements.Add(Link(Rect(C, 1, 1, 5, 2), 1));
            d.placements.Add(Link(Rect(O, 1, 1, 4, 1), 1));
            d.placements.Add(Link(Rect(G, 1, 1, 4, 3), 2));
            d.placements.Add(Link(Rect(R, 1, 1, 8, 0), 3));
            d.placements.Add(Rect(R, 1, 1, 3, 7));
            d.placements.Add(Rect(G, 1, 1, 9, 0));
            d.placements.Add(Link(Rect(P, 1, 1, 8, 2), 3));
            d.gates.Add(Gate(Up, 0, 2, R));
            d.gates.Add(Gate(Down, 0, 2, R));
            d.gates.Add(Gate(Left, 0, 2, B));
            d.gates.Add(Gate(Right, 0, 2, B));
            d.gates.Add(Gate(Down, 2, 2, Y));
            d.gates.Add(Shutter(Up, 2, 2, Y, false));
            d.gates.Add(Shutter(Right, 2, 2, G, true));
            d.gates.Add(Gate(Left, 2, 2, G));
            d.gates.Add(Gate(Up, 4, 2, P));
            d.gates.Add(Gate(Down, 4, 2, P));
            d.gates.Add(Piercing(Gate(Left, 4, 2, O)));
            d.gates.Add(Gate(Down, 6, 2, C));
            d.parts.Add(Part(PartType.Boiler, 0, C, 4, 0));
            d.parts.Add(Part(PartType.Boiler, 1, O, 4, 0));
            d.parts.Add(Part(PartType.Smokestack, 0, R, 3, 0));
            d.parts.Add(Part(PartType.Cab, 0, Y, 4, 1));
            d.parts.Add(Part(PartType.Piston, 0, G, 4, 1));
            d.parts.Add(Part(PartType.Piston, 1, P, 3, 1));
            d.parts.Add(Part(PartType.Driver, 0, B, 4, 2));
            d.parts.Add(Part(PartType.Driver, 1, C, 3, 2));
            d.parts.Add(Part(PartType.Coal, 0, R, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, O, 3, 3));
            d.parts.Add(Part(PartType.Tnt, 1, Y, 3, 3));
            ResolveHidden(d, false);
            d.solution = new int[] { 83887624, 67175939, 67174409, 134807819, 134217742, 83886094, 134677012, 524821, 134742293, 218237461, 201656582, 526598, 134546442, 459786, 201525516, 67176716, 198675, 201525523, 201393923, 67110413, 134676500, 134676746, 17432596, 134545412, 67108873, 67239940, 151126020, 134676490, 67371018, 184811530, 458761, 201786377, 134414597, 67174405, 201917959, 17368073, 67567883, 134807559, 592134, 134808070, 17367558, 67108869, 67240203, 67371015, 184811527, 201327109, 67108875, 150994955, 201459216, 198160, 201525008, 329488, 395539, 134612499, 17433878, 134807827, 17432851, 526604, 134742028, 17367052, 201394435, 526595, 134742531, 17367555, 218106381, 201328133, 83887621, 83886863, 132114, 134349586, 151191570, 67241744, 134349072, 327952, 184811537, 458768, 201786896, 83887632 };
            return d;
        }
    }
}
