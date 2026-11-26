# Color Gate Blast

Hybrid-casual puzzle for Android (Unity 6.3 LTS, URP, portrait 1080x1920).
Slide polyomino blocks into the matching-colour gate on the board border. Each block that leaves becomes a
turret (ammo = tile count) on the outer conveyor and shoots matching shield bricks on the Target Core.

## Open and play Level 1
1. Install Unity 6.3 LTS (6000.3.x) with Android Build Support, then add this folder in Unity Hub.
2. Menu **Tools > Color Gate Blast > Setup Main Scene** (builds camera, light, board, conveyor, core, UI).
3. Menu **Tools > Color Gate Blast > Generate Starter Levels 1-10**.
4. Set the Game view to **1080 x 1920 (Portrait)** and press **Play**.

Other tools: **Verify All Levels (re-solve)** and **Configure Android (API 36, AAB)**.

## Layout
- `Assets/_Project/Scripts/` runtime game code. `Core/` is UnityEngine-free rules code (board, core, solver state).
- `Assets/_Project/Editor/` level generator (sawtooth difficulty), solver (BFS + weighted best-first), menus.
- `Assets/_Project/Resources/Levels/` generated `LevelData` assets.

## Roadmap
Levels are built in parts of 10 (1-10, 11-20, ...) up to 150. Authentication, security and a database come after
the game and levels are done.
