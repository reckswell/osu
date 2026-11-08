# Gameplay spec: maniaortho

The source of truth for what the playfield must look and feel like. Numbers marked *(tune)* are starting points to be
refined by research and playtesting; record changes in `DECISIONS.md`. Items marked **[Alpha]** are pending Alpha's
answer in `QUESTIONS.md` and use the stated default until then.

Alpha's own words, which every item below must respect:

> The actual game itself should behave very similarly to the designs of SDVX, Project Sekai, Chunithm, and other
> orthographic VSRGs. I include SDVX as the primary choice since it uses actual buttons and fixed laning (which is the
> feature I want to preserve from osu mania!). Each note should approach from the top of the screen towards the bottom,
> rather than from the center, but add an option to add walls (similar to chunithm) where you can adjust where the
> notes actually come from on the screen. The general user interface (song selection, menu, etc.) can stay the same.

## 1. Model

The playfield is mania's lane-and-time model with a camera on top. Mania already turns each note's time into a
distance along its lane (including scroll velocity changes). This ruleset keeps that distance exactly and only changes
how a point at (lane position, distance) is placed on screen.

- **Track space**: x = across lanes (0 at the left edge of lane 0, 1 at the right edge of the last lane),
  d = distance from the judgement line along the track (0 at the line, 1 at the far edge of the visible track).
- **Projection**: a pure function `TrackToScreen(x, d) -> screen point` with no game state, unit-tested on its own.
- Everything drawn on the track (notes, holds, bar lines, lane dividers, the wall) is defined in track space and
  projected. HUD, judgement text and combo stay in screen space.

## 2. Camera

| Mode | Look | Use |
|---|---|---|
| **Perspective** (default **[Alpha]**) | True 3D projection. Lanes converge toward a vanishing point above the top of the screen; far notes are smaller and move slower on screen. | SDVX / Chunithm look |
| **Parallel tilt** | Track foreshortened vertically, lanes stay parallel (no convergence). Literally orthographic. | Readability at high key counts |
| **Flat** | Identical layout to osu!mania. | Baseline, and a sanity check that the projection path is correct |

Requirements:

- Notes enter at the **far end of the lane** (or at the wall, section 4), a visible edge with real width, never a
  point: the top edge of the screen or below it, as each game draws it (Alpha, 2026-11-08: Chunithm's notes appear
  under its HUD band, 16% down; SDVX's towards the middle, 41% down).
- The judgement line sits near the bottom: default 12% of screen height above the bottom edge *(tune)*.
- Parameters (Alpha, 2026-11-08: "a slider that allows the user to adjust the angle at which the lane actually ends
  relative to the top of the screen"): **lane tilt** (0-85°, how far the lane leans back, pivoting on the judgement
  line, so more tilt ends it lower on the screen), **lane length** (0.25-16 screen heights; longer narrows the far end;
  perspective only; a lane longer than the screen allows is cut at the top edge), judgement line height and track width
  at the line (fraction of screen width; or a lane width, section 5). One camera with a fixed 60° field of view for
  every preset, so a tilt looks the same in each (`research/topic-approach-angles.md`): Chunithm 56.3° (ends 16% down),
  Project Sekai 57.8° on a longer lane (to the top), SDVX 81.4° (ends 41% down). The parallel camera has no depth, so only its tilt counts.
- **Game presets** (Alpha, 2026-11-08: "sort all of the presets by games"): each plays like one game's screen, with its
  camera, judgement line, lane or stage width and note thickness, and shows the speed and the wall in that game's
  numbers and steps (§3, §4). Tilted: **Chunithm** (notes from under its HUD band), **SDVX** (notes from towards the middle,
  narrow far end), **Project Sekai** (high line, very narrow far end). Straight-on: **beatmania IIDX**, **DJMAX**,
  **Quaver**, **StepMania**. **osu!mania** is mania's own stage (flat); **Parallel tilt** a neutral parallel camera.
  Games without a wall or lane cover (SDVX, Project Sekai, DJMAX, osu!mania) have none: the wall is down and its
  settings hidden. Each game remembers everything under "Game preset" as the player leaves it (Alpha, 2026-11-08: "sdvx
  should have wall = 0, chunithm should have a saved wall preset depending on what the previous option was"): its look
  and numbers as changes on top of the game's own values (a setting's reset goes back to the game's), and the player's
  speed, wall and whether the wall keeps the visible time; a game chosen for the first time starts from the speed and
  wall in use. Custom keeps all of its own values. Values come from the research notes and are recorded in
  `DECISIONS.md`.
- **Settings panel** (Alpha, 2026-11-08): under the ruleset's heading, "Settings: orthomania" first (Gameplay: notes
  first; Other settings: track and motion, skin layout, input, folded by default), then "Game preset" (the game, then
  Speed, Lane and camera, Lane and note size, Wall). Each group folds and stays as left; searching finds settings in
  folded groups.
- No camera motion driven by beatmap data. Optional cosmetic motion is allowed, off by default, and must never move the
  judgement line: **kiai sway** (the far end swings with the beat) and **kiai lift** (the lane pitches up towards the top
  of the screen over three beats, as SDVX's charts do at a chorus, with the notes' speed at the line kept;
  `research/topic-camera-motion.md`).

## 3. Scroll speed

- Speed is defined as **visible time**: the milliseconds a note takes from appearing (far edge or wall) to reaching
  the judgement line. This matches mania's existing scroll speed setting (which maps to a time range) and IIDX's
  "green number", so a speed feels the same in every camera mode.
- Notes move linearly in **track space** (d is linear in time at constant SV). Under perspective they therefore move
  slowly far away and accelerate near the line. That is correct and expected (it is what SDVX does); do not "correct"
  it in screen space.
- **Note approach** (Alpha, 2026-11-08, Q17 (b)): a choice on every perspective camera of how track depth is drawn
  along the lane, from **Perspective** (the default; linear in depth, as above) through **Project Sekai** (notes grow by
  the same factor every moment, Sekai's own curve) to **Even** (a steady speed on screen). The track's shape does not
  change, notes still appear and reach the line at the same times, and everything placed by depth follows the same
  curve (`ApproachCurve`).
- Scroll velocity (SV) changes behave exactly as in mania (the same algorithm: sequential, or constant with the
  Constant Speed mod; mania has no overlapping mode).
- **Speed scale** (Alpha, 2026-11-07): the speed can be set in another game's numbers instead of osu!mania's: Chunithm,
  SDVX (lane speed), IIDX (green number), Project Sekai, BanG Dream!, Arcaea, DJMAX, Quaver or StepMania's CMod. Each
  number gives the time that game gives notes (`research/topic-speed.md`, `research/topic-speed-scales.md`): across the
  whole track for every game but IIDX, whose green number is the time visible below the wall. The speed stays osu!mania's
  one setting (stored finer, up to 60), so switching scales keeps it; F3/F4 move one of the chosen game's steps.
- Upscroll is not offered in this ruleset (the camera defines direction). Mania's scroll direction setting is ignored
  here, and that is recorded as a decision.

## 4. Walls

- **Top wall** (Chunithm's field wall, IIDX's SUDDEN+): an opaque cover over the far part of the track. Notes appear
  at the wall's edge. Position: 0-80% of the track length *(tune)*. Drawn in track space (it is a plane on the track
  or a vertical panel at that depth; pick whichever reads better in playtests).
- **Bottom lift** (IIDX's LIFT): optional, raises the judgement line. Off by default. Exception: a ported legacy skin
  whose hit position sits above the camera's line raises the line to the skin's hit position with the "Panel" and
  "Floor" key areas (section 8), never higher than the lift setting's maximum (40%).
- **Constant visible time by default**: moving the wall changes on-screen speed so that visible time stays the same.
  A toggle "keep speed when moving the wall" switches to keeping on-screen speed instead (visible time shrinks).
- Walls are ruleset **settings**, adjustable from settings and with a hotkey during gameplay (Shift+F3 / Shift+F4).
  They never change judgement or scoring, so they are not mods.

## 5. Lanes and key modes

- 1K-10K single stage are required. Dual stage (mania's 2x stages) is drawn as two highways side by side *(tune)*.
- The track keeps one width for every key count, as Chunithm's field does: more keys means narrower lanes, and the
  outermost lanes lean the same at 4K and 10K (Alpha, QUESTIONS Q10). Each preset sets the width, except under a
  ported skin at "Skin position and width" (§8), where the skin's own lane widths set it.
- **Width by lane** (Alpha, 2026-11-07, decision card): optionally the Highway track takes its width from a lane width
  instead, so its narrowest lane (between two dividers) is that wide at the judgement line and the track grows with the
  key count, up to the screen. One track width stays the default. The default skin's track only: ported and warped skin
  stages keep their own widths.
- Fixed lanes, one key per lane, same default bindings as mania. Key presses light the lane (SDVX-style beam rising
  from the judgement line) and the receptor at the line.

## 6. Notes, holds, bar lines

- Notes are rectangles in track space with a fixed thickness in track units, drawn as projected trapezoids. They shrink
  with distance in perspective mode.
- Highway notes are seamless with the lane lines: each side meets the line beside it with no gap and without covering
  it, at every depth and key count (Alpha, 2026-11-04). Without lane lines they keep a small gap inside their lane.
- Hold bodies are one projected quad from head to tail (subdivided if needed for texture correctness). Heads, bodies
  and tails must line up exactly at every depth: a misaligned hold is a visible bug.
- Textures on trapezoids use perspective-correct mapping. Two plain triangles show a bend along the diagonal.
- Bar lines are projected across the full track width.
- A note's leading edge must cross the judgement line exactly at its hit time (same as mania). Unit-test this for
  every camera mode.

## 7. Feedback and HUD

- Judgement text, combo and hit error meter stay in screen space near the judgement line, readable at all tilts.
- Hit lighting and explosions are drawn at the judgement line in the lane's projected position.
- The osu! HUD (score, accuracy, pp counter, progress) is unchanged.

## 8. Skins

- Ship a built-in highway skin first (simple, high contrast, SDVX-like colours per lane type).
- Legacy mania skins are **ported at runtime** (Alpha, 2026-11-04): the porter reads the skin's own images and
  `skin.ini` values through osu!'s mania skin lookups and draws them on the tilted track, so any skin the player
  selects plays tilted with no texture editing and no external tool. It covers notes, hold heads, bodies and tails
  (perspective-correct, keeping the skin's proportions), column backgrounds and lines, stage side images, the hit
  target, lane lights, keys, the stage bottom, and hit and hold lighting. The skin's own files are never changed.
- Skins whose hit position sits above the camera's judgement line (keys or an IIDX-style panel below it) choose where
  the key area goes ("Skin key area"); the skin's stage position (`ColumnStart`) moves the track sideways by default.
  By default every lane is as wide at the judgement line as the skin draws it ("Skin position and width"), and the
  track's centre there is exactly the skin's stage centre (as stable places it); a wider camera track that would not fit
  around it is narrowed, never moved. A side-placed stage's far end leans in towards the screen's centre ("Side stage
  lean", default 50%: its lanes meet halfway between the stage's centre and the screen's), and the whole stage can be
  pulled towards the centre ("Side stage pull", default 0%).
  Skins without legacy mania images (argon, triangles) keep the built-in highway look.

## 9. Mods

- Conversion and difficulty mods (Mirror, Random, key mods, DT/HT, HR/EZ, etc.) work unchanged.
- Visibility mods (Hidden, Fade In, Cover, Flashlight) need projected equivalents or are removed from this ruleset's
  mod list. Whichever is chosen per mod goes in `DECISIONS.md`. They must still score exactly like mania.

## 10. Performance

- Target: no dropped frames at 240 Hz at 1080p on Alpha's PC on dense 7K maps. No allocations per frame in the
  playfield. Measured only on device (L3), never in the cloud.

## 11. Acceptance checks (how each section is verified)

| Section | Check | Level |
|---|---|---|
| 1, 6 | `TrackToScreen` unit tests: lane edges, judgement line, far edge, monotonic depth, note crosses line at hit time | L0 |
| 3 | Visible time equals the configured time range in every camera mode (unit + screenshot at t = hit - visible time) | L0, L2 |
| 4 | Moving the wall keeps visible time constant (default) or speed constant (toggle) | L0, L2 |
| All | Replay parity: identical judgements, score and accuracy to mania | L1 |
| 2, 5, 6 | Screenshots of 4K and 7K at fixed times pass the rubric in `PLAYTESTING.md` | L2 |
| 10 | Frame times on Alpha's PC | L3 |
| Feel | Alpha's playtest notes | L4 |
