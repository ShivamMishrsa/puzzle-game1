# Playtest checklist (Unity Editor)

What the solver already proved (`Tools~/LevelVerifier`): every level 1-40 is **winnable within its move limit**.
What only a human in Unity can confirm: how it **looks, sounds and feels**. Tick these off.

## Automated play-through (already done)
An autoplay bot (`Assets/_Project/Scripts/Testing/AutoPlayer.cs`) plays every level inside Unity's Play Mode: it asks the solver for the next move from the LIVE game state, performs it through the real drag code, waits for turrets / projectiles / monument to settle, and repeats until the level ends. Result with Unity 6000.3.29f1: **all 40 levels WON, 0 errors, 0 warnings.** Re-run it:
```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.3.29f1\Editor\Unity.exe' -batchmode -nographics -projectPath <project> -executeMethod ColorGateBlast.EditorTools.AutoPlayLauncher.Run -cgbBot [-cgbFrom 1 -cgbTo 40]
```n(Run Setup Phase 1 once first so the scene and level assets exist.) It proves the game logic end to end; it cannot judge how things look or sound.

## Quick start (4 steps)
1. Open the folder in Unity 6.3 LTS; wait for packages; fix any red Console error first.
2. **Tools > Color Gate Blast > Setup Phase 1** (builds the scene and generates + solver-verifies all level assets).
3. Game view -> **1080 x 1920 (Portrait)**.
4. Press **Play**. Jump anywhere with **Tools > Color Gate Blast > Play From Level > Level 11 / 21 / 31 ...** (then Play).

Ads: test ids + an on-screen simulator are used until you import the Google Mobile Ads plugin and run
**Tools > Color Gate Blast > AdMob > Enable...**. Never tap live ads on your own phone (AdMob suspends accounts for self-clicks).

## Always check
- Drag a block: lifts, slides cell by cell, **trajectory lines** show on the floor; the line is **bright in the block colour when it reaches a gate**.
- Block enters its gate -> zoom-exit, gate sparks, a turret spawns on the conveyor with an **ammo badge = tile count**.
- Two same-colour turrets -> magnetic pull, burst, one bigger **Mega-Turret** with a laser.
- Shot parts fall off with physics, bounce off the conveyor, shed screws / springs / fragments, then vanish.
- TNT: **0.06 s freeze-frame**, slow-motion tail, camera shake, shockwave pushes debris.
- Combo exits climb the scale **C D E G A**; haptic ticks on Android (not in the Editor).

## Per-level focus
| Level | Check |
|---|---|
| 1-3 | Red bar slides straight into its gate (L1). Two reds exit back-to-back and merge (L2). Gray scrap iron never moves; shaking + ping when tapped (L3). |
| 4, 9 | Red shot on the TNT box blows the casing / chains to the second TNT. |
| 5 | Mini-boss gold banner. Yellow bar blocks the blue block. |
| 10 | Chapter-boss red banner, 2 track slots, 3 chained TNT, "Diorama Unlock" text on win. |
| **11, 13** | **Frosted ice shows crisp fracture lines (1 layer) and shatters into ice shards with a crack sound when the neighbouring block leaves. L13: the peeled yellow block shows a glowing blue stripe on its top face BEFORE it leaves, then a blue 1x1 core drops into its slot.** |
| 12 | Two iced reds thaw one after the other and merge into a red Mega-Turret that melts the acrylic door. |
| 14 | 2-layer ice: first hit changes the shell to the cracked look, second hit frees the block. |
| **15, 20** | Boss banners. **Hint button** (blue, orange "AD" badge): after the (simulated) ad, the next block glows gold with animated arrows to its gate. L15 shutter doors slide open (green LED) / slam shut (amber LED) with a clank on every exit. **+1 Track Slot** only appears when the conveyor is full - see note below. |
| 16 | Central blue block out: three ice shells crack at once, shutter opens, three reds rush out. |
| 17 | Left and right shutters are in opposite phase. |
| 18 | 7x7 board; cassette starts exposed. |
| 19 | 2-layer ice + peeled + shutter + pillar. |
| 21 | 360 icon beside the phone; drag the upper screen to rotate it. Brass key block; padlocked green gate; screw on the cradle. |
| 22 | Key #1 -> gate #1 -> key #2 -> gate #2. Rear TNT visible when you rotate. |
| 23 | Rails under the bars; dragging sideways gives a spring nudge + ping. |
| 24 | Three blue exits in a row -> **OVERDRIVE SURGE** (belt 1.75x for 3 s, free rapid fire, major arpeggio). |
| 25, 35, 39, 40 | 4 colours + 4 slots: the only levels where the conveyor can really fill up (see note). |
| 26 | Key out -> three rail blocks pour out -> Mega-Turret. |
| 27 | Peeled block's core carries the key; ice beside it thaws. |
| 28 | Handset and dial already removed. |
| 30 | Rotate the phone: green screw on the back; TNT tucked behind the casing; dial clicks on every hit. |
| **31, 37** | **A 2x1 block rolled over a portal pad does NOT teleport; only 1x1 blocks warp (L31: the lone red; L37: after the yellow 2x1 leaves, its 1x1 red core can).** No two blocks may ever overlap; a pad whose partner is occupied pulses amber and refuses entry. |
| **32, 38** | **Pocket watch: balance wheel oscillates, hands tick with a tick-tock sound, rotating 360 degrees stays smooth.** L32: brass gears frame the top (piercing) gate; one green block damages the lid AND the dome. |
| 33 | Ice block sits on the exit pad; free the red block below it. |
| 34 | Cyan and violet pad pairs + rails. |
| 36 | One block warps and exits in a single move, thawing three ice blocks. |
| 40 | Rotate for the rear screws; warp the key out of its pocket; padlocked piercing gates; mainspring TNT. |

## Note on "tight" 4-slot levels
Same-colour turrets **merge into one slot**. With only 3 colours (levels 15, 19, 20) four slots can never fill, so the
"Track Full" lose screen and the **+1 Track Slot** button cannot appear there; the **Hint** button is the monetisation
moment on those. The 4-colour levels (25, 35, 39, 40) and the 2-slot level 10 can fill the belt.

## Workshop style check (every level)
- [ ] Top 12 % shows only the LEVEL title + MOVES / REMAINING plaques; monument sits inside the top of the conveyor loop.
- [ ] Riveted tray inside the loop; bottom strip has Track Capacity LED pips (amber when occupied), Restart, pliers + gear medallions.
- [ ] Dragging a block towards an open gate shows a glowing colour arrow on the tray floor pointing into the gate.
- [ ] Gates are U-shaped brackets with neon chevrons and a coloured glow on the wood.
- [ ] Pliers medallion: rewarded-ad simulator -> medallion glows gold -> tap any tray piece -> it is snipped off (no turret, no move cost).
- [ ] Merged Mega-Turret: twin barrels, white-hot laser core with coloured glow, sparks at the target.

## Batches 5 and 6 (levels 41-60)
- [ ] **Welded pairs (41-50):** dragging either half moves both; the steel bridge bar with the glowing seam follows. Pushing the half whose colour matches the gate out snaps the bar (sparks + metallic snap), the other half steps forward and is free. A frozen half (44) locks the whole pair.
- [ ] **Steam gates (42, 46, 49, 50, 60):** the brass gauge needle swings to the red zone every 2 moves and the vent puffs steam; leaving while pressurized shows "PRESSURIZED! +2 ammo". L42 cannot be won without it.
- [ ] **Prism blocks (51-60):** the glass gem shows the active colour, the corner pip the next one; every block exit flashes + chimes and advances every prism. A prism only exits through a gate of its CURRENT colour.
- [ ] **Magnet pads (52, 56, 59):** a block that comes to rest right behind a pad is pulled onto it with a clunk, free of charge (no move used).
- [ ] **Monuments:** espresso machine (copper boiler dome + eagle finial, gauge, lever pinned by a rear screw, chrome group head, glass sight tube, drip grid, coils, valves, dynamite bundle) and TLR camera (leatherette plates, hood, two lenses that shatter, spinning iris blades, film crank, spinning spools, shutter gears, dynamite). Rotate 360 degrees on L50 / L60 for the rear screws.
- [ ] Chapter bosses (50, 60) show the "Chapter N Complete" banner and add a trophy to the shelf counter.

## Batch 7 (levels 61-70) and the clock
- [ ] **Clock:** a steel plate between MOVES and REMAINING shows the time; it starts on your first touch, turns red and pulses in the last 10 s; TIME'S UP offers +20 s (rewarded ad). Easy 30 s, medium 45 s, hard 60 s, extreme 120 s (stretched for long solutions).
- [ ] **Hard / extreme (41+):** MOVES equals the shortest solution - one wasted move and the level is lost.
- [ ] **Purple (61+):** amethyst blocks, gates, turrets and lasers.
- [ ] **Carriage rows (61, 63, 65, 66, 69, 70):** brass rails with a gold chevron strip; after EVERY exit the blocks lying inside the row slide one tile (carriage slide + bell ding); the strip flips when the row bounces off a wall. A 1x1 sliding onto a portal pad warps (63).
- [ ] **Mystery crates (62, 64, 67, 69, 70):** dark-oak crates with a gold "?": they slide but cannot leave a gate; the planks burst off (splinters + crack) when a neighbour exits. L64: ice + crate together. L67 / L70: the brass key is inside a crate.
- [ ] **Typewriter:** hood, rubber platen that slides + dings when hit, carriage-return lever, round brass keys (rear screws), typebar fan that flicks, ink-ribbon spools, escapement rack, springs, dynamite bundle.

## Front end, clock and batches 8-9 (levels 71-90)
- [ ] **Splash -> story map:** pastel splash with the three block buddies and a loading bar, then the map scrolled to your current level; locked / coming-soon nodes show a padlock and a toast; gear = sound + unlock-all (testing).
- [ ] **Stopwatch clock** between MOVES and REMAINING: brass bezel, ring drains green -> amber -> red, pulses in the last 10 s.
- [ ] **MOVES plaque** glows with a pulsing red rim at 3 moves or fewer; numerals are amber "Nixie" digits in a dark tube window.
- [ ] **One-way ratchets (73, 75, 77-80, 83, 88, 89...):** brass ratchet gear + chevron; pushing a block onto it from the wrong side clicks (click-clack) and does not move.
- [ ] **Nested armour (71-90):** inner parts (bracket, governor, mainspring / planets, cage) cannot be hit until their outer shell is gone. In 81-90 a Mega-Turret (merged colour) and a piercing-gate turret also strip the layer behind their target.
- [ ] **Repulsor coils (81, 83, 85-90):** a block that stops on or next to a coil is shoved one tile away with a thump + sparks (a welded pair moves together).
- [ ] **Orange / Cyan (81+),** hammered copper and satin teal; amber lamp rim light + floating dust motes on every level.
- [ ] **Gramophone / Orrery monuments:** spinning record, governor weights, flared horn; orbiting planets on brass gear arms inside tilted hoops.

## Batch 10 (levels 91-100, Swiss Skeleton Tourbillon)

- [ ] **Polarity coils (91, 93, 95-100):** a copper coil with a neon ring - cyan **PULL** drags a resting neighbour block one tile in, crimson-orange **PUSH** shoves it one tile away. Every coil flips on every block exit (arc spark + zap sound); the label under the ring shows the current mode.
- [ ] **Coupled screws (92, 95, 98-100):** a plate can carry 2-3 brass screws with different colour rings; each needs its own matching-colour turret and the plate cannot be damaged until all are out.
- [ ] **Tourbillon monument:** rose-gold bezel, sapphire crystal (glass break), chronograph pushers, skeleton bridges, spinning tourbillon cage with ruby jewel, twin barrels, micro-rotor, dynamite behind the cage.
- [ ] **Slots:** 4 track slots, 3 in Level 95 (mini-boss) and Level 100 (boss). Levels use exact optimal move limits (best found by deep search).
- [ ] **Level 100:** golden "CENTENNIAL MILESTONE" win banner and the chapter-10 trophy.
