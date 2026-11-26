namespace ColorGateBlast
{
    /// <summary>Block / gate / turret / brick colour identity. Index doubles as palette index.</summary>
    public enum BlockColor
    {
        Red = 0,
        Blue = 1,
        Green = 2,
        Yellow = 3,
        Purple = 4,
        Orange = 5
    }

    /// <summary>Side of the board (for gates) or of the target core (for turret firing lanes).</summary>
    public enum Side
    {
        Top = 0,
        Right = 1,
        Bottom = 2,
        Left = 3
    }

    /// <summary>Special behaviour of a block. Exclusive per block.</summary>
    public enum BlockKind
    {
        Normal = 0,
        /// <summary>Frozen until <c>iceThawExits</c> blocks have left the board.</summary>
        Ice = 1,
        /// <summary>When this block leaves the board all locked gates open.</summary>
        Key = 2,
        /// <summary>Colour is hidden until the block is first touched (visual only).</summary>
        Mystery = 3
    }

    /// <summary>Movement restriction of a block ("Directional-Only" blocks).</summary>
    public enum MoveAxis
    {
        Free = 0,
        Horizontal = 1,
        Vertical = 2
    }

    public enum LevelTier
    {
        Normal = 0,
        MiniBoss = 1,
        ChapterBoss = 2
    }

    public enum Difficulty
    {
        Easy = 0,
        Medium = 1,
        Hard = 2
    }

    public enum GameState
    {
        Loading,
        Playing,
        Won,
        Failed
    }
}
