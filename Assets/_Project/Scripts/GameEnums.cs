namespace ColorGateBlast
{
    /// <summary>Gray = indestructible scrap-iron obstacle (never moves, never exits, no gate).</summary>
    public enum BlockColor
    {
        Red = 0,
        Blue = 1,
        Yellow = 2,
        Gray = 3
    }

    /// <summary>Which edge of the tray a gate is on = the direction a block moves to leave through it.</summary>
    public enum GateDirection
    {
        Up = 0,
        Down = 1,
        Left = 2,
        Right = 3
    }

    public enum GameState
    {
        Preparing,
        Playing,
        Won,
        Lost
    }

    public enum BlockShape
    {
        /// <summary>width x height rectangle.</summary>
        Rect,
        /// <summary>3-tile L inside a 2x2 box; <c>rotation</c> picks the missing corner.</summary>
        L
    }

    /// <summary>"Directional arrow" blocks may only slide along one axis.</summary>
    public enum MoveAxis
    {
        Free = 0,
        Horizontal = 1,
        Vertical = 2
    }

    /// <summary>The destructible pieces of the Retro Alarm Clock monument.</summary>
    public enum PartType
    {
        Casing,
        Bell,
        Hand,
        Gear,
        Glass,
        /// <summary>TNT box (alarm clock) / TNT capacitor (cassette player).</summary>
        Tnt,
        // ---- Chapter 2: Retro Cassette Player ----
        Door,
        Cassette,
        Pcb,
        Button
    }

    /// <summary>Which 3D monument a level disassembles.</summary>
    public enum MonumentKind
    {
        AlarmClock = 0,
        CassettePlayer = 1
    }

    public enum LevelTier
    {
        Normal = 0,
        MiniBoss = 1,
        ChapterBoss = 2
    }
}
