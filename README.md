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
- [x] Levels 1-60 (6 chapters), solver-verified, one commit per level
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

## Play in the browser
See `docs/WEBGL.md` (GitHub Actions auto-build, or a local one-click build into `docs/play`). Link after publishing: https://shivammishrsa.github.io/puzzle-game1/

## Verify levels without Unity
See `Tools~/LevelVerifier/README.md`.

## In Unity
`Tools > Color Gate Blast > Generate Levels 1-10` (and `11-20`) writes `Resources/Levels/Level_NNN.asset`, saving a
level only if the solver proves it winnable in its move limit. `Verify All Levels (solver)` re-checks them.





