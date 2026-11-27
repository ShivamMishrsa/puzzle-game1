namespace ColorGateBlast
{
    /// <summary>Gray = indestructible scrap-iron obstacle (never moves, never exits, no gate).</summary>
    public enum BlockColor
    {
        Red = 0,
        Blue = 1,
        Yellow = 2,
        Gray = 3,
        /// <summary>Emerald Enamel Green (levels 21+).</summary>
        Green = 4,
        /// <summary>Royal Amethyst Purple (levels 61+).</summary>
        Purple = 5
    }

    public static class ColorCount
    {
        /// <summary>Number of BlockColor values (array size for per-colour tables).</summary>
        public const int Total = 6;
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
        Button,
        // ---- Chapter 3: Vintage Rotary Phone ----
        Handset,
        Cradle,
        Dial,
        Ringer,
        Coil,
        // ---- Chapter 4: Antique Pocket Watch ----
        BalanceWheel,
        Ruby,
        // ---- Chapter 5: Vintage Espresso Machine ----
        Boiler,
        Gauge,
        Lever,
        GroupHead,
        SightTube,
        DripGrid,
        Valve,
        // ---- Chapter 6: Twin-Lens Reflex Camera ----
        Body,
        Hood,
        Lens,
        Iris,
        Crank,
        Spool,
        ShutterGear,
        // ---- Chapter 7: Vintage Mechanical Typewriter ----
        Platen,
        Keys,
        Typebar,
        Ribbon,
        Rack,
        Spring
    }

    /// <summary>Which 3D monument a level disassembles.</summary>
    public enum MonumentKind
    {
        AlarmClock = 0,
        CassettePlayer = 1,
        RotaryPhone = 2,
        PocketWatch = 3,
        EspressoMachine = 4,
        TlrCamera = 5,
        Typewriter = 6
    }

    /// <summary>Difficulty class of a level: sets the countdown clock and whether the move limit is exact.</summary>
    public enum DifficultyClass
    {
        Easy = 0,       // levels 1-20:   30 s
        Medium = 1,     // levels 21-40:  45 s
        Hard = 2,       // levels 41+:    60 s, exact move count
        Extreme = 3     // bosses / mini-bosses from 41 on: 120 s, exact move count
    }

    public enum LevelTier
    {
        Normal = 0,
        MiniBoss = 1,
        ChapterBoss = 2
    }
}
