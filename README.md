# Color Gate Blast

Hybrid-casual ASMR puzzle for Android (Unity 6.3 LTS, URP, portrait 1080x1920).
Slide blocks into the matching-colour gate on the tray border. Each block that leaves becomes a turret (ammo = tile
count) on the conveyor and shoots the matching parts of a 3D monument (alarm clock, cassette player, ...), which
physically falls apart. Same-colour turrets merge into a laser Mega-Turret.

## Status
- [x] Engine-free rules core: tray (gates, obstacles, ice, peeled blocks, shutters), monument model, turret
      merging, solver (BFS + weighted best-first) - `Assets/_Project/Scripts/Core`
- [x] Levels, one class per level in `Assets/_Project/Editor/Levels`, every one proven winnable within its move limit
- [x] Unity runtime v1: 3D tray + conveyor + turrets, alarm clock and cassette player monuments, physics debris, TNT, procedural sound/FX, HUD, AdMob (test ids) + simulator, solver-based hints, `Tools > Color Gate Blast > Setup Phase 1`
- [ ] Authentication / security / database (after game + levels)

## Levels
| Chapter | Levels | Monument | New mechanics |
|---|---|---|---|
| 1 | 1-10 | Retro Alarm Clock | gates, turret merging, scrap iron, TNT, L-shapes |
| 2 | 11-20 | Retro Cassette Player | ice, peeled (dual-layer) blocks, shutter gates |
| 4 | 31-40 | Antique Pocket Watch | teleporter portal pads, clockwork piercing gates |
| 3 | 21-30 | Vintage Rotary Phone | Green, brass screw-bolts, key blocks + padlocked gates, rail blocks, Overdrive |

## Verify levels without Unity
See `Tools~/LevelVerifier/README.md`.

## In Unity
`Tools > Color Gate Blast > Generate Levels 1-10` (and `11-20`) writes `Resources/Levels/Level_NNN.asset`, saving a
level only if the solver proves it winnable in its move limit. `Verify All Levels (solver)` re-checks them.



