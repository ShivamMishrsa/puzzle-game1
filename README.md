#| 14 | 131-140 | Steam Locomotive | EXTREME+: 11x11 and (from 135) 12x12 trays, 17-21 blocks, hill-climbed layouts with 46-78 move exact solutions, 3 track slots, every mechanic stacked. Solutions are stored, so Hint is instant. |
 Color Gate Blast

Hybrid-casual ASMR puzzle for Android (Unity 6.3 LTS, URP, portrait 1080x1920).
Slide blocks into the matching-colour gate on the tray border. Each block that leaves becomes a turret (ammo = tile
count) on the conveyor and shoots the matching parts of a 3D monument (alarm clock, cassette player, ...), which
physically falls apart. Same-colour turrets merge into a laser Mega-Turret.

## Status
- [x] Engine-free rules core: tray (gates, obstacles, ice, peeled blocks, shutters), monument model, turret
      merging, solver (BFS + weighted best-first) - `Assets/_Project/Scripts/Core`
- [x] Levels, one class per level in `Assets/_Project/Editor/Levels`, every one proven winnable within its move limit
- [x] Unity runtime v1: 3D tray + conveyor + turrets, alarm clock and cassette player monuments, physics debris, TNT, procedural sound/FX, HUD, AdMob (test ids) + simulator, solver-based hints, `Tools > Color Gate Blast > Setup Phase 1`
- [x] Levels 1-120 (12 chapters), solver-verified, one commit per level
- [ ] Authentication / security / database (after game + levels)

See `docs/PLAYTEST.md` for the in-Unity checklist (what the solver cannot prove: look, sound, feel).

## Photorealistic ASMR Workshop style (every level)
One look for every `LevelData` (1-40 today, 41-150 automatically): `Assets/_Project/Scripts/Visuals`
- `WorkshopCameraRig` - FOV 38, pitch 58, 9:16; solves distance + look-at so the single rounded conveyor loop (monument on top,
  riveted tray below) fills the screen between a 12 % top HUD clearance and a 15 % bottom margin.
- `WorkshopEnvironmentBuilder` - scratched walnut plank bench with grain normal map, coffee mug (top-left), bit kit +
  black/yellow flathead screwdriver (top-right), 18 scattered wood screws / washers.
- `WorkshopLightingSetup` - warm soft-shadow key light (falls bottom-left), cool rim light, runtime URP volume: bloom 0.95,
  depth of field, vignette 0.22, ACES.
- `HudSkin` + `UIManager` - embossed LEVEL title, riveted steel MOVES / REMAINING plaques with badges, Track Capacity LED
  pips, Restart plaque, pliers + gear medallions with padlock badges (rewarded-ad gated, test ids only).
- Materials in `GameMaterials`: block colours #B8332A / #2E649E / #C9A236 / #1E825C (metallic 0.45, smooth 0.58),
  scrap iron #5A5856 with rust, gunmetal riveted tray, U-bracket gates with neon chevrons + coloured point light,
  brushed-steel belt with scrolling dashed centre line.
- Pliers booster: rewarded ad, then tap any tray piece to snip it off. Drag arrow: a coloured floor arrow points into the open gate.

## Levels
| Chapter | Levels | Monument | New mechanics |
|---|---|---|---|
| 1 | 1-10 | Retro Alarm Clock | gates, turret merging, scrap iron, TNT, L-shapes |
| 2 | 11-20 | Retro Cassette Player | ice, peeled (dual-layer) blocks, shutter gates |
| 4 | 31-40 | Antique Pocket Watch | teleporter portal pads, clockwork piercing gates |
| 3 | 21-30 | Vintage Rotary Phone | Green, brass screw-bolts, key blocks + padlocked gates, rail blocks, Overdrive |
| 5 | 41-50 | Vintage Brass & Copper Espresso Machine | welded twin-linked blocks (steel bridge bar), steam pressure gates (+2 bonus ammo while pressurized) |
| 6 | 51-60 | Vintage Twin-Lens Reflex Camera | colour-shifting prism blocks (advance on every exit), electromagnetic pull-pads (free 1-tile pull) |
| 7 | 61-70 | Vintage Mechanical Typewriter | 5th colour Purple, carriage-shift rows (blocks slide 1 tile after every exit), crated mystery blocks |
| 8 | 71-80 | Vintage Brass & Mahogany Gramophone | 2-layer nested armour (inner cores shielded), one-way ratchet turnstiles, 8x8 trays, exact optimal move limits |
| 9 | 81-90 | Celestial Brass Orrery | 7 colours (+ Orange, Cyan), repulsor coils, 3-layer vault armour (Mega-Turrets pierce a layer), 9x9 trays |
| 10 | 91-100 | Swiss Skeleton Tourbillon | 9x9, all 7 colours, Magnetic Polarity Flipper Coils (PULL/PUSH flip on every exit), coupled multi-screw plates, 4 track slots (3 in the L95 mini-boss and L100 Centennial boss), exact moves |
| 11 | 101-110 | Grandfather Clock Tower | 10x10 trays (more blocks, 14-19 per level), all 7 colours, 4-layer armour (hood + glass door, dial + swinging pendulum, weights + chimes, dynamite), polarity coils + repulsors + coupled screws + portals + ratchets, 4 track slots (3 in L105 and L110), exact moves |
| 12 | 111-120 | Automaton Music Box | 11x11 trays (14-19 blocks), all 7 colours, glass dome + comb/dancer + pinned discs + dynamite, three polarity coils, coupled screws, ratchets, two portal pairs, 3 track slots (2 in the L120 boss), exact moves and a tighter clock (par x 1.7 + 5 s, min 40 s) |

## Front end
Splash screen, then a scrolling story map with all 150 level nodes (stars, padlocks, a bobbing buddy on the current level, a world banner per chapter). `MAP` button in game and on the win / lose panels. Gear button: sound on/off and an "unlock all levels (testing)" switch. Code: `Visuals/MapScreen.cs`.

## Clock and move limits
| Class | Levels | Clock | Moves |
|---|---|---|---|
| Easy | 1-20 | 30 s | a few spare moves |
| Medium | 21-40 | 45 s | a couple of spare moves |
| Hard | 41+ (normal levels) | 60 s | exactly the shortest solution |
| Extremely hard | 41+ (mini-boss and chapter boss) | 120 s | exactly the shortest solution |

The clock starts at your first touch and pauses during rewarded ads. It is stretched to 2 s per move + 10 s when a level's solution is long, so it is always winnable. When it runs out you can revive with +20 s (rewarded ad).

## Play in the browser
See `docs/WEBGL.md` (GitHub Actions auto-build, or a local one-click build into `docs/play`). Link after publishing: https://shivammishrsa.github.io/puzzle-game1/

## Verify levels without Unity
See `Tools~/LevelVerifier/README.md`.

## In Unity
`Tools > Color Gate Blast > Generate Levels 1-10` (and `11-20`) writes `Resources/Levels/Level_NNN.asset`, saving a
level only if the solver proves it winnable in its move limit. `Verify All Levels (solver)` re-checks them.





