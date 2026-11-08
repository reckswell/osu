# Approach angles, lane tilt and physical screen angles

Last checked: 2026-11-08

## Summary

Alpha asked for a "tilt" slider that sets where (and how) the lane ends relative to the top of the screen, consistent
across the per-game presets, and suggested that the physical screen angles (Chunithm's reclined screen, SDVX's upright
portrait screen, Sekai on a phone lying flat) explain how each game's notes approach. Findings:

- **The physical screen angle does not explain the rendered perspective.** A reclined arcade screen mostly cancels the
  standing player's downward gaze. For any plausible posture in front of a screen reclined 30-45°, what the eye sees
  differs from the rendered image by 0.88-1.18× in far/near width and at most 4% in height (our computation, posture
  unverified). Chunithm's screen angle itself is unsourced: SEGA and the press give only "32-inch LCD" and a
  "forward-leaning" cabinet, so Alpha's 45° is **unverified**. A phone flat on a table makes Sekai's far end 4-12%
  narrower than rendered and the track about 20% shorter (iPad: more). The look of each game is in its rendered camera.
- **The in-game cameras.** Chunithm, SDVX (USC) and K-Shoot MANIA (KSM) are each tilted about 70-75° from the image
  plane under their own FOVs (Chunithm 74.2° at 27.5°, USC 71.25° at 60°, KSM 75° at 45°). That similarity hides
  the real differences: at one common 60° FOV the lane angles are Chunithm 56°, Sekai 58°, Arcaea 58°, YARG 63°,
  KSM 70° and SDVX 71°. The visible lane is 3.0 track widths long for Chunithm, 5.0 for Arcaea, 9.3 for Sekai and
  10 for SDVX. SDVX's notes "come from the middle" because its lane is steep and long, so the vanishing point lands on
  screen (0.21H landscape, 0.32H portrait). The screen being "parallel to the user" is not the reason.
- **SDVX's "lane tilts up" is a chart effect that rotates the lane about the judgement line.** It is KSH/KSON
  `zoom_top` (15° per 100 units), USC's `pLanePitch` and VOX `CAM_RotX`. The judgement line stays put on screen. In
  USC's landscape camera, +17° brings the far edge to the top edge. Far/near barely changes on the way (0.106 → 0.122).
- **Prior art for a player-facing tilt:**
  - StepMania's Hallway/Distant (±30° about the field's centre; the receptors move, and visible time changes);
  - YARG's camera presets (FOV, height, distance, pitch; the strikeline moves);
  - D4DJ's judgement-line "depth" (−10 to 10) and lane width (80-150%);
  - TAKUMI³'s stage tilt (+2.0 = upright);
  - Next SEKAI's chart-driven `stageTilt` 0-1.

  None pivots about the judgement line as a player setting. Only the SDVX clones' chart effect does.
- **Approach curves.** Linear in depth (true perspective): Chunithm, SDVX (clones), Arcaea (clones), YARG, StepMania,
  D4DJ (Sonolus, nearly linear). Eased (exponential in screen scale): Project Sekai and Deemo (Sonolus, 1.06^45),
  BanG Dream (Sonolus, 1.1^50 plus an offset) and 22/7 (Sonolus).

**Proposal** (*What this means for us*):

- a **Lane tilt** slider in degrees that pivots about the judgement line, defined at one reference FOV (60°) so a
  degree means the same in every preset;
- its low end, **Top**, fits the far edge to the top edge. For Chunithm, Top is almost exactly today's stretch
  (far/near 0.269 instead of 0.24);
- a FOV-free **Lane length**;
- an **Approach easing** slider: −1 is linear depth, 0 is Sekai's curve exactly, +1 is constant on-screen speed;
- no cabinet-angle simulation;
- an optional cosmetic **kiai tilt**.

Conventions (as in the other notes):

- y is measured from the top in screen heights H, and x in screen widths W, at 16:9 unless stated.
- J is the judgement line's y, F the far edge's y, W the track width at the line, and r the far/near width ratio.
- VP is the vanishing point's y.
- "Lane angle" is the angle between the track plane and the image plane (90° minus the camera's pitch relative to
  the track). 0° is a flat, mania-like stage; larger values are steeper.
- A track width is the width of the playable lanes at the line.

## Scope, evidence and licences

- **Open-source code**, read at pinned commits, cloned to the scratchpad only. Nothing was copied into the repository;
  every number below comes from our own Python re-derivation of the published transforms.
  - USC (MIT) and the KSM chart format specification (CC0).
  - The Sonolus engines for BanG Dream, Project Sekai, Deemo and 22/7 (all MIT, NonSpicyBurrito) and Next SEKAI
    (MIT, Kyle Chang).
  - The Sonolus D4DJ engine (no licence file: numbers only).
  - Old Rhythia / Sound Space Plus (MIT).
- **Existing notes, extended rather than redone:** `game-chunithm.md` (decoded camera), `game-sdvx.md` and
  `game-usc.md` (USC and KSM rigs), `game-sekai.md` (Sonolus geometry and curve), `game-arcaea.md` (clone rig),
  `game-yarg-clonehero.md` (camera presets) and `game-stepmania-etterna.md` (Tilt/Skew mods).
- **Hardware sources:**
  - KONAMI's own announcement (SDVX);
  - press reports (GAME Watch, Famitsu);
  - Wikipedia (JA) and the pixiv dictionary;
  - RemyWiki;
  - a device-size guide for Sekai (kamigame).
- **Could not be read or does not exist:**
  - **No official or credible source gives the screen angle of any cabinet.** We searched SEGA's operator site,
    cabinet drawings, press reports, patents, wikis and home-cabinet builders.
  - SEGA's public 1/100 cabinet drawings (`am.sega.jp/cad`) contain no dimension text, and were not downloaded.
  - RemyWiki's IIDX version pages and silentblue refuse robots.
  - KONAMI's product pages give monitor sizes but no angles.
- **Postures in the screen-angle computation are assumptions, marked (unverified):** eye heights, distances and
  screen heights. The computation's conclusion holds across the range swept.

## Camera and track

### 1. Physical screens: what is sourced

| Game / device | Screen | Orientation and angle | Play posture | Source |
|---|---|---|---|---|
| **Chunithm** (2015-, silver), **NEW and later** (gold, 2021-11-04) | 32-inch LCD; gold model 120 fps (silver 60 fps) | Landscape 16:9. Cabinet described as "前傾の独特なフォルム" (a unique forward-leaning form), the upper half a smoke-black hood; the GROUND SLIDER is a horizontal panel in front of the display. **Screen angle not stated anywhere we could read; Alpha's ~45° is unverified.** | Standing; arms raised into the AIR sensors at elbow-shoulder height | [AA1][AA2][AA3] |
| **Sound Voltex**, cabinets before Valkyrie | 32-inch LCD (KONAMI: "monitor size changed from 32 to 43 inches") | Vertical (portrait) LCD; Konasute renders 1080×1920 portrait. Angle unverified (Alpha: "parallel to the user") | Standing | [AA5][AA7]; `game-sdvx.md` [S6] |
| **Sound Voltex Valkyrie model** (2021-02-17) | 43-inch, 120 Hz; plus a 21.5-inch touch panel below it | Portrait. Angle unverified | Standing | [AA5]; `game-sdvx.md` [S1] |
| **O.N.G.E.K.I.** (2018) | Size and orientation unverified | Enclosed cabinet nicknamed "ホッチキス" (stapler) that blocks the surroundings | **Seated**; lever in the centre, 3 buttons each side | [AA9] |
| **maimai** | Size unverified | Touch screen inside a ring of buttons, the cabinet likened to a washing machine (title only) | Standing | [AA23] |
| **beatmania IIDX** | 16:9 widescreen; LIGHTNING MODEL (2019-12-16) has two screens, the second a touch screen | Upright landscape (unverified angle). Notes are flat (no perspective), so the angle is irrelevant to us | Standing | [AA8] |
| **Project Sekai** | Phones and tablets; a guide recommends 10-11 inches, calls 8-9 inches the size for multi-finger play **with the device on a desk**, and 10-11 inches the limit for thumb play held in the hands | Landscape, 16:9 stage (letterboxed on other aspects, `game-sekai.md`) | Held (thumbs) or flat on a desk (several fingers). The note.com measurements in `game-sekai.md` used an iPad Pro | [AA10]; `game-sekai.md` [K3] |
| **BanG Dream**, **Arcaea** | Phones and tablets | Landscape | Held or flat (unverified: no source found on how players place them) | — |

### 2. Does the screen angle explain the rendered perspective? (our computation)

**Method.**

1. Place the physical screen in 3D: a reclined plane with its bottom edge nearest the player.
2. Put the eye at a plausible position.
3. Project the rendered track trapezoid onto a retina perpendicular to the gaze, with the gaze aimed at the track's
   centre.
4. Compare the apparent far/near width ratio, and the apparent angle between the judgement line and the far edge,
   with the same screen seen square-on from the same distance.

The script is in the scratchpad (`screen_view.py`).

**The general rule.** What matters is δ, the angle between the gaze and the screen's normal, not the screen's tilt.
With δ = 0 (the screen faces the eye) the retina sees exactly the rendered image. For a 32-inch screen seen from
600 mm with the top receding, the Chunithm image (r 0.241) appears as:

| δ (gaze vs screen normal) | 0° | 10° | 20° | 30° | 45° | 60° |
|---|---|---|---|---|---|---|
| Apparent far/near | 0.241 | 0.221 (×0.92) | 0.202 (×0.84) | 0.186 (×0.77) | 0.166 (×0.69) | 0.152 (×0.63) |
| Line to far edge, angle | 28.4° | 28.3° | 27.5° | 25.8° | 21.8° | 16.0° |

**Chunithm at the arcade** (32-inch screen):

- **Assumptions (unverified):** bottom edge 0.95-1.05 m high and 0.30-0.50 m in front of the eye horizontally; eye
  1.45-1.65 m high (a standing adult).
- **Results:**
  - **Reclined 30°:** δ = 0-19°, far/near ×1.00-1.18. A vertical screen seen from above has its top closer to the
    eye, so it *reduces* the perspective.
  - **Reclined 45°:** δ = 0-14°, far/near ×0.88-1.02.
  - The track's apparent height changes by at most 4% in either case.
- **For contrast:** an upright screen seen from above (0°, eye 1.55 m) gives δ 43° and far/near ×1.47. A screen
  reclined 60° gives ×0.86.
- **Conclusion:** a reclined arcade screen is what lets a standing player see the image roughly square-on. "As seen
  at the arcade" and "on an upright PC monitor at eye level" differ by about ±10% in far/near, smaller than the
  difference between our presets and the arcade (Q7's stretch already changes Chunithm's vanishing point from
  −0.079H to −0.29H). It does not matter enough to model.

**Project Sekai flat on a table vs held** (r 0.073; the eye position is unverified):

| Device | Flat, eye 420 mm up and 300 mm back (δ 36°) | Flat, eye 450 mm up and 200 mm back (δ 24°) | Held, square on, 320 mm | Held, 30° off the gaze |
|---|---|---|---|---|
| 6.1-inch phone | far/near ×0.95, line-to-far angle ×0.80 | ×0.96, ×0.90 | ×1.00 | ×0.92, ×0.86 |
| 11-inch iPad | ×0.89, ×0.79 | ×0.92, ×0.90 | ×1.00 | ×0.86, ×0.84 |
| 12.9-inch iPad | ×0.88, ×0.79 | ×0.91, ×0.90 | ×1.00 | ×0.84, ×0.84 |

- Lying flat does make far notes thinner, as Alpha says: the far end gets 4-12% narrower, and everything gets about
  10-20% shorter vertically.
- The rendered far/near of 0.073, the smallest of any reference, is still most of the effect.

**SDVX Valkyrie** (43-inch portrait, assumed vertical, 1.05-2.00 m high; the eye 1.5-1.7 m high and 0.6-0.7 m
away; all unverified):

- δ = 0-19°, and far/near is ×1.00-1.19 of the rendered 0.089.
- The eye is level with the upper half of the screen, where the far end is, so the upright screen slightly
  *weakens* the perspective. Notes still come from mid-screen because the camera puts them there.

### 3. In-game cameras compared

All screen values are at 16:9 unless stated.

- **Native**: the game's own camera.
- **At FOV 60**: the unique camera with a 60° vertical FOV that draws the same trapezoid (`game-chunithm.md`
  explains why any FOV can draw a given trapezoid).
- Solved in closed form by our script (`cam.py`).
- Eye height, eye distance behind the line and lane length are in track widths.

| Game (source) | Native vertical FOV | Native lane angle | Native eye height / distance behind the line | Native visible length | Lane angle at FOV 60 | Length at FOV 60 | J | F | W | r | VP | Approach |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Chunithm (decoded camera) | 27.5° | **74.2°** | 0.89 / 1.70 | 6.17 | **56.2°** | 3.02 | 0.926 | 0.163 (under the HUD frame) | 0.611 | 0.241 | −0.079 | linear |
| SDVX, USC landscape | 60° | **71.3°** | 0.90 / 0.88 | 10 | **71.3°** | 10.0 | 0.941 | 0.284 | 0.435 | 0.106 | +0.206 | linear (clone) |
| SDVX, USC portrait 9:16 (arcade proxy) | 90° | **70.2°** | 0.80 / 0.88 | 12 | 78.2° | 19.9 | 0.707 | 0.354 | 0.810 of the portrait width | 0.089 | +0.320 | linear (clone) |
| K-Shoot MANIA | 45° | **75.0°** | 0.96 / 1.50 | 12.7 | 69.5° | 9.5 | 0.885 | 0.262 | 0.400 | 0.121 | +0.176 | linear |
| Arcaea (clones) | 50° | **63.4°** | 0.53 / 0.53 | 5.88 | 58.2° | 5.0 | 0.857 | 0.070 | 0.849 | 0.119 | −0.036 | linear (clone) |
| YARG Default preset | 55° | **65.9°** | 1.33 / 1.43 | 2.5 | 63.4° | 2.3 | 0.827 | 0.409 (fade from 0.464) | 0.292 | 0.45 | +0.070 | linear |
| StepMania Hallway + Reverse, 4K | 26° (45° horizontal) | ±30° plane rotation | — | — | 13.3° | 1.8 | 0.794 | 0 (screen edge) | 0.288 | 0.80 | −3.2 | linear |
| Project Sekai (Sonolus) | none: a 2D trapezoid | — | — | — | 57.8° (equivalent) | 9.3 | 0.791 | 0.015 | 0.787 | 0.073 | −0.046 | **eased** |
| BanG Dream (Sonolus) | none: a 2D trapezoid | — | — | — | 60.0° (equivalent) | 11.4 | 0.820 | 0.048 | 0.800 | 0.058 | 0.000 (the top edge) | **eased** |
| D4DJ (Sonolus, no licence) | none: a 2D trapezoid | — | — | — | 63.9° (equivalent) | 11.1 | 0.736 | 0.112 | 0.827 | 0.056 | +0.075 | nearly linear |
| O.N.G.E.K.I. | unknown | — | — | — | — | — | — | — | — | — | — | (unverified) |

**Notes on the table.**

- **Confirmed:** Chunithm, USC and KSM are each about 70-75° under their own cameras (`game-chunithm.md`,
  `game-usc.md`). At a common FOV they are not alike. Chunithm's narrow 27.5° lens and short lane (6.2 widths) give a
  wide far end (r 0.24) that the camera aims well above, so its vanishing point is above the screen. SDVX's 60-90°
  lens and 10-12-width lane give a far end a tenth of the near width, with the vanishing point on screen.
- **The 2D engines draw a perspective-consistent trapezoid.**
  - Sekai, BanG Dream and D4DJ in Sonolus have no 3D camera.
  - They place every point by its screen scale s about a vanishing point: x is multiplied by s, and y is at
    VP + s·(J − VP).
    - BanG Dream: `perspectiveLayout`, `shared/src/engine/data/utils.ts:25-35`; the note quad is multiplied by its
      scale, `SingleNote.ts:55`, `VisibleNote.ts:86`.
    - Sekai: `game-sekai.md`.
    - D4DJ: `utils.ts:3-13`.
  - Any such trapezoid is the image of a plane, so the geometry is a true perspective. Only the motion along it
    differs (section *Note approach*). Whether the official Sekai app uses a 3D camera is unknown. Its notes follow
    the exponential curve (`game-sekai.md` [K3]), which a camera moving notes linearly in depth cannot produce.
- **BanG Dream (Sonolus) geometry at 16:9** [AA11] `play/src/engine/playData/archetypes/Initialization.ts:9-51`:
  - The stage is locked to 1334:750. At 16:9 it fills the width; the stage top (the vanishing point) is at the top
    edge, and the stage bottom (the judgement line) is 0.82 of the stage height below it, at 0.820H.
  - The 7 lanes span ±3.5 units of stage width / 8.75, which is 0.800W at the line.
  - The lane starts at s = 0.05 (0.041H), and notes appear at s = 0.058 (0.048H) (`note.ts:1-10`).
- **D4DJ (Sonolus) geometry** [AA14] `play/.../Initialization.ts:10-43`:
  - The vanishing point is at 0.075H and the judgement line at 0.736H by default.
  - The 7 lanes span 0.827W.
  - Notes appear at s = 1/18 (0.112H).
- **Where the camera aims:**
  - Chunithm looks at a point 1.27 track widths past the line, 0.05 widths above the track. The image centre falls
    at 23% of the visible track depth.
  - USC aims so the line sits 27° below the view axis (landscape) or 22.5° below it (portrait).
  - KSM looks 15° down from a fixed eye.
  - Arcaea looks at a point below the track plane, 1.2 lane spans past the line.

  Sources: `game-chunithm.md`, `game-usc.md`, `game-arcaea.md`.
- **Portrait vs landscape** ("different length screens"). SDVX is portrait: its lane occupies only 0.354-0.707H,
  under a banner and above a console, so its notes start mid-screen on a tall display. USC re-aims the same rig for
  a landscape window (line 0.941H, far edge 0.284H). Arcaea interpolates its camera between 4:3 and 16:9. Its far
  edge at 4:3 is 0.173H, with the vanishing point on screen (`game-arcaea.md`).

### 4. What rotating the lane about the judgement line does

The KSH spec (CC0) defines `zoom_top`, for charts from version 167, as rotating "the upper side of the lane with the
judgment line as an axis", at 2400 units per 360° (100 units = 15°). Older charts are limited to ±300 (±45°)
[AA18] `ksh_format.md:372-395`; KSON calls it the same, `kson_format.md:740-750`.

USC implements it as a pitch of the track about its own origin, which is the critical line
(`GetOriginTransform`, `Camera.cpp:86-108`; `lanePitch = PitchScaleFunc(pLanePitch) × 15°`, `:236`). The camera is
unchanged, so the line keeps its screen height and width. USC's `PitchScaleFunc` (`:40-84`) compresses the KSH units
slightly: in landscape 100 units are 12.75° and 300 units are 38.2°.

Our projection of USC's landscape rig under that pitch (positive lifts the far end; reproduced exactly by our generic
camera model at FOV 60):

| Lane pitch | −20° | −10° | −5° | 0 | +5° | +10° | +15° | +17.3° | +20° | +30° |
|---|---|---|---|---|---|---|---|---|---|---|
| Far edge F | 0.562H | 0.425H | 0.356H | 0.284H | 0.209H | 0.129H | 0.043H | **0.000H** | −0.052H | −0.280H |
| Far/near r | 0.101 | 0.102 | 0.104 | 0.106 | 0.109 | 0.114 | 0.119 | 0.122 | 0.126 | 0.145 |
| VP | 0.519H | 0.367H | 0.288H | 0.206H | 0.119H | 0.025H | −0.079H | −0.131H | −0.195H | −0.487H |

The same pivot for the other cameras, solved at each one's native FOV and at the common 60°:

| Camera | Pivot that brings the far edge to the top | r there | VP there | −10° (far edge, r) |
|---|---|---|---|---|
| Chunithm, FOV 27.5 (native) | **+5.7°** | 0.247 | −0.304H | 0.441H, 0.235 |
| Chunithm, FOV 60 | **+10.4°** | 0.269 | −0.341H | 0.299H, 0.224 |
| USC landscape, FOV 60 (native) | **+17.3°** | 0.122 | −0.131H | 0.425H, 0.102 |
| KSM, FOV 60 | +16.1° | 0.138 | −0.142H | — |
| Arcaea, FOV 50 (native) / 60 | +3.4° / +3.9° | 0.122 / 0.124 | −0.12H | 0.257H / 0.232H |
| Sekai (equivalent), FOV 60 | +0.8° | 0.074 | −0.063H | 0.189H, 0.067 |
| BanG Dream (equivalent), FOV 60 | +2.5° | 0.059 | −0.052H | — |
| YARG Default, FOV 60 | +40.7° | 0.654 | −1.57H | — (a short lane cannot reach the top without going nearly flat) |

What this shows:

- **Pivoting about the line moves the far edge a lot and the far width a little**, because the lane is long compared
  with the camera's distance. This is the SDVX "lane lifts to approach from the top" effect Alpha describes.
- **The per-degree effect depends on the FOV.** Chunithm needs +5.7° at its own 27.5° and +10.4° at 60°. A slider
  in degrees is only consistent across presets if it is defined at one reference FOV.
- **Today's "stretch to the top" (Q7 (a)) is almost exactly a pivot.** With J, W and r held fixed it gives VP
  −0.292H. A +10.4° pivot gives r 0.269 and VP −0.341H, and a +5.7° native pivot gives r 0.247 and VP −0.304H.

### 5. Three ways to make notes enter at the top, compared

F = 0 in every row. Near-line speed is |dy/dd| at the line = (J − F)/r, in screen heights per visible time; it
scales a note's on-screen thickness at the line.

| Method | Chunithm: r, VP, lean, near-line speed | Voltex (USC landscape): r, VP, lean, near-line speed |
|---|---|---|
| Arcade, unchanged (far edge not at the top) | 0.241, −0.079H, 28.4°, 3.17 (F 0.163) | 0.106, +0.206H, 27.8°, 6.2 (F 0.284) |
| Stretch, today (keep r) | 0.24, −0.292H, 24.0°, 3.86 (+22%) | 0.106, −0.112H, 20.2°, 8.88 (+43%) |
| **Pivot about the line** (FOV 60: +10.4° / +17.3°) | **0.269, −0.341H, 23.2°, 3.44 (+9%)** | **0.122, −0.131H, 19.8°, 7.71 (+24%)** |
| Lengthen the lane (×3.71 / impossible) | 0.079, −0.079H, 28.4°, 11.7 (+270%) | impossible: the vanishing point is on screen |

The pivot keeps the arcade's motion closest of the three methods that reach the top.

### 6. Player-facing settings that change the lane's angle, length or camera (prior art)

| Game | Setting | Range, step, default | What it changes | Judgement line | Source |
|---|---|---|---|---|---|
| StepMania 5 / Etterna | Perspective: Overhead, Hallway, Distant, Incoming, Space; raw Tilt and Skew | Named mods are tilt ∓1 and skew 0/1; any float via mod strings; default Overhead | Rotates the whole field ∓30° per unit **about its centre line**, zoom ×(1 − 0.1·\|tilt\|), skew moves the vanishing point | **Moves** (0.800 → 0.794H for Hallway); visible time **changes** (+38%) because speed is px per beat | `game-stepmania-etterna.md` |
| YARG | Camera preset: FOV, height, distance, pitch, curve, fade, orthographic | FOV 40-150° (default 55), height and distance ±40, pitch 0-180° (24.12), curve −3 to 3 (0.5), fade 0-5 (1.25) | Every camera parameter; 12 named presets | **Moves** with the camera | `game-yarg-clonehero.md` |
| YARG / Clone Hero | Highway length | YARG 0.001-10, default 1 | Far end distance; keeps note speed, so visible time changes | Stays | `game-yarg-clonehero.md` |
| D4DJ Groovy Mix (official) | 判定ラインの高さ (judgement line height, described as its "depth") | −10 to 10, step 1, default 0 (presets: Another −6, Stage −5) | Moves the line | **Moves** | [AA15] |
| D4DJ (official) | 広さ (lane width) | 80-150%, step 10%, default 100% (Pro 90%) | Lane width | Width changes | [AA15] |
| D4DJ (Sonolus recreation) | Judgement Line Height, Width, Lane Length | −10 to 10, step 1 (1.39% H per step); 0.8-1.5 (step 0.01); 0-1 | The line slides along the converging lanes towards the vanishing point, its width ∝ its distance from it; lane length hides the far part | Moves | [AA14] `options.ts:48-100`, `Initialization.ts:12-16` |
| TAKUMI³ (mobile) | ステージ傾き (stage tilt), ステージ高さ (stage height = judgement bar position) | Range, step and default unverified; "+2.0 is fully perpendicular to the judgement bar" (upright); negative height raises the bar | Lane tilt between perspective and upright; bar position | Unverified pivot | [AA19] |
| Next SEKAI (Sonolus) | `stageTilt` in chart camera events (not a player option) | 0-1, default 1; 0 = "a flat vertical stage" | Widths interpolate linearly: w(depth) = tilt·depth + (1 − tilt)·0.537. The approach blends from Sekai's curve to linear on screen | y stays, but its **width shrinks** to 0.537 at tilt 0 | [AA13] `layout.py:42-55,932-991,1081-1093,1220-1225`; `docs/level_data.md:56` |
| Next SEKAI (Sonolus) | Alternative Approach Curve (player, advanced) | Toggle, default off | A partly linear-depth approach | Stays | [AA13] `options.py:285-291`, `layout.py:1037-1045` |
| SDVX (arcade, EG) | — | No camera, lane or tilt option in the EG play options list | Lane pitch, zoom, roll, spin, split come from charts only | — | [AA21] |
| USC | ForcePortrait, EnableFancyHighwayRoll | Toggles, default off / on | Portrait camera in a landscape window; slam-roll timing | — | [AA17] `GameConfig.cpp:74,249` |
| Chunithm, Arcaea, Project Sekai (official), Quaver, osu!mania, DJMAX, IIDX | — | No lane-angle, FOV or camera option | — | — | `game-chunithm.md`, `game-arcaea.md`, [AA22], other notes |
| Rhythia (Sound Space Plus; not a lane game) | FOV, spawn distance, approach rate, parallax | FOV 70, spawn distance 40, approach rate 40 (= 1 s visible), parallax 6.5 (defaults) | Camera FOV and how far away notes spawn | n/a | [AA20] `scripts/Rhythia.gd:563-606` |

- **Nobody offers a player-facing pivot about the judgement line.** The only pivot about the line is the SDVX
  clones' chart effect.
- **Every player-facing tilt that exists moves the judgement line, its width, or the visible time.** StepMania,
  YARG and D4DJ move the line or its width; StepMania and the highway-length settings change the visible time.
- **For us the pivot is the natural fit.** It keeps SPEC 2's "never move the judgement line" and SPEC 3's visible
  time.
- RoBeats, Malody, BanG Dream (official) and Clone Hero's camera were searched without finding documented
  lane-angle options (unverified negatives).

### 7. Alpha's own frames, measured (2026-11-08)

Alpha attached three screenshots to the request: a Chunithm play (1206×668, cropped from a video), an official
Project Sekai play (1068×600, 16:9) and a Konasute SDVX EG training screen (814×1441, the portrait image scaled by
0.754). They are the project's first real SDVX and Sekai frames. The images were measured in the session's scratchpad
and are not kept in the repository. Method: lane edges found per row by colour and brightness (the Chunithm field's dark
edge against the background, Sekai's light lane lines, SDVX's three white BT lane lines and the cyan/magenta rails at
the BT area's edges); straight lines fitted through 6-11 rows each; the judgement line from a brightness profile across
it. Every fitted edge passes within about 2 px of its points.

| Quantity | Chunithm | Project Sekai | SDVX EG (portrait) |
|---|---|---|---|
| Judgement line y | 0.937H (626/668; the crop may cut a few rows) | 0.792H (centre of the judgement box, 453-497) | **0.707H** (1018/1441) |
| Far edge y | 0.165H (under the HUD frame) | 0.013H (lit lane top at 8 px; the lane lines reach the top edge) | **0.393H** (566/1441) |
| Vanishing point y | −0.079H | −0.052H | **0.369H** |
| Width at the line | 0.687W dark field incl. side margins; the judgement line itself 0.618W | 0.793W (12 cells) | **0.766 of the portrait width** (BT area), 0.433H |
| Far/near width | 0.24 | 0.078 at 8 px | **0.071** (BT area) |
| Whole highway / BT area | — | — | 1.34 (rails' outer edges; USC uses 1.5) |

- **Chunithm** agrees with `game-chunithm.md` (far edge 0.163H, far/near 0.24) to within the crop.
- **Sekai** agrees with `game-sekai.md`'s Sonolus geometry (0.791H, top edge, VP −0.046H, 0.787W, 0.073) to within
  about 1% of H, which confirms that the Sonolus engine reproduces the official stage.
- **SDVX EG** matches USC portrait's critical line exactly (0.707H) but its lane is steeper and longer: far edge 0.393H
  instead of 0.354H, vanishing point 0.369H instead of 0.320H, far/near 0.071 instead of 0.089, and a BT area 0.766 of
  the portrait width instead of 0.540. At the 60° reference FOV this is a lane angle of **81.4°** and a lane 11.5
  screen heights (26 BT-area widths) long; USC landscape is 71.3°.

## Note approach and speed

**Which games move notes linearly in depth.** Here s(u) is the screen scale about the vanishing point (1 at the
line) and u is the elapsed fraction of the approach (0 when the note appears, 1 at the hit). Stats are from our
script (`approach.py`):

| Game (source) | Curve | Far scale | Line/far on-screen speed | Time spent in the near half of the on-screen track | Same far scale, linear depth |
|---|---|---|---|---|---|
| Chunithm (decoded, measured) | linear depth: 1/s linear in u | 0.241 | 17.2× | 19.4% | — |
| SDVX (USC, KSM) | linear depth | 0.106 | 89× | 9.6% | — |
| Arcaea (clones) | linear depth | 0.119 | 71× | 10.6% | — |
| YARG, StepMania | linear along the plane | — | 5× (YARG) | — | — |
| D4DJ (Sonolus) | depth from 12 to 1 linear in u, then s remapped from [1/12, 1] to [1/18, 1] (`utils.ts:70-85`) | 0.056 | 144× | 7.7% | 324×, 5.3% |
| **Project Sekai** (Sonolus; consistent with the official game) | s = 1.06^(45(u − 1)) (`game-sekai.md`) | 0.073 | **13.8×** | **23.8%** | 189×, 6.8% |
| Deemo (Sonolus) | the same as Sekai (`shared/src/engine/data/note.ts:5-6`) | 0.073 | 13.8× | 23.8% | — |
| **BanG Dream** (Sonolus) | s = 0.05 + 0.95 · 1.1^(50(u − 1)) (`shared/src/engine/data/note.ts:9-10`) | 0.058 | **117×** | **14.4%** | 296×, 5.5% |
| 22/7 (Sonolus) | s = 0.1 + 0.9 · 71.77^(u − 1) (`note.ts:5-6`) | 0.113 | 72× | 15.9% | 79×, 10.1% |

- The official curves of BanG Dream, Deemo, 22/7 and D4DJ are unverified. These are fan recreations; only Sekai's
  has been checked against frame captures (`game-sekai.md` [K3]).
- **BanG Dream's speed** in the recreation: T = (12 − s)/2 s up to speed 11, then (16 − s)/10 s
  (`play/src/engine/playData/note.ts:7-11`), already in `topic-speed-scales.md`.
- **The eased family.** Sekai's curve is the exponential case of a family. For a far scale r and a parameter p:
  - (s^p − 1)/p is linear in time; p = 0 means ln s is linear.
  - **p = −1** is linear depth (true perspective).
  - **p = 0** is Sekai's exponential. With r = 0.073 it is exactly the pjsekai curve, since 0.073 = 1.06^−45.
  - **p = +1** is constant on-screen speed (a flat stage).
  - The line/far on-screen speed ratio is r^(p − 1), from 1/r² down to 1.

  | Far scale r | p = −1 | −0.5 | 0 | +0.5 | +1 |
  |---|---|---|---|---|---|
  | 0.241 (Chunithm) | 17.2×, 19.4% | 8.5×, 26.0% | 4.1×, 33.5% | 2.0×, 41.7% | 1×, 50% |
  | 0.106 (Voltex) | 89×, 9.6% | 29×, 16.6% | 9.4×, 26.4% | 3.1×, 38.0% | 1×, 50% |
  | 0.073 (Sekai) | 188×, 6.8% | 51×, 13.5% | 13.7×, 23.8% | 3.7×, 36.7% | 1×, 50% |

  (Line/far speed, then time spent in the near half of the track's screen height.) BanG Dream's curve sits near
  p ≈ −0.65 by its speed ratio.
- **Tilt does not change visible time.** All of these define speed along the track (`topic-speed.md`). Pivoting the
  lane only changes the projection, so a tilt slider leaves SPEC 3's visible time alone. StepMania is the exception:
  its speed is screen pixels per beat, so tilting changes visible time.

## Judgement line and receptors

| Setting | Line height on screen | Line width |
|---|---|---|
| KSH `zoom_top` / USC lane pitch (chart) | fixed | fixed |
| StepMania Tilt | moves (about the field's centre) | changes slightly (zoom ×(1 − 0.1·\|tilt\|)) |
| YARG camera presets | moves | changes |
| D4DJ line height | moves along the lanes | ∝ its distance from the vanishing point |
| Next SEKAI `stageTilt` | fixed | 1 → 0.537 as tilt goes 1 → 0 |
| **Proposed Lane tilt / Lane length** | **fixed** | **fixed** |

## Lane covers / walls

- With the far edge at the top, a pivot to Top also moves where the wall's 0% sits. The wall stays a fraction of
  track depth (DECISIONS "Wall is a fraction of track depth"), so it follows the lane with no change.
- Sekai's Note Start Point (0-100) and D4DJ's Lane Length hide the far part of the lane and keep speed
  (`game-sekai.md`, [AA14]). The two are the same idea as our wall.

## Notes and holds (shapes, rendering)

- Notes have a fixed thickness in depth (SPEC 6). Their height on screen at the line is thickness × (J − F)/r, so it
  follows the tilt:
  - Chunithm at 2.55%: 8.1% H at the arcade pose, 8.8% at Top by pivot, 9.8% with today's stretch.
  - Voltex at 1.25%: 7.8% H at the arcade pose, 9.6% at Top by pivot, 11.1% with today's stretch.
- No change to SPEC 6 is needed. The thickness setting already exists per preset.

## Key count and lane layout

No change. Every camera here keeps one width for the track; D4DJ's width (80-150%) is our track width setting.

## Feedback (judgement display, hit effects, lane lighting)

**SDVX's "tilt when the chorus hits" is chart data, not an automatic effect.**

- KSH/KSON give charts `zoom_top` (pivot about the line), `zoom_bottom` (move the near end towards the camera),
  `zoom_side`, `center_split` (100 units = one lane of margin), `tilt` (laser roll: normal, bigger, biggest, keep,
  zero, or a manual float) and slam spins [AA18].
- The VOX equivalents are `CAM_RotX` and `CAM_Radi` (`game-usc.md`).
- No kiai or chorus flag exists in either format.
- Whether official charts place these effects mostly at choruses is unverified; it matches player descriptions but
  we found no source.
- **Only the pitch is compatible with SPEC 2.** USC's roll rotates the judgement line, and `zoom_bottom` moves it.
  The pitch about the line moves neither.
- Our existing kiai sway (a shear about the line, DECISIONS 2026-11-04) is the same family of effect.

## What this means for us

All of these are visual settings: they never change judgement, scoring or visible time (invariants 1 and 3, SPEC 3).
Values below are for 16:9 and the reference FOV of 60°.

### A1. "Lane tilt": a pivot about the judgement line, in degrees (SPEC 2; DECISIONS "Camera is set in screen terms")

**Keep the four screen-terms numbers** (judgement line, far edge, track width, far/near) as the stored model and the
only input to `HighwayProjection`. Add **Lane tilt** as a derived control that rewrites the far edge and the far/near
width together, along the path a real camera would take. This is the "reverse" that DECISIONS already allows ("a
tilt/FOV pair can be added later as a derived input").

**What it means:**

- The lane's angle to the screen, for a camera with a 60° vertical FOV.
- Changing it rotates the lane about the judgement line with the camera fixed, as KSH `zoom_top` does (100 units =
  15°). The judgement line's height and width never move.

**Why one reference FOV:**

- The same pivot moves the far edge about twice as far under 60° as under Chunithm's 27.5° (section 4).
- A fixed reference makes 10° mean the same in every preset.
- 60° is USC landscape's FOV, DxLib's default and Seaurchin's, and SPEC 2's original default.

**Range, step and display:**

- From **Top** (the angle at which the far edge reaches the top edge) to 85°, step 0.5°.
- Show the resulting "lane ends N% below the top" beside it.
- Steeper angles end the lane lower, towards the SDVX look. Values below Top are not offered, because
  `HighwayLayout` already clamps the far edge to the screen.

**Conversion** (a pure function, unit-testable like `HighwayProjection`):

1. Inputs: f = 1/tan(30°) = 1.732, the aspect A = width/height, and the current J, F, W, r (y from the top).
2. VP = J + (F − J)/(1 − r); pitch θ = atan((1 − 2·VP)/f).
3. z = f/(2·A·W); y = (1 − 2J)·z/f.
4. Eye height h = z·sin θ − y·cos θ; eye distance D = z·cos θ + y·sin θ.
5. Lane length L = z·(1/r − 1)/cos θ.
6. Lane angle = 90° − θ.
7. Pivot by Δ (positive lifts the far end): the far point is (Y, Z) = (−h + L·sin Δ, −D − L·cos Δ).
8. Its depth is z_F = −Y·sin θ − Z·cos θ and its height y_F = Y·cos θ − Z·sin θ.
9. F′ = (1 − f·y_F/z_F)/2 and r′ = z/z_F.
10. Check: with the arcade inputs this reproduces every row of section 4 (USC's own rig to 0.001).

**Visible time:** unchanged (SPEC 3). Only how depth lands on screen changes.

### A2. "Top" is the tilt's low end, and replaces the stretch (SPEC 2; QUESTIONS Q7)

A pivot to Top is physically consistent and differs from today's stretch by only a little (section 5):

- **Chunithm:** r 0.24 → 0.269, VP −0.29H → −0.34H, near-line speed 3.86 → 3.44 H per visible time, against the
  arcade's 3.17.
- **Voltex:** r 0.106 → 0.122, near-line speed 8.88 → 7.71, against the arcade's 6.2.

Recommend that Q7's option (a) be implemented as "Lane tilt = Top". It keeps the arcade's motion closer, and makes
"stretched to the top" and "SDVX's lane lifted" the same control. Q7's other options become slider positions:

- (b), exact geometry, is the arcade angle;
- (c) is the arcade angle plus a frame over the top 16%.

### A3. "Lane length" (SPEC 2)

**What it does:** stretches the visible lane, in track widths, with the camera and the judgement line fixed. The far
edge slides along the lanes towards the vanishing point. This is the "lane looks proportionally longer" of SDVX
(10 widths, against Chunithm's 3.0 at FOV 60).

**Formula** (independent of the FOV): with VP fixed, r′ = 1/(1 + (1/r − 1)·λ) and F′ = J + (VP − J)·(1 − r′).

**Range:** λ from ×0.5 to ×4 of the preset, step 0.05, default ×1. Show the length in track widths at the reference
FOV: Chunithm 3.0, Voltex 10.0, Sekai 9.3.

**Interaction with Top:**

- If the far edge would pass the top, it clamps at Top. With the tilt at Top, a longer lane lowers the angle that
  Top needs.
- Lengthening is not a way to reach the top. Chunithm needs ×3.71 and r 0.079 to get there, and SDVX cannot,
  because its vanishing point is on screen. Lengthening is a look in its own right.

### A4. No FOV slider (SPEC 2)

With the four screen numbers fixed, the FOV does not change the image of the track. It only changes how strongly the
tilt acts, and how upright things (the wall panel) are drawn. Keep it a hidden constant (60°). An advanced "Camera
FOV" (20-90°) can come later if Alpha wants Chunithm's own 27.5° behaviour.

### A5. Approach easing (SPEC 3; QUESTIONS Q17)

If Alpha answers Q17 with (b), use the one-parameter family above rather than a two-way switch:

- **Range:** "Approach easing" from −1 to +1, step 0.05.
- **Positions:** −1 is linear depth, the default for Chunithm and Voltex. 0 is Sekai's exponential, the Sekai
  preset. +1 is constant on-screen speed.

**Implementation:** a monotone remap of the depth fraction after mania's own positions (SV included), with s(0) = r
and s(1) = 1. Hit times and visible time stay exact, and SV keeps working. Sekai's 3.4%-of-time note thickness
(`game-sekai.md`) suggests drawing thickness in time rather than depth under easing (decide when building).

**Reference values:** BanG Dream ≈ −0.65; D4DJ ≈ linear.

### A6. No simulated cabinet screen angle

Players see their own monitor roughly square-on, and a reclined arcade screen seen by a standing player is also
roughly square-on. The difference is ±10% in far/near and under 4% in height (section 2).

A "cabinet angle" option would imitate a viewing mismatch nobody at the arcade experiences. Its effect (far/near
×0.84 at δ 20°) is already reachable with the far/near setting. Not recommended.

### A7. Optional kiai tilt (SPEC 2: cosmetic motion, off by default, never moves the line)

- **Effect:** an SDVX-style lane pitch on kiai sections.
- **Range:** −15° to +15°, default 0 (off).
- **Timing:** eased in and out over one swing, exactly like `KiaiSway` (300 ms minimum).
- **Pivot:** about the judgement line (A1), so the line, judgements and key area stay still.
- **Clamp:** positive values only act while the far edge is below the top. On a lane at Top, only negative values
  (leaning back, the far end dropping into view) do anything.
- **Suggested magnitude:** 8° when enabled, our own value. SDVX charts commonly use `zoom_top` in hundreds of units
  (100 = 15°; USC 12.75°); the typical size is unverified.
- **Not offered:** USC's roll and `zoom_bottom`, which move the line.

### A8. Per-preset values

| Preset | J (line) | W | Lane tilt at rest | Far edge at rest | r at rest | Top: tilt, r | Lane length (FOV 60) | Approach easing | Today |
|---|---|---|---|---|---|---|---|---|---|
| **Chunithm** | 0.926 | 0.611 | 56.2° (arcade) | 0.163H | 0.241 | **45.9°, 0.269** | 3.0 widths | −1 (linear) | stretched: F 0, r 0.24 (= lane angle 47.5°, VP −0.29H) |
| **Voltex** (USC landscape) | 0.941 | 0.435 | 71.3° (arcade proxy) | 0.284H | 0.106 | **53.9°, 0.122** | 10.0 widths | −1 (linear) | stretched: F 0, r 0.106 (= 54.8°, VP −0.11H) |
| **Sekai** (proposed, Q17) | 0.791 | 0.787 | 57.8° (native: notes appear at 0.015H) | 0.015H | 0.073 | 57.0°, 0.074 | 9.3 widths | **0** (Sekai's curve) | none |
| Arcaea (reference only; Q19 said no preset) | 0.857 | 0.849 | 58.2° | 0.070H | 0.119 | 54.4°, 0.124 | 5.0 | −1 | — |
| BanG Dream (reference) | 0.820 | 0.800 | 60.0° | 0.048H | 0.058 | 57.5°, 0.059 | 11.4 | ≈ −0.65 | — |
| KSM (reference) | 0.885 | 0.400 | 69.5° | 0.262H | 0.121 | 53.4°, 0.138 | 9.5 | −1 | — |
| YARG Default (reference) | 0.827 | 0.292 | 63.4° | 0.409H | 0.45 | 22.8°, 0.654 (not useful) | 2.3 | −1 | — |

**Defaults to propose:**

- **Chunithm:** Top (45.9°), as Q7 (a) asks, now by pivot.
- **Voltex:** Q7 is still open. The research still recommends the faithful 71.3° (notes from the middle, as Alpha
  describes SDVX), which departs from SPEC 2's "notes enter at the top edge" and so needs Alpha's answer. Until then
  the default stays at Top (53.9°), and the faithful look is one slider move away.
- **Sekai:** its native angle, which is already at the top.

**SPEC 2's old numbers:** "tilt 55°, FOV 60°" happens to sit within 1.2° of Chunithm's arcade angle at FOV 60
(56.2°).

### A9. Questions for Alpha (for `QUESTIONS.md`, if the main session agrees)

1. Should "Top" become a pivot of the lane (Chunithm far/near 0.27) instead of today's stretch (0.24)? The look is
   nearly the same, and near notes rush 11% less.
2. For Voltex, should the default be notes from the middle (arcade, 71°) or from the top (54°)? This is Q7's Voltex
   half, now one slider.
3. Should the eased approach be a slider (−1 linear … 0 Sekai … +1 flat speed) rather than Q17's on/off switch?

## Unverified / open questions

- **Cabinets:**
  - No source gives the screen angle of any cabinet: Chunithm (Alpha's ~45°), SDVX standard or Valkyrie, maimai,
    O.N.G.E.K.I. or IIDX.
  - O.N.G.E.K.I.'s and maimai's screen sizes and orientations are unverified.
  - IIDX LIGHTNING MODEL's screen size is unverified.
- **The posture numbers in section 2** (eye heights, screen heights and distances) are plausible assumptions, not
  measurements. The conclusion (±10-18% at most for a screen that roughly faces the eye) holds across the range swept.
- **The official app geometries** of BanG Dream, D4DJ and Deemo, and their approach curves, are the Sonolus
  recreations' values. Only Sekai's curve is checked against the official game.
- **O.N.G.E.K.I.'s in-game camera:** no decoded or open-source data found.
- **TAKUMI³'s stage tilt range, step, default and pivot.**
- **Typical SDVX chart `zoom_top` sizes,** and whether they cluster at choruses.
- **Whether the arcade SDVX matches USC's portrait camera** (inherited from `game-sdvx.md`).

## Sources

Repository references are to the commit read. Repositories with no licence file were read for numbers only.

- [AA1] GAME Watch, CHUNITHM location-test report: https://game.watch.impress.co.jp/docs/news/677824.html.
  "ディスプレイは32インチの液晶"; "前傾の独特なフォルムだが上半分は仕切りのフードと電光掲示板"; the GROUND SLIDER in front of the
  display; AIR STRINGS at elbow-shoulder height.
- [AA2] Wikipedia (JA), チュウニズム: https://ja.wikipedia.org/wiki/チュウニズム. 32-inch LCD; gold model (NEW,
  2021-11-04) "液晶が60fpsから120fpsに向上"; silver model.
- [AA3] pixiv 百科事典, CHUNITHM: https://dic.pixiv.net/a/CHUNITHM. Gold model's 120 fps monitor.
- [AA4] SEGA AM 1/100 cabinet drawings: https://am.sega.jp/cad/ and https://am.sega.jp/cad/video_game/22659 (CHUNITHM
  NEW). They exist; they contain no dimension text, and were not downloaded or measured.
- [AA5] KONAMI, SOUND VOLTEX EXCEED GEAR and Valkyrie model announcement (2021-02-17):
  https://www.konami.com/amusement/corporate/ja/topics/20210217/. "モニターサイズが32インチから43インチへ"; 120 Hz; 21.5-inch
  touch panel.
- [AA6] RemyWiki, SOUND VOLTEX EXCEED GEAR: https://remywiki.com/AC_SDVX_EG. Touchscreen under the main screen;
  120 Hz (also `game-sdvx.md` [S1]).
- [AA7] Highway Games, Sound Voltex Booth listing:
  https://highwaygames.com/arcade-machines/sound-voltex-booth-11915/print.html. "the vertical LCD screen".
- [AA8] RemyWiki, What is beatmania IIDX: https://remywiki.com/Beatmania_IIDX. 16:9 widescreen; LIGHTNING MODEL (2019-12-16)
  has two screens, the touch screen replacing the effector.
- [AA9] Famitsu, O.N.G.E.K.I. announcement (2017-12): https://www.famitsu.com/news/201712/17148322.html. Seated, enclosed
  cabinet ("ホッチキス"); lever in the centre, 3 buttons on each side.
- [AA10] kamigame, Project Sekai device guide: https://kamigame.jp/gaming/35939.html. "机に置いてプレイする多本指スタイル"
  for 8-9 inches; 10-11 inches recommended and the limit for thumb play held in the hands.
- [AA11] sonolus-bandori-engine, MIT, © 2023 NonSpicyBurrito, commit 783c7b8:
  https://github.com/NonSpicyBurrito/sonolus-bandori-engine.
  - `shared/src/engine/data/note.ts:1-10`: lane t 0.05, approach curve.
  - `shared/src/engine/data/utils.ts:25-35`: `perspectiveLayout`.
  - `play/src/engine/playData/archetypes/Initialization.ts:9-51`: stage transform.
  - `play/src/engine/playData/note.ts:7-11`: duration.
  - `VisibleNote.ts:86`; `SingleNote.ts:55`.
  - `shared/src/engine/configuration/options.ts`: no camera options.
- [AA12] sonolus-pjsekai-engine, MIT, commit 0f6f37f: https://github.com/NonSpicyBurrito/sonolus-pjsekai-engine.
  Geometry and curve as in `game-sekai.md` [P1]; its options list has no stage-angle option.
- [AA13] sonolus-next-sekai-engine, MIT, © 2025 Kyle Chang, commit bfe6144:
  https://github.com/Next-SEKAI/sonolus-next-sekai-engine.
  - `sekai/lib/layout.py:42-55`: tilt constants.
  - `:932-991`: `refresh_layout`.
  - `:1037-1045`: approach curves.
  - `:1081-1093`: `approach_at_tilt`.
  - `:1220-1225`: `width_factor_at_tilt`.
  - `sekai/lib/options.py:285-291`: Alternative Approach Curve.
  - `docs/level_data.md:56`: `stageTilt` "1 is the default and 0 means a flat vertical stage".
- [AA14] sonolus-d4dj-engine, **no licence file (numbers only)**, commit aab7733:
  https://github.com/gorenganhunter/sonolus-d4dj-engine.
  - `shared/src/engine/data/utils.ts:3-13`: `perspectiveLayout`.
  - `:70-85`: approach.
  - `play/src/engine/playData/archetypes/Initialization.ts:10-43`: geometry and duration.
  - `shared/src/engine/configuration/options.ts:48-100`: line height, lane length, width.
- [AA15] Gamerch D4DJ wiki, live settings: https://gamerch.com/d4dj/128735. 判定ラインの高さ −10 to 10, step 1
  (presets 0 / 0 / 0 / −6 / −5), described as the line's depth; 広さ 80-150%, step 10%; speed 1.0-11.5.
- [AA16] sonolus-deemo-engine (MIT, commit 4eb6e5f), `shared/src/engine/data/note.ts:5-6`,
  `play/.../Initialization.ts:8-18`; sonolus-nanaon-engine (22/7, MIT, commit 9ff850a), `shared/src/engine/data/note.ts:5-6`.
  https://github.com/NonSpicyBurrito/sonolus-deemo-engine, https://github.com/NonSpicyBurrito/sonolus-nanaon-engine.
- [AA17] Unnamed SDVX Clone, MIT, commit 6c3a33f: https://github.com/Drewol/unnamed-sdvx-clone.
  - `Main/src/Camera.cpp:40-84`: `PitchScaleFunc`.
  - `:86-108`: `GetOriginTransform`, pivot at the critical line.
  - `:236-256`: lane pitch applied.
  - `:357-390`: `CreateRenderState`.
  - `Main/include/Camera.hpp:18-19,124-131`: pitch unit 15°, FOVs 60/90, offsets.
  - `Main/src/GameConfig.cpp:74,249`: player options.
- [AA18] K-Shoot MANIA chart format specification, CC0, commit b984509: https://github.com/kshootmania/ksm-chart-format.
  - `ksh_format.md:357-395`: `tilt`; `zoom_top` "rotates the upper side of the lane with the judgment line as an
    axis", 2400 per 360°; range ±300 before version 167; `center_split`.
  - `kson_format.md:740-750`.
- [AA19] TAKUMI³:
  - wikiwiki ルール, https://wikiwiki.jp/takumi_game/ルール: "ステージの傾きを調整できる。プラスにするほど傾きは垂直に近づき、+2.0で
    完全に判定バーと垂直になる"; ステージ高さ.
  - altema, https://altema.jp/takumic: "レーンの傾きや背景の濃さ、判定バーの位置".
  - App Store, https://apps.apple.com/app/id1491836718: "Lane height and tilt angle adjustment".
- [AA20] Rhythia / Sound Space Plus (old client), MIT, commit c0d6df1: https://github.com/Rhythia/sound-space-plus.
  `scripts/Rhythia.gd:563-606`: approach rate 40, spawn distance 40, parallax 6.5, FOV 70.
- [AA21] voltexes.com, EXCEED GEAR play options guide: https://voltexes.com/exceed-gear-play-options-guide/. No lane,
  camera or tilt option.
- [AA22] SEGA, Project Sekai support, "What are the different show settings":
  https://support-pjsekai.sega.com/hc/en-us/articles/16601102191257. No lane-angle, line-position or perspective
  setting.
- [AA23] Famitsu, maimai でらっくす BUDDiES (2023-09; title only): https://www.famitsu.com/news/202309/05315723.html.
  "洗濯機みたいなアーケード用音楽ゲーム".
- Cross-referenced notes: `game-chunithm.md` (decoded camera [C21], screenshot geometry), `game-sdvx.md` and
  `game-usc.md` (USC/KSM rigs, chart camera), `game-sekai.md` (Sonolus geometry and curve, [K3] captures),
  `game-arcaea.md` (clone rig), `game-yarg-clonehero.md` (camera presets), `game-stepmania-etterna.md` (Tilt/Skew),
  `topic-speed.md`, `topic-speed-scales.md`.
- Our scripts (scratchpad only, not committed): `cam.py` (trapezoid ↔ camera, pivot), `usc_pitch.py` (USC rig under
  lane pitch), `tilt_table.py`, `presets.py`, `screen_view.py` (physical screen vs eye), `approach.py` (curves).
