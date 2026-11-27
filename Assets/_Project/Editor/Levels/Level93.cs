using ColorGateBlast;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Polarity Ratchet Ring (Tourbillon): three polarity coils inside a ring of one-way ratchets - a PULL grabs you through a ratchet the wrong way, a PUSH throws you out. Time every exit.</summary>
    public sealed class Level93 : LevelBase
    {
        public static LevelDefinition Build()
        {
            LevelDefinition d = Make(93, "Ratchet Polarity", 9, 9, 23, 4, LevelTier.Normal, "Coils flip with every exit and the ratchet only turns one way!");
            d.turnstiles.Add(Turnstile(4, 4, Right));
            d.turnstiles.Add(Turnstile(4, 3, Up));
            d.polarities.Add(Polarity(2, 2, true));
            d.polarities.Add(Polarity(6, 6, false));
            d.polarities.Add(Polarity(2, 6, true));
            d.placements.Add(Scrap(1, 2, 3, 3));
            d.placements.Add(Scrap(2, 1, 5, 5));
            d.placements.Add(Rect(C, 2, 2, 2, 0));
            d.placements.Add(Rect(G, 2, 2, 7, 2));
            d.placements.Add(Rect(P, 2, 2, 4, 6));
            d.placements.Add(Rect(C, 2, 2, 4, 1));
            d.placements.Add(Rect(O, 2, 2, 0, 6));
            d.placements.Add(Rect(B, 2, 2, 6, 0));
            d.placements.Add(Rect(Y, 2, 2, 7, 7));
            d.placements.Add(Rect(R, 2, 2, 0, 1));
            d.placements.Add(Rect(O, 2, 1, 0, 5));
            d.placements.Add(Rect(G, 1, 1, 4, 4));
            d.placements.Add(Rect(O, 1, 1, 2, 5));
            d.placements.Add(Rect(B, 1, 1, 3, 2));
            d.placements.Add(Rect(Y, 1, 1, 3, 6));
            d.placements.Add(Rect(R, 1, 1, 1, 8));
            d.placements.Add(Rect(P, 1, 1, 8, 6));
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
            d.parts.Add(Part(PartType.Bezel, 0, C, 3, 0));
            d.parts.Add(Part(PartType.Bezel, 1, O, 3, 0));
            d.parts.Add(Part(PartType.Crystal, 0, P, 3, 0));
            d.parts.Add(Part(PartType.Bridge, 0, Y, 3, 1));
            d.parts.Add(Part(PartType.Bridge, 1, G, 3, 1));
            d.parts.Add(Part(PartType.TourbillonCage, 0, R, 3, 1));
            d.parts.Add(Part(PartType.MainBarrel, 0, B, 3, 2));
            d.parts.Add(Part(PartType.Rotor, 0, C, 3, 2));
            d.parts.Add(Part(PartType.Tnt, 0, O, 2, 3));
            ResolveHidden(d, false);
            return d;
        }
    }
}
