# Highway presets by game

Last checked: 2026-11-08

## Summary

The highway preset dropdown is meant to list games, and each entry sets that game's look. This note collects every
figure such a preset needs, in the code's units (`HighwaySettings`, `HighwayPresets.Get`). Most figures come from the
per-game notes and are cited by item id. The gaps were filled here:

- **IIDX geometry.** beatoraja's default 7K skin (GPL-3.0, numbers only) puts the line 19.4% above the bottom, the
  lane up to the top edge, and notes at 2.1% of the lane. The arcade lane is 723 px tall in the cover maths [I3]. If
  that is 1080p and the lane starts at the top edge, the arcade line is 33% up. That is an estimate.
- **Etterna.** Both shipped themes put the receptors at ±164 px, so the line is 15.8% above the bottom. The default
  noteskin's taps are 58 px tall, 14.4% of the line-to-top distance. The default speed is C400.
- **BanG Dream!** From the MIT Sonolus recreation:
  - line 18.0% up;
  - notes appear 4.8% below the top;
  - 7 lanes are 0.800W at the line, with far/near 0.058;
  - notes are 9.5% H at the line, 2.6% of visible time;
  - an eased approach, as in Sekai.
- **DJMAX.** Only estimates: the line about 30% up (from the fan-measured 754 px lane) and the gear about 0.26W.

Chunithm and Voltex keep their measured values. No note says they are wrong.

**Proposed dropdown entries:**

- Chunithm, Voltex, IIDX, DJMAX, Quaver, StepMania / Etterna and osu!mania;
- Sekai and BanG Dream!, marked as waiting on Q17 (eased approach);
- no Arcaea (Q19 answered (c)).

## Scope, evidence and units

**Units are the code's** (`osu.Game.Rulesets.ManiaOrtho/Highway/HighwaySettings.cs`):

- heights are fractions of screen height H, widths fractions of a 16:9 screen width W;
- `JudgementLineHeight` is measured above the bottom;
- `FarEdgeHeight` is measured below the top (0 = the far edge stretched to the top edge, QUESTIONS Q7 default (a));
- `TrackWidth` is the width at the line (`WidthMode.Track`), and `LaneWidth` one lane's width (`WidthMode.Lane`);
- `NoteThickness` is a fraction of track depth.

**2D games become a straight-on track:** `CameraMode = ParallelTilt`, `FarWidthRatio = 1`. Depth is then linear on
screen (`HighwayProjection`: c = 0), so **note thickness = note height / distance from the line to where notes
appear**. Stretching that distance to the top edge (Q7) keeps the ratio and scales the note.

**Confidence labels:**

| Label | Meaning |
|---|---|
| measured | pixels of a real frame or asset, by us or a cited measurer |
| documented | read from code or an official or wiki source |
| derived | our algebra on documented values |
| estimated | a defensible guess, with its reasoning |
| unverified | no source |

**New evidence in this note:**

- **beatoraja** (exch-bms2), **GPL-3.0, read for numbers only**, commit `9340253a`:
  - `skin/default/play/play7main.lua:116-118,150-157,529-537,324-334,481` (layout);
  - `src/bms/player/beatoraja/skin/json/JsonPlaySkinObjectLoader.java:80-91`: note height is the source image
    height × the screen scale.
- **Etterna** (etternagame/etterna), **MIT**, commit `2d706147`:
  - `Themes/_fallback/metrics.ini:30-31,113,686-693`;
  - `Themes/Til Death/metrics.ini:107-108`, `Themes/Rebirth/metrics.ini:73-74`;
  - `Themes/Til Death/Scripts/01 player_config.lua:87-88`, `Themes/Rebirth/Scripts/01 player_config.lua:165`;
  - `NoteSkins/dance/DivideByZero/_Down Tap Note 1x8.png`: 64×512, 8 frames, opaque box 58×58 inside each 64×64
    frame (alpha bounding box, measured with PIL; nothing copied).
- **StepMania 5** (stepmania/stepmania), **MIT**, commit `825467b`: `Themes/_fallback/metrics.ini:32-33,1127-1128`.
- **sonolus-bandori-engine** (NonSpicyBurrito), **MIT**, commit `783c7b8`:
  - `play/src/engine/playData/archetypes/Initialization.ts:10-52`;
  - `shared/src/engine/data/lane.ts`, `shared/src/engine/data/note.ts`, `shared/src/engine/data/utils.ts`
    (`perspectiveLayout`);
  - `play/src/engine/playData/archetypes/notes/visibleNotes/singleNotes/SingleNote.ts:27-37`.
- **ez2djmax-speed-calculator** [D15], MIT, `app/core/calculator.ts:5-7`: DJMAX lane 754, bar 560, commented width 502.

**Could not be read:** KONAMI's eagate pages, iidx.org, Steam, RemyWiki images, YouTube and every image host were
refused by this environment's proxy. **No IIDX, DJMAX or SDVX arcade frame was measured.**

## Camera and track

| Game | Camera | Far/near width | Source, confidence |
|---|---|---|---|
| Chunithm | perspective | **0.24** | `game-chunithm.md` P1; measured |
| SDVX | perspective | **0.106** (USC landscape; arcade proxy 0.089) | `game-sdvx.md` P1; documented (clone code), arcade unverified |
| beatmania IIDX | 2D, straight-on | 1 | `game-iidx.md` "Camera and track"; documented |
| DJMAX RESPECT V | 2D, straight-on | 1 | `game-djmax.md` "Camera and track"; documented |
| Quaver | 2D, straight-on | 1 | `game-quaver.md` "Camera and track"; documented (code) |
| StepMania / Etterna | 2D by default (Overhead); optional Hallway 0.80 | 1 (Hallway 0.80) | `game-stepmania-etterna.md` S5; documented (code) |
| osu!mania | 2D, straight-on | 1 | `game-osu-mania.md` "Camera and track"; documented (code) |
| *Project Sekai* | perspective, **eased** approach | 0.073 where notes appear | `game-sekai.md` K2; derived (MIT engine fits official captures) |
| *BanG Dream!* | perspective, **eased** approach | **0.058** where notes appear (0.05 at the lane top) | this note; derived (MIT recreation), official unverified |
| *Arcaea* | perspective, linear | 0.119 | `game-arcaea.md` R2; derived (fan clones) |

**BanG Dream! camera, derived.** `perspectiveLayout` scales x by y, so y is the screen scale s. The skin transform maps
y = 0 to the stage top t and y = 1 to the line.

- At 16:9 the stage keeps 1334:750: t = 0.9995 of the half-height, so the vanishing point is at the top edge, and
  the stage is 0.82 H tall (`lowPosition`).
- The lane is drawn from y = 0.05 (`lane.t`), which is 0.041H from the top.
- Notes spawn at s = lerp(0.05, 1, 1.1^−50) = 0.058, which is 0.048H from the top.
- The outer edge leans 40.9° from vertical, close to Sekai's 39.9°.
- The approach is s = lerp(0.05, 1, 1.1^(50(u − 1))), with u the share of the duration gone. That is exponential in
  time, like Sekai's, so it needs Q17's eased approach. Linear depth at r = 0.058 would crawl far away, as
  `game-sekai.md` K2 shows for r = 0.073.

## Judgement line and receptors

Height above the bottom of the screen, 16:9.

| Game | Line height | Source, confidence |
|---|---|---|
| Chunithm | **0.074** | `game-chunithm.md` P1; measured |
| SDVX | **0.059** (USC landscape; KSM 0.114; the arcade's portrait 0.29 sits over a console) | `game-sdvx.md` P1; documented, arcade unverified |
| IIDX, arcade | **≈ 0.33** | **estimated**: the lane is 723 px in KONAMI's cover maths [I3 in `game-iidx.md`]. At 1080p that is 0.669H, so the line is 0.331 up if the lane starts at the top edge. Both the resolution and the lane top are inferences. |
| IIDX, beatoraja default skin | 0.194 | documented: notes dst y = 140, h = 580 in a 720-high, bottom-origin skin (`play7main.lua:529-537`) |
| DJMAX RESPECT V | **≈ 0.30** | **estimated**: the fan-measured visible lane is 754 px at 1080p [D15][SS21 in `topic-speed-scales.md`], 0.698H, so 0.302 up if it ends at the top edge (unverified) |
| Quaver | **0.120** at 4K, 0.129 at 7K | `game-quaver.md` QV4; documented (default Bar skin) |
| Etterna (Til Death and Rebirth, both shipped themes) | **0.158** | documented: receptors at ±164 of 480, Reverse puts them at 404/480 = 0.842H |
| StepMania 5 (`_fallback`) | 0.200 | `game-stepmania-etterna.md` "Projected screen geometry"; documented |
| osu!mania, argon (lazer default skin) | **0.182** | `game-osu-mania.md` "Judgement line and receptors"; documented |
| osu!mania, triangles / legacy default | 0.143 / 0.1625 | same; documented |
| *Project Sekai* | 0.209 | `game-sekai.md` K2; derived |
| *BanG Dream!* | 0.180 | this note; derived (`lowPosition` 0.82) |
| *Arcaea* | 0.143 | `game-arcaea.md` R2; derived |

All values are within the slider's 0-0.40 (`HighwaySettings.MAX_JUDGEMENT_LINE_HEIGHT`).

## Width and lanes

"Per lane" is a fraction of W per lane at the line. Our maps have **no scratch lane**: IIDX's scratch, DJMAX's side
tracks and L/R notes, and SDVX's FX lanes and lasers have no mania column. Maps flagged `SpecialStyle` (N+1) get the
1 | N−1 split instead (QUESTIONS Q24).

| Game | Mode | Value | 4K lane | 7K lane | Special lanes | Source, confidence |
|---|---|---|---|---|---|---|
| Chunithm | Track | **0.611** (inner 16 cells; rails 1/16 each side) | 0.153 | 0.087 | none: one field for every chart | `game-chunithm.md` P1, P4; measured |
| SDVX | Track | **0.435** (whole highway; BT area 0.290) | 0.109 | 0.062 | 4 BT; FX = 2 BT wide; laser margins 1/6 each side | `game-sdvx.md` P1, P2; documented (USC) |
| IIDX | Lane | **≈ 0.036** (key-lane mean) | track 0.143 | track 0.250 | white key 0.039, black key 0.031, scratch 0.055; 7 keys = 0.250W, with scratch 0.305W | documented for beatoraja's default skin (50 / 40 / 70 px of 1280, `play7main.lua:150-157`; `topic-wide-keys.md` "IIDX"); arcade proportions unverified |
| DJMAX | Track | **≈ 0.26** | 0.065 | 0.037 | 5B centre lane on two keys; side tracks cover half the gear; 8B L/R notes are thin bars over the six lanes | **estimated**: the calculator's commented `DJMAX_WIDTH = 502` [D15] is 0.261W if it is 1080p gear pixels. The mode it was measured in, and whether 4B and 6B gears are equally wide, are unverified (`game-djmax.md` "Key count") |
| Quaver | per key count | stage **0.249** at 4K, 0.297 at 7K | 0.062 | 0.042 | equal lanes, no gaps; centre marked by colour only | `game-quaver.md` "Key count and lane layout"; documented |
| StepMania / Etterna | Lane | **0.0749** (64 px of 854) | track 0.300 | track 0.525 (Etterna uniform; SM5 0.56 with 3 \| 1 \| 3 gaps) | none | `game-stepmania-etterna.md` "Key count"; documented |
| osu!mania argon | Lane | **0.044** (60 of 1366; pitch 0.045 with spacing) | track 0.182 | track 0.362 | odd-count centre column twice as wide (`topic-wide-keys.md` "Presets") | `game-osu-mania.md` "Key count"; documented |
| osu!mania triangles / legacy default | Lane | 0.0586 / 0.0351 | 0.240 / 0.141 | 0.413 / 0.246 | triangles' odd centre 70 instead of 80 | same; documented |
| *Project Sekai* | Track | 0.787 (12 cells) | 0.197 | 0.112 | none | `game-sekai.md` K2; derived |
| *BanG Dream!* | Track | **0.800** (7 lanes, 0.114 each) | 0.200 | 0.114 | none; borders 0.25 lane outside | this note; derived |
| *Arcaea* | Track | 0.849 | 0.212 | 0.121 | none | `game-arcaea.md` R2; derived |

**Two games fit neither width mode.** Quaver's stage grows by 19% from 4K to 7K and argon's by 99%. Our two modes
are "same for every key count" (Track) and "proportional to the key count" (Lane).

- Quaver is closer to Track: 0.27 is within ±9% of both key counts. Lane at 0.052 misses by 16-23%.
- Argon is Lane, apart from its double centre column, which the highway's own equal lanes do not draw.

## Notes and holds (thickness)

| Game | Thickness | Note width / lane | Source, confidence |
|---|---|---|---|
| Chunithm | **0.0255** of track depth | 98% | `game-chunithm.md` P5; measured (decode + frame) |
| SDVX | **0.0125** (USC chip) | 100% (USC) | `game-sdvx.md` P4; documented (clone), arcade unverified |
| IIDX | **≈ 0.021** | 100% | documented for beatoraja (12 px of a 580 px lane, `play7main.lua:324-326`, `JsonPlaySkinObjectLoader.java:89-91`); arcade unverified |
| DJMAX | ≈ 0.035 | 100% (unverified) | **estimated**, no frame measured: between IIDX's 0.021 and Quaver's 0.049 |
| Quaver | **0.049** at 4K, 0.034 at 7K | 100% | `game-quaver.md` "Notes"; measured (default skin images) |
| Etterna | **0.144** (58 px tap of a 404 px field) | 91% | this note; measured (default DivideByZero noteskin) |
| StepMania 5 | ≈ 0.167 (64 px arrow frame of 384 px) | ≈ 100% | estimated: the frame size, not the arrow's opaque height |
| osu!mania argon | **0.070** (42 of 598) | 100% | `game-osu-mania.md` "Notes"; documented |
| osu!mania triangles | 0.018 (12 of 658) | 100% | same; documented |
| *Project Sekai* | 3.4% of visible time (7.4% H at the line) | full lane | `game-sekai.md` K2; derived |
| *BanG Dream!* | 2.6% of visible time (9.5% H at the line), centred on the line | 100% | this note; derived (`note.h` 0.11625 / 2; scaled with s) |
| *Arcaea* | 0.015 at the line, growing ×5.17 to the far end | 89% | `game-arcaea.md` R3; derived |

**The StepMania / Etterna value exceeds the slider.** The "Note thickness" range is 0.005-0.08
(`ManiaOrthoRulesetConfigManager.cs:213`), and Etterna's faithful 0.144 is outside it. The preset can use 0.08 and say
so, or the range can grow. StepMania's notes are huge by design: 64 px of a 480 px screen.

## Where notes appear

| Game | Where notes enter | We use |
|---|---|---|
| Chunithm | 0.163H, under the HUD frame (`game-chunithm.md` "Where notes appear") | top edge (Q7 (a)) |
| SDVX | 0.284H, a visible narrow far edge (`game-sdvx.md` P1) | top edge (Q7; the note recommends 0.284) |
| IIDX | the top edge (beatoraja: 140 + 580 = 720, documented); arcade inferred | top edge |
| DJMAX | unverified; the top edge assumed above | top edge |
| Quaver | the top edge (`game-quaver.md` "Scroll speed to on-screen speed") | top edge |
| StepMania / Etterna | the top edge (draw distance one screen height beyond the receptors, `game-stepmania-etterna.md`) | top edge |
| osu!mania argon | 30 units (0.039H) below the top, the stage padding (`game-osu-mania.md`) | top edge, or 0.039 to be exact |
| *Sekai* | 0.015H | 0.015 (`game-sekai.md` K2) |
| *BanG Dream!* | 0.048H (lane art from 0.041H) | 0.048 |
| *Arcaea* | 0.070H | 0.070 |

## Lane covers / walls

| Game | Cover and scale | Range | Default | Moving it keeps | Our `WallScale`, keep-time | Source |
|---|---|---|---|---|---|---|
| Chunithm | field wall, steps | 0-16 (ours 0-14) | 0 | speed | Chunithm, keep speed (but Q8: SPEC keeps time) | `game-chunithm.md` P3; `topic-covers.md` C1 |
| SDVX | none in the arcade (USC adds fades) | — | — | — | Percent, 0 | `game-sdvx.md` "Lane covers" |
| IIDX | SUDDEN+ white number, 1/1000 of the lane | WN 42-999 | off (Beginner Option); beatoraja on at 200 | visible time (FHS) | Iidx, keep visible time | `game-iidx.md` 1-2, "Options" |
| DJMAX | none; FADER presets (FADE IN ≈ top 33%) | — | off | — | Percent, 0 (`FarFade` is the nearest look, `game-djmax.md` R1) | `game-djmax.md` R1 |
| Quaver | top cover, % of the **screen height** from the top | 0-75% | off (25% when on) | speed | Lane, keep speed | `game-quaver.md` QV5 |
| Etterna | theme lane cover (sudden/hidden), height in px | px | off (height 10) | speed | Lane, keep speed | `topic-covers.md`; this note (`player_config.lua`) |
| osu!mania | Cover **mod**, share of the lane | 0.2-0.8 | 0.5 when the mod is on | speed | Lane, 0 (a mod, not a setting: invariant 3) | `game-osu-mania.md` "Lane covers" |
| *Sekai* | Note Start Point | 0-100 | 0 (unverified) | speed | Lane | `game-sekai.md` K4 |
| *BanG Dream!* | the engine's stage cover, lerp(lane top, line, v) | 0-1 | 0 | speed | Lane | this note (`Stage.ts:166-178`); official unverified |
| *Arcaea* | none | — | — | — | Percent, 0 | `game-arcaea.md` "Lane covers" |

Every preset's wall default stays **0**: every game's own default is off.

## Note approach and speed

Speed scales already exist (`Highway/SpeedScale.cs`, `topic-speed-scales.md`). Times are from appearing to the line.

| Game | Scale (enum) | Range, step | Default | Common | Formula | Confidence |
|---|---|---|---|---|---|---|
| Chunithm | `Chunithm` | 1.00-15.00 by 0.25, then 16-19 | **6.0** = 914 ms | 7.5-8.5 = 731-645 ms; 8.00-11.50 = 686-477 ms | 5486 / s | documented |
| SDVX | `Sdvx` | 1.00-10.00 by 0.01 | unverified | 6.50-8.50 = 738-565 ms | 4800 / L | setting documented; formula derived, ±7% |
| IIDX | `Iidx` (green number) | NHS GN 1200-260; ours 150-1200 | unverified (NHS default level not found) | GN 250-330 = 417-550 ms | GN × 5/3 | documented |
| DJMAX | `Djmax` | 1.0-9.0 by 0.1 | **2.0** = 1346 ms | 4.0-7.0 = 673-385 ms | 2693 / s | default documented; formula derived |
| Quaver | `Quaver` | 5.0-100.0 by 0.1 | **15.0** = 902 ms | unverified | 13528 / S (4K) | documented |
| StepMania / Etterna | `StepMania` (CMod) | any | Etterna **C400** = 900 ms by our 360,000 / C (947 ms with Etterna's ±164 receptors); SM5 1x (XMod, BPM-relative) | unverified | 360,000 / C (SM5 `_fallback`); 378,750 / C (Etterna themes) | documented (`metrics.ini:30`; SM5 `:32`) |
| osu!mania | `Mania` | 1-40 by 0.1 | **8** = 1436 ms label (1469 argon) | Alpha 29 = 405 ms (argon) | 11485 / s × (768 − H) / 643.2 | documented |
| *Project Sekai* | `Sekai` | 1.0-12.0 by 0.01 | 5.0 = 2369 ms | 10.0-11.6 = 741-398 ms | 350 + 3650((12 − s)/11)^1.31 | derived |
| *BanG Dream!* | `BanGDream` | 1.0-12.0 by 0.1 | 5.0 = 3500 ms | 10-11 = 1000-500 ms | (12 − s) × 500, then (16 − s) × 100 | derived |
| *Arcaea* | `Arcaea` | 1.0-6.5 by 0.1 | unverified | "4+" = 833 ms or less | 3333 / s | measured |

Sources: `topic-speed-scales.md` summary table; `game-iidx.md` "Green number"; `game-quaver.md` "Scroll speed to
on-screen speed"; `game-osu-mania.md` "Scroll speed to time range"; this note for Etterna's default.

## What this means for us

### PG1. Dropdown entries and values (SPEC 2, 3, 4, 5, 6)

The `HighwaySettings` each entry would set. Wall 0 everywhere. A game's "keep visible time" follows its wall scale
(`WallScales.KeepsVisibleTime`).

| Preset | Camera | Far/near | Line | Far edge | Width mode, value | Thickness | Wall scale | Speed scale |
|---|---|---|---|---|---|---|---|---|
| **Chunithm** (keep) | Perspective | 0.24 | 0.074 | 0 | Track 0.61 | 0.0255 | Chunithm | Chunithm |
| **Voltex** (keep) | Perspective | 0.106 | 0.059 | 0 | Track 0.435 | 0.0125 | Percent | Sdvx |
| **IIDX** | ParallelTilt | 1 | **0.33** (est.; beatoraja 0.19) | 0 | Lane 0.036 | 0.021 | Iidx | Iidx |
| **DJMAX** | ParallelTilt | 1 | **0.30** (est.) | 0 | Track 0.26 (est.) | 0.035 (est.) | Percent | Djmax |
| **Quaver** | ParallelTilt | 1 | 0.12 | 0 | Track 0.25 (4K exact; 0.27 splits 4K and 7K) | 0.049 (4K; 7K 0.034) | Lane | Quaver |
| **StepMania / Etterna** | ParallelTilt | 1 | 0.158 | 0 | Lane 0.075 | 0.08 (slider cap; faithful 0.144) | Lane | StepMania |
| **osu!mania** | ParallelTilt | 1 | 0.182 | 0 (or 0.039) | Lane 0.044 | 0.070 | Lane | Mania |
| *Sekai* (Q17) | Perspective + eased | 0.073 | 0.209 | 0.015 | Track 0.787 | 3.4% of time | Lane | Sekai |
| *BanG Dream!* (Q17) | Perspective + eased | 0.058 | 0.180 | 0.048 | Track 0.800 | 2.6% of time | Lane | BanGDream |
| *Arcaea* (Q19: no) | — | 0.119 | 0.143 | 0.070 | Track 0.849 | 0.015 → ×5.17 | Percent | Arcaea |

- **osu!mania.** The existing **Classic** preset (Flat, mania's own stage) is the exact osu!mania look. A
  straight-on "osu!mania" entry only matters for the highway's own notes and track. One option is to list Classic
  under the name "osu!mania" and drop the separate entry.
- **IIDX line.** 0.33 is the arcade estimate and 0.19 the documented skin. One IIDX frame settles it (see Unverified).
  Either way, a ported skin's key panel fills the space below the line ("Panel", Q16).
- **Speed and wall scales** are display choices (`SpeedScale` and `WallScale` doc comments). Setting them with the
  preset is the "game's own units" Alpha asked for. They change no visible time.

### PG2. Corrections and conflicts for the main session

1. **`WallScale.Lane` describes Quaver's cover as a share of the lane.** Quaver measures it from the screen top as a
   share of the screen height (`game-quaver.md` "Lane covers / walls"). With the 4K line at 0.880H, Quaver 25% is
   28.4% of the lane. Either relabel it "% of the lane (osu!mania)", or convert by the line height for Quaver.
2. **`SpeedScales.ToMilliseconds(StepMania)` uses 360,000 / C**, which is SM5's `_fallback` with receptors at ±144.
   Etterna's two shipped themes use ±164: 378,750 / C, 5.2% longer (`game-stepmania-etterna.md` already notes the
   +5%). The label could say "StepMania" or the constant could follow the preset.
3. **Note thickness maximum 0.08** is below Etterna's 0.144 (above).
4. **Neither width mode fits Quaver**, and argon's double centre column is not drawn (above).
5. **No conflict with the Chunithm and Voltex presets.** Their values match `game-chunithm.md` P1/P5 and
   `game-sdvx.md` P1/P4. Q7 still decides whether Voltex's far edge should be 0.284.

## Unverified

- **IIDX arcade:** the screen resolution of the 723 px lane, where the lane starts, the lane widths, the note height,
  the NHS default level. One INFINITAS or Lightning-model 1080p frame would settle the line, widths and notes.
  Measure the red line's y, the lane top, the white, black and scratch lane widths, and one note's height.
- **DJMAX:** the line height, the gear width per mode, the note height, and whether the visible lane reaches the top
  edge. No frame was measured; the line and width are estimates from a fan calculator's constants.
- **SDVX:** the arcade geometry (`game-sdvx.md`) and the default lane speed.
- **StepMania 5:** the default arrow's opaque height (only the 64 px frame size was used).
- **BanG Dream!:** whether the official game matches the recreation's geometry and durations, and its cover option.
- **Typical speeds** of Quaver and Etterna players; **Arcaea**'s default speed.

## Sources

- Per-game notes, by item id: `game-chunithm.md` P1, P3, P4, P5; `game-sdvx.md` P1, P2, P4; `game-iidx.md` 1-2 and
  [I3]; `game-djmax.md` R1 and [D15]; `game-quaver.md` QV4, QV5; `game-stepmania-etterna.md` S5; `game-osu-mania.md`
  "Judgement line and receptors", "Key count", "Notes"; `game-sekai.md` K2, K4; `game-arcaea.md` R2, R3;
  `topic-speed-scales.md` summary table and [SS20], [SS21], [SS23]; `topic-covers.md` C1; `topic-wide-keys.md` "IIDX",
  "Presets".
- beatoraja, GPL-3.0 (numbers only, nothing copied), commit 9340253a866a2f0091d44c6fa9de7637ce229ef9:
  https://github.com/exch-bms2/beatoraja/blob/9340253a866a2f0091d44c6fa9de7637ce229ef9/skin/default/play/play7main.lua
  (skin 1280×720; lanes x 20, width 390; lane widths 50 / 40 / 70; notes dst y 140, h 580; note images 12 px tall;
  lane cover 390×580) and
  https://github.com/exch-bms2/beatoraja/blob/9340253a866a2f0091d44c6fa9de7637ce229ef9/src/bms/player/beatoraja/skin/json/JsonPlaySkinObjectLoader.java
  (lines 80-91: note height = image height × screen scale).
- Etterna, MIT, commit 2d706147cc4bcf95a8ceb24dc4f4129be207f803: https://github.com/etternagame/etterna
  (`Themes/_fallback/metrics.ini:30-31` `DefaultModifiers="C400"`, `DefaultNoteSkinName="DivideByZero"`; `:113`
  `ArrowSpacing=64`; `:686-687` receptors ±144; `Themes/Til Death/metrics.ini:107-108` and
  `Themes/Rebirth/metrics.ini:73-74` receptors ±164; `Themes/Til Death/Scripts/01 player_config.lua:87-88` lane cover
  off, height 10; `NoteSkins/dance/DivideByZero/_Down Tap Note 1x8.png` 58×58 opaque per 64×64 frame).
- StepMania, MIT, commit 825467b: https://github.com/stepmania/stepmania (`Themes/_fallback/metrics.ini:32-33`
  `DefaultModifiers="1x"`, noteskin "default"; `:1127-1128` receptors ±144).
- sonolus-bandori-engine, MIT, commit 783c7b817bd8490ca496d7a57bce98dae077f9f7:
  https://github.com/NonSpicyBurrito/sonolus-bandori-engine (`Initialization.ts:10-52` target aspect 1334/750,
  `lowPosition` 0.82, lane unit = stage width / 8.75; `lane.ts` t 0.05, b 1; `note.ts` h 0.11625 / 2, approach
  lerp(0.05, 1, 1.1^(50(u − 1))); `utils.ts` `perspectiveLayout` x × y; `SingleNote.ts:27-37` full-lane notes centred on
  the line; `Stage.ts:166-178` stage cover).
- ez2djmax-speed-calculator, MIT, commit 6544d2eb52977ec3d9131ca05850534200ce4098:
  https://github.com/johypark97/ez2djmax-speed-calculator/blob/6544d2eb52977ec3d9131ca05850534200ce4098/app/core/calculator.ts
  (`DJMAX_HEIGHT = 754`, `DJMAX_BAR = 560`, commented `DJMAX_WIDTH = 502`).
- [I3] まさお, IIDX gear-change simulator note (2025-08-21): https://note.com/masaoblue/n/n02ecb7426a76. Lane "treated
  as 723 px"; covers 0-723 px; no resolution or screen position stated (read 2026-11-08).
- This repository (MIT): `osu.Game.Rulesets.ManiaOrtho/Highway/HighwaySettings.cs` (`HighwayPresets.Get`),
  `Highway/SpeedScale.cs`, `Highway/WallScale.cs`, `Highway/HighwayProjection.cs`,
  `Configuration/ManiaOrthoRulesetConfigManager.cs:207-235` (slider ranges).
