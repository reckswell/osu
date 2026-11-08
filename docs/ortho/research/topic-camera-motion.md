# Camera motion during play: what SDVX's lane does, what other games do at a chorus, and what kiai could do

Last checked: 2026-11-08

## Summary

Alpha asked how rhythm games move their playfield during play, above all how Sound Voltex (SDVX) "tilts" the lane when
the chorus hits, so that we can offer optional cosmetic motion in kiai sections (SPEC 2). osu!mania maps give us only
kiai sections, timing points and notes.

1. **SDVX's lane has eleven kinds of motion** (table in *Camera and track*). Only two are automatic: the roll toward
   the chart's lasers, and the shake on every slam. Everything else is placed by the chart author: pitch (`zoom_top`),
   dolly (`zoom_bottom`), sideways shift, split, manual tilt, and the spins and swings fired by hit slams. No source and
   neither open-source clone moves the camera because of a chorus, a BPM change or a section start.
2. **Pitch about the critical line (`zoom_top`) is the only SDVX motion that leaves the line completely still.** That
   is true in both clones. The dolly keeps the line's centre but scales its width. Roll, spin, shift, swing, split and
   slam shake all move the line on screen. A normal 10° roll tilts the line 8° and slides it 6.6% of the screen width
   (W), while the far end barely moves.
3. **Official charts move the camera for whole sections.** We counted 124 KSH conversions of official charts:
   - The camera is off its rest pose 59% of the chart's time.
   - The usual pose is **zoomed out** (`zoom_bottom` median −60), **with the lane pitched up** (`zoom_top` median
     +25) for about half of that time.
   - A pose is held for a median of 31 beats (about 8 bars, 10 s) and ramps in over a median of 3 beats (0.9 s).
   - 60% of these moves start on a bar line.

   This is the "tilt when the chorus hits" that Alpha sees. The link to choruses is our inference from the lengths;
   KSH files do not mark choruses.
4. **On screen (USC's landscape camera), the median pose:**
   - raises the far edge 19% of its distance to the top;
   - keeps the far end's pixel width;
   - narrows the line by 23%, so the far/near ratio grows ×1.30.

   The lane looks "proportionally longer", as Alpha described.
5. **No other game checked moves its field automatically at a chorus.**
   - osu!'s kiai flashes hit objects on each beat and fires star fountains when it starts.
   - BanG Dream!'s fever fires sprinklers.
   - Camera motion is either written into the chart (SDVX, Arcaea, StepMania attacks) or set off by the player (YARG
     star power, slams).
6. **Proposal**, all optional and off by default:
   - **"Kiai lift"**: an SDVX-style pitch about the judgement line that raises the far edge toward the top and widens
     the far end, so notes reach the line at the same on-screen speed. Suggested strength 30%, over 3-beat ramps.
   - Keep **Kiai sway**.
   - Optionally an osu!-style **kiai flash**.
   - Rejected: SDVX's zoom-out, roll, spins and shakes, since each moves the judgement line or the lanes at it.

## Scope, evidence and licences

**Code read (ideas and numbers only; nothing copied):**

| Source | Licence | Commit | Used for |
|---|---|---|---|
| USC [U1] | MIT | `6c3a33f` | Roll, pitch, zoom, spin and shake maths |
| K-Shoot MANIA v2 [K1] | MIT | `1de5843` | The same. Newer than the `37afba9` in `game-usc.md`; the camera files did not change in ways that matter here |
| KSH/kson chart format specification [F1] | CC0 | `b984509` | What each chart key means |
| vox2ksh specs [V1] | MIT | `a412d08` | VOX (arcade) camera controls and a model of the arcade tilt read from the game's DLL |
| ArcCreate [A1] | GPL-3.0, read only | `a00f94c` | Arcaea `camera()` events |
| StepMania 5 [S1] | MIT | `825467b` | Effect mods |
| YARG [Y1] | LGPL-3.0 | `8e5c4a8` | Animation settings |
| osu! | MIT (this repo) | — | Kiai effects |

**Confidence of the vox2ksh model.** Its `specs/evidence/README.md` describes the evidence folder as LLM-generated. Its
DLL-derived numbers are therefore **low confidence**; we use them only where the two clones agree with them.

**Chart counts.** Each KSH file was streamed into memory from GitHub, parsed, and dropped. No chart was saved, and
only aggregate numbers are recorded here. The parser:

- places events by measure subdivision, honouring `beat=` and `t=`;
- treats graph values (`zoom_*`) as linear between points, as the spec says;
- measures time through the tempo map.

| Set | Charts | KSH `ver` | What it is |
|---|---|---|---|
| vox2ksh reference conversions [C4] | 69 | 160-171 | Hand conversions of official charts |
| TedTheRed/exceed-gear-converts [C1] | 48 | 167 | Work-in-progress conversions of EXCEED GEAR charts, 2019 |
| TsFreddie/KSHConvertFix [C2] | 7 | 167 | Fixed conversions |
| **Official total** | **124** | | |
| GRI51/ksh_chart [C3], comparison only | 90 | 171 | One author's original fan charts |

Two caveats apply to the counts:

- Conversions are human transcriptions in KSM units. The vox2ksh regression gives `CAM_RotX` 1.0 ≈ `zoom_top` 140
  and `CAM_Radi` 1.0 ≈ `zoom_bottom` −125 [V1 `camera.md:15-18`] (low confidence).
- Some `tilt=zero` lines are a workaround for KSM's early tilt, not arcade behaviour [V1 `camera.md:22-40`].

**Screen numbers** come from our re-implementation of USC's landscape camera, the same one as in `game-usc.md`. It is a
throwaway script in the scratchpad, not committed. **No arcade frame or video was measured.**

## Camera and track

### SDVX's moves at a glance

Units:

- USC stores roll in turns. The KSH `zoom_*` keys are integers that USC divides by 100 [U1 `BeatmapFromKSH.cpp:816-829`].
- "Line" means the critical (judgement) line.
- W and H are the screen's width and height, from our USC landscape projection.

| Move | KSH key (VOX control) | Trigger | Size and timing (USC / KSM) | Line on screen |
|---|---|---|---|---|
| Laser roll ("tilt") | laser positions; type `tilt=normal/bigger/biggest/keep_*/zero` (`#TILT MODE INFO`) | **automatic**, from the lasers under the line | 10° × 1 / 1.75 / 2.5. Rolls in at a constant 40°/s (USC) or 45°/s (KSM), so a full tilt takes about 0.25 s. Returns at about 10°/s, so about 1.1 s | Rotates and slides: 8° and 6.6% W at 10° |
| Manual tilt | `tilt=<float>` (`Tilt`) | chart | 10° per unit, linear between points. USC chases it at 100°/s unless the change is immediate. KSM cross-fades auto to manual in 40 ms | As the roll |
| Pitch | `zoom_top` (`CAM_RotX`) | chart | USC: remapped × 15° per unit, so 100 = 12.7° in landscape. KSM: 15° per 100, with the rise scaled by 1/2.5 | **Fixed** |
| Dolly | `zoom_bottom` (`CAM_Radi`) | chart | USC: the track moves along the line of sight to the line. KSM: the near edge moves toward the camera | Centre fixed; width ×1.65 at +100, ×0.66 at −100 |
| Shift | `zoom_side` | chart | 100 = 0.36 track widths sideways (both clones) | Slides 15.6% W per 100 |
| Split | `center_split` (`Morphing2`) | chart | 100 opens a one-lane gap at the centre | The two halves part |
| Spin | `@(` `@)` on a slam (VOX roll types) | chart marker plus a **hit** slam | One full turn, then a recovery. Line wobbles ±20° | The track spins; the line wobbles |
| Half spin | `@<` `@>` | chart marker plus a hit slam | KSM: 60° and back, then −15° and back. USC: a damped sine peaking at about 57° | The track swings; the line wobbles |
| Swing | `S<` `S>` | chart marker plus a hit slam | KSM: a sideways sine, 3 one-way swings by default, squared decay | Slides |
| Slam shake | every slam | **automatic**, on a hit slam | USC: 1° of yaw per full-width slam, gone in 83 ms. KSM: a 150 ms jolt, peak 2.4% of the track width | Jolts about 0.7-1% W |
| Lane hide / lane offset | `LaneY` (VOX only) | chart | — | — |

### Laser roll in detail

- **Target.** Each laser adds its cursor position: the left laser adds x, the right laser adds x − 1. The sum is
  clamped to ±1 and multiplied by 10° (MAX_ROLL_ANGLE) times the tilt type's scale [U1 `Camera.hpp:21`,
  `Camera.cpp:392-407`; K1 `HighwayTiltAuto.cpp:20-51`; `GameDefines.hpp:142`].
  - So the lane tilts toward the lasers' mean position.
  - The scales are normal 1.0, bigger 1.75 and biggest 2.5 [F1 `kson_format.md:667-689`; U1 `Game.cpp:2409-2427`].
- **Looking ahead.** Both clones start rolling toward a laser that begins within 2 beats [U1 `Scoring.cpp:356-367`;
  K1 `HighwayTiltAuto.cpp:37-47`, half a measure]. vox2ksh says the arcade does not (low confidence) [V1
  `camera.md:22-28`].
- **Rate in.**
  - USC moves the line's roll at a constant 40°/s, and the highway follows at 40°/s × the scale [U1
    `Camera.cpp:121-171`]. A full tilt of any type therefore takes 0.25 s.
  - KSM moves at 4.5 tilt units/s (45°/s) and slows linearly within the last 1° [K1 `HighwayTiltAuto.cpp:57-78`].
  - The arcade model gives 3.75 units/s (37.5°/s), and "bigger" follows 2.375 × the tilt at 90°/s [V1
    `camera.md:49-52`]. That is low confidence and disagrees with KSM's 1.75.
- **Rate back to flat.** Returning is 4-5 times slower than leaving, in every source:
  - USC: 10°/s, and 5°/s within the last 1° ("slow tilt", [U1 `Camera.cpp:128-132`]), about 1.1 s from 10°.
  - KSM: 9°/s, with a 5°/s floor [K1 `HighwayTiltAuto.cpp:9-14,68-75`].
  - Arcade model: 9.4°/s [V1 `camera.md:51`].
- **Keep.** `keep_*` types update the tilt only to a larger value of the same sign [F1 `kson_format.md:680`; U1
  `Camera.cpp:394-406`; K1 `HighwayTiltAuto.cpp:108-120`]. Official charts almost never use keep: VOX tilt mode 0 is
  used "for almost all recent charts" [V1 `vox_format.md:105-112`], and only 1 of our 124 conversions has `keep_`.
- **Slam roll.** A slam adds its end position to the line's roll target for 100 ms, then ignores the laser for another
  100 ms [U1 `Camera.hpp:166-169`, `Camera.cpp:177-184`, `Game.cpp:2130-2142`].
- **Pivot.** Both clones roll about an axis through (USC) or 3 units below (KSM, eye at height 45, axis at 42) the
  eye [U1 `Camera.cpp:87-108`; K1 `GraphicsDefines.hpp:59-63`, `GraphicsMain.cpp:37-42`]. On screen, the pivot sits
  near the vanishing point (0.231H in USC landscape). So **the far end stays put and the judgement line swings**: at 10°
  the line's centre moves 0.156 track widths in both clones. The arcade is close to this by USC's account: roll and tilt
  were matched "close enough" [U2].

### Pitch, dolly and shift in detail

- **Meaning.**
  - From KSH `ver` 167, `zoom_top` "rotates the upper side of the lane with the judgment line as an axis", at 2400
    units per turn [F1 `ksh_format.md:297-299,389-390`].
  - `zoom_bottom`: "The lower side of the lane gets closer with a larger value."
  - `zoom_side` shifts the lane left (−) or right (+) [F1 `ksh_format.md:392-394`].
  - VOX `CAM_RotX`: "Higher = lane top higher on screen". VOX `CAM_Radi`: "Higher = more zoomed out" [V1
    `vox_format.md:341-350`].
- **Interpolation.**
  - KSH graphs are linear between points; two lines on one tick make an immediate change [F1 `ksh_format.md:372-376`].
  - KSM v2 and kson add optional Bézier curves (`zoom_top_curve` and others) [F1 `ksh_format.md:383-387`].
  - Charts before `ver` 167 get a 40 ms exponential smoothing in KSM [K1 `CamSystem.cpp:9-10,31-50`].
  - VOX rows carry a start value, an end value and a length in cells. A `Realize` control seems to set "overshoot,
    start, end" (unverified) [V1 `vox_format.md:337-339`].
- **USC pitch.**
  - A rigid rotation of the track about the line by `PitchScaleFunc(z) × 15°`. That is a piecewise-linear remap with
    knots at −4 and +5.59 units, so 100 = 12.7° in landscape [U1 `Camera.hpp:18-19`, `Camera.cpp:40-84,236`].
  - The camera aims at the un-pitched, un-rolled line [U1 `Camera.cpp:239,366-373`], so **the line does not move**.
- **KSM pitch.** 15° per 100, but the far edge rises by L·sin θ / 2.5 while it comes closer by L(1 − cos θ). That is a
  squashed rotation that looks weaker than a real 15° [K1 `Highway3DGraphics.cpp:88-101`].
- **USC dolly.**
  - The track moves along the eye-to-line direction by `1.65^(−z) − 1` track widths (z ≤ 0), or `distance ×
    (1.65^(−z^1.35) − 1)` (z > 0) [U1 `Camera.cpp:6,241-254`].
  - The line's centre stays on the same sightline. Its width scales.
- **KSM dolly.** Only the near edge moves, along a fixed 35° direction (scale 1/450 for positive values, 1/180 for
  negative), and the line graphic is rescaled empirically [K1 `Highway3DGraphics.cpp:15,96-100`; `CameraMath.hpp:7-18,
  37-53`]. Whether the line's centre stays exactly put there was not computed.
- **Pitching past the top cuts visible time.** In USC landscape the far edge reaches the top of the screen at about
  `zoom_top` 130 (16.6°). Beyond that, notes first appear at the screen's top edge:

  | `zoom_top` | First visible at depth | Visible time |
  |---|---|---|
  | 150 | 0.77 | ×0.77 |
  | 200 | 0.43 | ×0.43 |

  This is our projection. Under 10% of official conversions go above 90.

### Spins, swings and shake

- **Spin, USC** [U1 `Camera.cpp:17-38,186-216,256,409-417`]:
  - The length is in 192nds of a 4-beat measure.
  - The roll unwinds one full turn linearly over the first 75% of the length.
  - A damped sine (amplitude 30°, decay e^(−3.5t)) then settles over the next 75%.
  - The line is drawn with `sin(2π × spin) × 20°` instead of spinning.
- **Spin, KSM** [K1 `CamPatternSpin.cpp:51-86`]:
  - The track reaches 360° at 53% of the length with a sine ease-out.
  - It overshoots by 30° by 65%, then settles by 100%.
  - The line wobbles `sin(2πt) × 20°` and fades out by 150%.
  - The background spins 720° in 75%.
- **Half spin, KSM** [K1 `CamPatternHalfSpin.cpp:48-81`]: 60° at 27.5% of the length, back to 0 at 55%, −15° at 77.5%,
  0 at 100%.
- **Spin, arcade** (low confidence): the default lengths are 7, 2, 3, 12 and 3 beats for VOX types 1-5, and a normal
  roll completes its turn in the first 3/7 of its duration, then settles with a damped sine [V1 `camera.md:80-106`].
- **Only on a hit.** Spins fire only when the slam is hit, not missed [U1 `Game.cpp:2150-2162`; K1
  `JudgmentHandler.cpp:167-174`]. A player guide puts it this way: "handling the right-angle knobs well can rotate the
  lane itself" [R3].
- **Official spin lengths** (conversions):
  - Full spins (232): median 1.5 beats, 10th to 90th percentile 0.5-4.
  - Half spins (110): median 2 beats.
  - 84% of charts have at least one spin, with a median of 2 per chart.
- **Swing, KSM**: a sideways shift of `10 × sin(π · repeat · t) × decay × scale/50 / 2` units, default scale 250 and
  3 repeats [F1 `ksh_format.md:83-96`; K1 `CamPatternSwing.cpp:58-75`]. Conversions never use it [V1 `camera.md`, Spin
  section].
- **Slam shake.**
  - USC: `amplitude = −slam size` degrees of camera yaw, decaying 0.2 per 1/60 s [U1 `Camera.cpp:221-234,258-264`,
    `Game.cpp:2150-2154`]. USC's comment says the arcade (VIVID WAVE) uses "slamLength (0 to 1) × 15px", decaying
    3 px per frame [U1 `Camera.hpp:3-7`].
  - KSM: `sin(πr)(1 − r)² × 2.83` units over 150 ms. The peak is about 1.1 units, 2.4% of the 46.8-unit track, about
    40 ms in. It is skipped when the slam carries a swing [K1 `LaserSlamShake.cpp:5-20`, `JudgmentHandler.cpp:169-172`].

### What each move does on screen

Our projection of USC's landscape camera at 16:9. y is down from the top. "Gap" is the far edge's distance to the top
of the screen.

| Pose | Line centre x | Line y | Line width | Line angle | Far edge y | Far/near |
|---|---|---|---|---|---|---|
| Rest | 0.500W | 0.941H | 0.435W | 0° | 0.284H | 0.106 |
| Roll 10° (normal) | 0.566W | 0.933H | 0.433W | −8.0° | 0.283H | 0.105 |
| Roll 17.5° (bigger) | 0.616W | 0.916H | 0.430W | −14.1° | 0.281H | 0.102 |
| Roll 25° (biggest) | 0.665W | 0.889H | 0.424W | −20.1° | 0.279H | 0.099 |
| `zoom_top` 25 (3.2°) | 0.500W | 0.941H | 0.435W | 0° | 0.236H (17% of gap) | 0.108 |
| `zoom_top` 40 (5.1°) | same | same | same | 0° | 0.207H (27%) | 0.109 |
| `zoom_top` 80 (10.2°) | same | same | same | 0° | 0.125H (56%) | 0.113 |
| `zoom_top` 130 (16.6°) | same | same | same | 0° | 0.014H (95%) | 0.121 |
| `zoom_bottom` −60 | 0.500W | 0.941H | 0.340W | 0° | 0.303H | 0.131 |
| `zoom_bottom` −100 | 0.500W | 0.941H | 0.287W | 0° | 0.318H | 0.152 |
| `zoom_bottom` +100 | 0.500W | 0.941H | 0.718W | 0° | 0.255H | 0.067 |
| **Median pose** (top +40, bottom −65) | 0.500W | 0.941H | 0.335W (×0.77) | 0° | 0.231H (19%) | 0.138 (×1.30) |
| **90th-percentile pose** (top +80, bottom −100) | 0.500W | 0.941H | 0.287W (×0.66) | 0° | 0.171H (40%) | 0.163 (×1.54) |
| `zoom_side` 100 | 0.656W | 0.941H | 0.435W | 0° | 0.284H | 0.106 |
| Shake 1° | 0.493W | 0.941H | 0.435W | −0.8° | 0.284H | 0.106 |

- **In every zoom pose, the far end keeps its pixel width** (×0.99-1.07). The line narrows instead, which is why the
  ratio grows.
- **Near-line speed.** The near-line on-screen speed ((line y − far y) / ratio, in H per unit depth) drops ×0.83 in
  pixels at the median pose. Relative to the line's width it rises ×1.08.

### Is any of it automatic? What do charts do?

**Automatic.**

- Only the laser roll, which follows lasers the chart placed, and the slam shake.
- Spins need a chart marker and a hit slam.
- Neither clone reads sections, kiai-like markers or BPM changes to move the camera. USC's camera uses the timing
  point only for spin lengths and look-ahead [U1 `Camera.cpp:121,409-417`; `Scoring.cpp:362`]. KSM's camera system
  reads the chart's graphs [K1 `CamSystem.cpp:23-60`].
- For the arcade, sources describe chart content: "New charts may include zooming, moving, and even disappearing of the
  chart lanes" [R1]. The knob tilt was the series' headline feature in 2012 [R2]. No source mentions automatic motion
  at a chorus (unverified negative).

**Official conversions (124 charts):**

| Key | Charts using it | Points per chart (median) | Typical non-zero value | Ramp length (median, 10th-90th percentile) | Share of the chart's time it is non-zero (median) |
|---|---|---|---|---|---|
| `zoom_top` | 114 (92%) | 12 | \|35\|; 10th-90th percentile 25-80; 811 positive vs 211 negative | 3.0 beats (0.5-8), 880 ms (160-2740) | 27% |
| `zoom_bottom` | 123 (99%) | 30 | \|60\|; 25-100; 2798 negative vs 356 positive | 2.0 beats (0.25-8), 545 ms (90-2460) | 52% |
| `zoom_side` | 1 (1%) | — | one gimmick chart | — | — |
| `center_split` | 0 | — | — | — | — |
| manual `tilt` | 53 (43%) | — | median \|0.3\| (3°) | — | — |

- **Tilt type lines:** `normal` 288, `zero` 120, `keep_normal` 2. No `bigger` or `biggest`.
- **Excursions** (from leaving 0 to returning to 0):

  | Key | Excursions | Median length | 10th-90th percentile | Median peak |
  |---|---|---|---|---|
  | `zoom_top` | 365 (3.2 per chart) | 31 beats, 10.3 s | 4-80 beats | 40 |
  | `zoom_bottom` | 727 (5.9 per chart) | 23 beats, 7.3 s | 2-88 beats | 65 |

- **Ramps.** A ramp leaving 0 lasts a median of 3 beats (`zoom_top`, 900 ms) or 2 beats (`zoom_bottom`, 830 ms). A ramp
  back to 0 lasts 2-3.5 beats.
- **Bar alignment.** 55-65% of departures from 0 start on a bar line. They fall on every 4th or 8th bar only a little
  more often than chance, counting bars from the chart's start:

  | Bar | Observed | Chance |
  |---|---|---|
  | Every 4th | 15-19% | about 15% |
  | Every 8th | 9-12% | about 8% |
- **Joint pose**, sampled every quarter beat:

  | Pose | Share of chart time |
  |---|---|
  | At rest | 40.7% |
  | Zoomed out only | 26.4% |
  | Zoomed out and pitched up | 26.1% |
  | Zoomed out and pitched down | 2.7% |
  | Zoomed in (any) | 3.7% |
  | Pitch without zoom | 0.5% |

  **Pitch-up almost never comes without zoom-out.** While non-zero:
  - `zoom_top`: median +25, 10th-90th percentile −25 to +55;
  - `zoom_bottom`: median −60, 10th-90th percentile −114 to −13.
- **The fan charts move the camera much more** (90 charts):
  - `zoom_top` is in 97% of them, with 46 points per chart, non-zero 66% of the time;
  - 12 of them use `bigger` tilt.
- **Reading.** KONAMI's charts hold one pose per section and change it a few times per song.

### Other games

| Game | Does the field or camera move? | Trigger | Detail | Source |
|---|---|---|---|---|
| Chunithm | No camera motion described in normal play | — | The ExTAP accent note plays a **background** effect in one of 8 chart-chosen directions: up, down, centre, left/right centre, rotate left, rotate right, in/out | `game-chunithm.md`; `game-chunithm-feedback.md` [N1] |
| O.N.G.E.K.I. | The field itself changes: lanes narrow or move, and the field can break, so the character must follow | chart | Camera unverified | [G1] |
| Project Sekai | No camera motion found | — | Fever Time exists only in co-op shows: "a higher score is earned for each tapped note". Its visuals are unverified. Neither MIT fan engine implements fever or skill visuals. Official options include "Cut-in effect" and "Display Skill / ON FIRE" | [P1][P2][P3]; `game-sekai.md` |
| BanG Dream! | No lane change described | co-op fever | At fever start "sprinklers shoot out"; the audience's lightsticks wave faster and turn rainbow and golden; a "Fever Effects" HI/LO option | [B1] |
| Arcaea | Yes, chart-authored | chart; arcs | `camera(t, x, y, z, rx, ry, rz, easing, duration)` (details below). Arc tilt about 3° | [A1]; `game-arcaea.md` |
| maimai | None found (unverified negative) | — | — | — |
| YARG | Yes, automatic | play events | Highway raise at the start; kick bounce 0.03 units; punch on a combo break (0.25 s); "scoop" on star power (1° dip over 0.083 s). All move the strikeline | `game-yarg-clonehero.md`; [Y1] |
| Clone Hero | Unverified | — | — | — |
| StepMania 5 | Yes, as effect mods | player options, or timed chart "attacks" | Formulas below | [S1] |
| osu! | No camera motion | kiai | Details below | [O1][O2] |
| DJMAX | No camera motion found (unverified negative) | — | FEVER is a combo multiplier, up to ×5 | `game-djmax.md` |

**Arcaea's `camera()` events.**

- Moves are in 1/100 units and rotations in degrees.
- The easing is `l`, `qi`, `qo`, `s` or `reset`.
- Every active event adds its eased share, so moves accumulate [A1 `RawCamera.cs`, `CameraType.cs`,
  `CameraEvent.cs:58-80`, `CameraService.cs:184-235`].
- The camera moves, so the judgement line moves with it.
- Scene control includes `trackdisplay`, `enwidencamera`, `enwidenlanes` and `hidegroup` [A1].
- How often official charts use any of this is unverified.

**StepMania's effect mods** [S1 `ArrowEffects.cpp`; metrics `[ArrowEffects]`]:

- **Beat** kicks the column sideways on every beat. The kick peaks on the beat after a 0.2-beat quadratic rise, decays
  over 0.3 beat, rests for the remaining half beat, and alternates direction each beat. It is 20 px at 100%, shaped
  along the column by `sin(y/15 + π/2)` (`:196-228,766-770`).
- **Drunk**: x += cos(t + 0.2·column + y·10/480) × 32 px, with t in seconds.
- **Tipsy**: y += cos(1.2t + 1.8·column) × 25.6 px.
- **Bumpy**: z += 40·sin(y/16).
- Receptors are placed through the same column renderer [`ReceptorArrowRow.cpp:79-83`], so these move them too (not
  traced further).
- **NoAttacks** turns chart attacks off [`PlayerOptions.cpp:484-485,1101-1102`].

**osu!'s kiai.**

- osu!'s hit circles flash on each beat [`osu.Game.Rulesets.Osu/Skinning/Default/KiaiFlash.cs:16-42`]:
  - 25% additive white;
  - an 80 ms OutQuint rise that starts 80 ms early;
  - an OutSine fade over max(80, beat − 80) ms.
- Legacy skins flash at 30% [`osu.Game/Skinning/LegacyKiaiFlashingDrawable.cs:23,46-55`]. Taiko has a kiai glow.
- **Star fountains** fire in every ruleset within 500 ms of a kiai start, with a 400 ms shot
  [`osu.Game/Screens/Play/KiaiGameplayFountains.cs:55-63,82`; `Player.cs:456`]. The "Star fountains" setting defaults to
  on [`OsuConfigManager.cs:153`].
- osu!mania in lazer has no other kiai visual. Kiai "doesn't affect gameplay mechanics" outside taiko, and "cannot be
  deactivated by the player" [O2].

**The most common chorus effect** across these games is a beat-synced flash, or a one-shot burst when the section
starts (osu! fountains, BanG Dream! sprinklers). It is not camera motion. Camera motion at a chorus is always written
into the chart (SDVX, Arcaea, StepMania attacks) or set off by the player (YARG star power, SDVX/KSM slams). Among
chart-written motion, SDVX's held "zoom out + pitch up" pose is the one this survey can count.

## Note approach and speed

- **No camera move changes when a note is hit, or how much track the clones show.** Visible time stays the same until
  the far edge leaves the screen: USC `zoom_top` 150 gives ×0.77 (above).
- **On-screen speed does change.**
  - The median SDVX pose slows the near-line approach ×0.83 in pixels and speeds it ×1.08 relative to the line's width.
  - Pure pitch speeds it up about 9% (`zoom_top` 40).
  - During a ramp, notes drift with the far end.
- **Our projection** (`HighwayProjection`): the on-screen speed at the line is `(JudgementY − FarY) / FarWidthRatio`
  screen heights per unit depth, which is the derivative of `YAt` at depth 0. Any motion that keeps that ratio keeps
  the final approach looking the same.

## Judgement line and receptors

- **Keeps the line fully still:** SDVX pitch (`zoom_top`), in both clones.
- **Keeps the line's centre but changes its width:** the dolly (`zoom_bottom`).
- **Moves the line:**
  - roll and manual tilt (rotate and slide);
  - spin and half spin (the line wobbles ±20°; the track turns);
  - `zoom_side` and swing (slide);
  - slam shake (jolt);
  - `center_split` (lanes part at the line).
- **Other games:** Arcaea's camera, YARG's motions and StepMania's mods all move the line or the receptors.
- **KSM draws the line as its own plane**, with its own rotation term [K1 `Jdgline3DGraphics.cpp:32-33`;
  `GraphicsDefines.hpp:66-68`]. That is how a spin turns the track but only wobbles the line.

## Lane covers / walls

- VOX `LaneY` can hide the lane, with `LANECLEARCOL` shading the empty lane area [V1 `vox_format.md:358-370`]. It is
  chart-only.
- Our wall lives in track space (SPEC 4), so any projection change in *What this means for us* carries it along.

## Notes and holds (shapes, rendering)

- No SDVX move changes note shapes. Notes ride the plane, so they turn and scale with it.

## Key count and lane layout

- `center_split` opens a gap at the centre, 100 = one lane [F1 `ksh_format.md:377-382`]. Official conversions never use
  it; VOX `Morphing2` appears in a few gimmick charts [V1 `vox_format.md:362-366`].

## Feedback (judgement display, hit effects, lane lighting)

The automatic "section" effects are all feedback rather than camera:

- osu!'s kiai flash and star fountains (values above);
- taiko's kiai glow;
- BanG Dream!'s fever sprinklers and audience colours;
- Chunithm's ExTAP background effects.

## Readability and player options

| Game | Option that reduces motion | Default | What it covers |
|---|---|---|---|
| SDVX (EXCEED GEAR) | None. The play options guide lists no camera, lane-effect or tilt option, only background brightness and VALKYRIE EFFECT [R5] | — | — |
| USC | `EnableFancyHighwayRoll`, config file only [U1 `GameConfig.cpp:249`, `Game.cpp:1372`, `Camera.cpp:337-344`] | on | Only slam roll and roll ignore; all other motion stays |
| KSM v2 | None found among the config keys | — | — |
| YARG | "Enable Highway Animation": "the highway will animate at the beginning and end of the song and based on performance" [Y1 `SettingsManager.Settings.cs:640`] | on | The raise, punch and scoop |
| YARG | "Drum Kick Bounce Multiplier": "Set to "0" if you want to disable" [Y1 `:641`] | 1 (range 0-2) | The kick bounce |
| StepMania | `NoAttacks` | off | Chart attacks; player mods are opt-in anyway |
| BanG Dream! | Fever Effects HI/LO [B1] | — | Fever visuals |
| Project Sekai | Cut-in effect; Display Skill / ON FIRE; Lite MV, "recommended for full-combo attempts" (`game-sekai.md` K1, K6) | — | Overlays and the music video |
| osu! | "Star fountains" | on | Fountains only. The kiai flash has no switch |
| Arcaea | Unverified | — | — |

- **What players say about SDVX's motion is thin.** One fan wiki calls the lane effects "fancy interface effects that
  can potentially screw over the player". It says the final songs of the KONAMI Arcade Championship (KAC) use lane
  changes to "catch the finalists off-guard" [R4] (low weight).
- No guide was found that measures how motion affects reading.
- **Pattern:** games whose motion is part of the chart (SDVX, KSM, Arcaea) give no way to turn it off. Games that move
  the camera on their own (YARG), or by player mod (StepMania), give a toggle.

## What this means for us

SPEC 2 allows optional cosmetic motion, off by default, that never moves the judgement line. Mania maps carry kiai
sections and timing points only. Each effect below is a pure function of time, like `KiaiSway`, so seeking and
captures stay exact. All of them sit outside the presets, as Kiai sway does, so they carry across presets (Alpha's "keep
them across different presets"). Lift and pulse are defined relative to each preset's own geometry.

### C1. Kiai lift: SDVX's chorus pose, with the line held still (recommended first; SPEC 2, 3, 4)

**What it does.** During kiai the track pitches up about the judgement line. The far edge rises toward the top of the
screen, and the far end widens just enough that notes reach the line at the same on-screen speed.

**Setting.** "Kiai lift", 0-100%, default **0 (off)**, outside presets, tilted cameras only.

**Formula** at strength L, scaled by the envelope below:

- `FarY' = FarY × (1 − L)`
- `FarWidthRatio' = min(1, FarWidthRatio × (JudgementY − FarY') / (JudgementY − FarY))`
- `JudgementY`, `NearWidth` and `CentreX` are unchanged.

**Why this formula and not SDVX's exactly.** SDVX gets its "proportionally longer" lane by zooming out, which narrows
the line by 23% at the median pose. That moves our receptors: at 7K on the Chunithm preset the outer lanes would move
135 px at 1080p, 0.8 of a lane. Matching SDVX's ×1.30 ratio while holding the line would instead slow the final
approach by 23%. Keeping `(JudgementY − FarY) / FarWidthRatio` fixed keeps the near-line look. SDVX itself stays
within ±10% of it relative to the line's width.

**Timing.**

- Ramp in over **3 beats** from the first kiai point: SDVX's median ramp, 0.9 s. Clamp it to 500-1500 ms.
- Use smoothstep rather than KSH's linear ramp, to avoid velocity jumps.
- Hold until the kiai ends, then ramp out over 3 beats. SDVX's median return is 2-3.5 beats.
- Merge sections closer than one ramp, as `KiaiSway` merges its sections.

**Suggested strength when turned on: 30%.**

- At 30% the far edge rises 30% of its gap. That is between USC's `zoom_top` 40 (27%) and SDVX's median pose (19%).
- 100% brings the far edge to the top edge, about `zoom_top` 130, which official charts rarely exceed.
- That is Alpha's description of SDVX ("tilt upwards to approach from the top") at full strength.

**Screen effect** (`HighwayProjection`, 1080p). "Far notes move" is how far the notes near the far edge drift over the
ramp:

| Rest pose | Strength | Far edge | Far/near | Far notes move |
|---|---|---|---|---|
| SDVX-like (USC landscape: line 0.941H, far 0.284H, ratio 0.106) | 30% | 0.199H | 0.120 | 92 px |
| SDVX-like | 100% | 0.000H | 0.152 | 307 px |
| Chunithm arcade (far edge 0.163H) | 30% | 0.114H | 0.255 | 53 px |

**It does nothing while the far edge sits at the top edge**, as in today's Chunithm and Voltex presets. It becomes
useful together with the approach-angle setting Alpha asked for in the same message, which lowers the far edge
SDVX-style. Alpha must choose (Q-a below).

**Guarantees.**

- The judgement line, the key area below it and the hit lighting do not move (SPEC 2, SPEC 8).
- Visible time is unchanged, because track-space depth is unchanged (SPEC 3).
- The wall rides along in track space (SPEC 4).
- Notes still enter at the far edge, never inside the track (SPEC 2).
- With a side stage's lean, recompute `LeanTowards` from the lifted ratio so the vanishing point stays on its target
  (`HighwayLayout.LeanTowards` depends on the ratio).

### C2. Kiai sway: keep it, and record SDVX's scale (SPEC 2)

- **Ours is the opposite of SDVX's roll.** Our sway is a shear about the line, so the far end moves and the line stays.
  SDVX's roll pivots near the vanishing point, so the line moves and the far end stays. Keep ours (`game-sdvx.md` P5).
- **Calibration.**
  - SDVX's normal 10° tilt turns lanes about 8° on screen. In our shear that is a far-edge shift of
    tan 8° × track height, which is **7.3% W** on the Chunithm and Voltex presets.
  - Bigger is 13% W and biggest 19% W.
  - Our 100% (`MAX_KIAI_SWAY` = 3% W) is 3.3°, about 40% of a normal SDVX tilt.
  - If Alpha wants SDVX-sized motion, raise `MAX_KIAI_SWAY` to 0.07. `HighwayLayout.MAX_SWAY` (0.2) already allows it.
- **Optional "SDVX timing" style** (our design, unverified as a match):
  - Hold the lean on one side instead of swinging on every beat, changing side on bar lines.
  - Reach it in **250 ms** (SDVX's 40°/s for a full tilt).
  - Return over **1.1 s** at the kiai end, four times slower, as SDVX returns.
  - Default: Beat (the current behaviour).

### C3. Kiai flash: osu!'s own kiai effect on the track (optional, no motion)

- **What it does.** On each beat in kiai, the track surface and lane lines (not the notes) flash with osu!'s values:
  - 25% additive white;
  - an 80 ms OutQuint rise starting 80 ms before the beat;
  - an OutSine fade over max(80, beat − 80) ms.
- **Default:** off.
- **Assessment.** This is the most common kiai/chorus effect in the survey and the one osu! players know. It cannot
  hurt reading, because nothing moves. It is not SDVX-like.

### C4. Kiai-start pulse (optional, low priority)

- **What it does.** On the first beat of a kiai section, one extra pulse of lift: +15 points of lift, a 0.2-beat
  ease-in and a 0.8-beat ease-out. The shape follows StepMania's Beat envelope; the overshoot is in the spirit of the
  clones' spin recoveries.
- **Defaults:** off; 15 points when turned on.
- **Assessment.** It does not move the line and is brief. It has the same no-op caveat as C1.

### C5. Beat pulse (optional, lowest priority)

- **What it does.** On each beat in kiai, lift ±5 points with StepMania's Beat envelope: peak on the beat, a 0.2-beat
  quadratic rise and a 0.3-beat decay. Beats shorter than 300 ms are doubled, as for the sway.
- **Assessment.** Far notes jump on every beat. It is the least readable of the line-safe options and not SDVX-like.

### C6. Not recommended (each moves the judgement line or the lanes at it; SPEC 2)

- **Zoom-out (dolly).** It is SDVX's most frequent move (99% of charts, 52% of the time). It moves the line's ends:
  the outer lanes shift 0.8 of a lane at 7K at the median −60.
- **Roll toward one side.** The line slides 6.6% W and tilts 8° at 10°.
- **Spins, half spins, swings.** The whole track turns 360° or 60°. Mania has no slams to fire them.
- **Slam shake, YARG-style punch or bounce.** The line jolts 0.7-1% W.
- **Centre split, lane hiding.**

### Ranking

| Effect | SDVX-like | Readable | Moves the line | Default |
|---|---|---|---|---|
| C1 Kiai lift | High (SDVX's held chorus pose) | High (near-line speed kept; 3-beat ramps) | No | Off; 30% when on |
| C2 Kiai sway (exists) | Medium (SDVX's roll is laser-driven and pivots near the far end) | High at 3% W | No | Off |
| C3 Kiai flash | None (osu!) | Highest (no motion) | No | Off |
| C4 Kiai-start pulse | Low | High | No | Off |
| C5 Beat pulse | Low (StepMania) | Medium | No | Off |
| Zoom-out, roll, spin, shake | High | Low for keyboard play | Yes | Excluded |

### Questions for Alpha (for `QUESTIONS.md`)

- **Q-a.** Kiai lift raises a far edge that sits below the top of the screen. With today's presets (far edge at the top)
  it does nothing. Should it wait for the approach-angle setting? Or should it, on those presets, widen the far end
  anyway? At SDVX's ×1.30 that slows notes near the line by 23%.
- **Q-b.** Do you want an opt-in "SDVX roll" that moves the judgement line, as the arcade does? It would need an
  exception to SPEC 2.
- **Q-c.** Should the osu!-style kiai flash on the track be offered?

## Unverified / open questions

- **Arcade SDVX numbers.** The roll rates, the bigger scale (KSM 1.75 vs the low-confidence 2.375), the roll pivot,
  spin shapes and the slam shake size are the clones' or vox2ksh's approximations. No arcade frame was measured.
- **Choruses.** That SDVX camera excursions coincide with choruses is an inference from their length (about 8 bars)
  and bar alignment. KSH has no section markers.
- **VOX `Realize`** (overshoot?) semantics.
- **Whether KSM's dolly keeps the line's centre exactly still.**
- **Other games:** Arcaea's official use of `camera()` and any player option there; Project Sekai and DJMAX fever
  visuals; maimai's and Clone Hero's lack of motion; osu! stable's mania kiai visuals; whether StepMania's receptors
  follow every mod.
- **Charts not counted:** official chart files themselves (VOX), and KSH conversion packs that live only in archives.
- **Our proposals (C1-C5) are our own designs** calibrated on these numbers. None reproduces an arcade feature exactly.

## Sources

- [U1] Unnamed SDVX Clone, MIT, commit 6c3a33f9c66460f13934df034dca7ae3f9a130f9:
  https://github.com/Drewol/unnamed-sdvx-clone/tree/6c3a33f9c66460f13934df034dca7ae3f9a130f9.
  - `Main/include/Camera.hpp:3-21,117-169`: shake comment, pitch units, MAX_ROLL_ANGLE, durations.
  - `Main/src/Camera.cpp`:
    - `:6,17-38` zoom power, damped sine, spin;
    - `:40-108` pitch remap and rig;
    - `:110-257` roll rates, slam roll, spins, offset, shake, zoom;
    - `:337-344` fancy roll;
    - `:357-390` the camera aims at the un-rolled line;
    - `:392-417` keep, spin length.
  - `Main/src/Game.cpp:879-897` (roll target, zooms), `:1372`, `:2130-2162` (slams, shake, spins), `:2409-2427`
    (tilt types).
  - `Main/src/Scoring.cpp:347-371` (2-beat look-ahead), `Main/src/GameConfig.cpp:249`.
  - `Beatmap/src/BeatmapFromKSH.cpp:816-895,1332-1360`.
- [U2] USC PR #267 "Camera Changes": https://github.com/Drewol/unnamed-sdvx-clone/pull/267. Roll and tilt matched to
  SDVX "close enough".
- [K1] K-Shoot MANIA v2, MIT, commit 1de5843ad99de1c8c31b286fe4c4a0eda7a38b96:
  https://github.com/kshootmania/ksm-v2/tree/1de5843ad99de1c8c31b286fe4c4a0eda7a38b96.
  - Under `kshootmania/src/MusicGame/`:
    - `Camera/HighwayTiltAuto.cpp:9-146`, `HighwayTiltManual.cpp:10`, `HighwayTilt.cpp:13`;
    - `Camera/CamSystem.cpp:9-60`, `Camera/CameraMath.hpp:7-53,109-113`;
    - `Camera/CamPattern/CamPatternSpin.cpp:51-86`, `CamPatternHalfSpin.cpp:48-81`, `CamPatternSwing.cpp:58-75`;
    - `GameDefines.hpp:142-143`;
    - `Graphics/GraphicsDefines.hpp:59-68`, `Graphics/GraphicsMain.cpp:37-42`;
    - `Graphics/Highway/Highway3DGraphics.cpp:15,88-106`, `Graphics/Jdgline/Jdgline3DGraphics.cpp:32-33`;
    - `Judgment/LaserSlamShake.cpp:5-31`, `Judgment/JudgmentHandler.cpp:156-174`.
- [F1] KSH and kson chart format specifications, CC0, commit b984509ad6b43c208f8a3b4aff51d6a8664b896d:
  https://github.com/kshootmania/ksm-chart-format/tree/b984509ad6b43c208f8a3b4aff51d6a8664b896d.
  - `ksh_format.md:83-96` (spins, swing parameters), `:272-299` (version history: tilt relaxation, ver 167 pitch),
    `:357-394` (tilt, zooms, centre split, curves, camera meanings).
  - `kson_format.md:667-689` (tilt scales 1.0/1.75/2.5, keep), `:740-752` (camera graphs).
- [V1] vox2ksh, MIT, commit a412d089c4842b01ae12ceacf419f4756467b8b8: https://github.com/whiteou7/vox2ksh.
  - `specs/vox_format.md:105-112` (`#TILT MODE INFO`), `:331-370` (`SPCONTROLER`: `Realize`, `CAM_RotX`,
    `CAM_Radi`, `Tilt`, `LaneY`, `Morphing2`).
  - `specs/camera.md:15-18` (unit mapping), `:22-40` (pretilt), `:45-58` (arcade tilt model), `:80-106` (spin lengths
    and shape).
  - `specs/evidence/README.md` calls the evidence LLM-generated: low confidence.
- [C1] TedTheRed/exceed-gear-converts, commit 5802914d7e7bf089993b8400a3570be7c9b5c87e:
  https://github.com/TedTheRed/exceed-gear-converts. 48 KSH conversions, read in memory for counts only.
- [C2] TsFreddie/KSHConvertFix, commit 0ec04536e81902b7a0c784eca81e2f1fc6fd3df7:
  https://github.com/TsFreddie/KSHConvertFix. 7 conversions, counts only.
- [C3] GRI51/ksh_chart, commit ae1fbbebeaf1ec9f08865666755750faef91a557: https://github.com/GRI51/ksh_chart. 90 original
  fan charts, counts only.
- [C4] vox2ksh reference conversions, `scripts/shared/reference/ksh/` at [V1]'s commit. 69 charts, counts only; the
  clone was deleted after counting.
- [R1] RemyWiki, What is SOUND VOLTEX: https://remywiki.com/What_is_SOUND_VOLTEX. "New charts may include zooming,
  moving, and even disappearing of the chart lanes."
- [R2] 4Gamer, 2012-01-18, SOUND VOLTEX announcement: https://www.4gamer.net/games/139/G013911/20120118039/. Knobs
  that "tilt the line on the display itself" as the main feature.
- [R3] Scrapbox (kuranogawa), SOUND VOLTEX: https://scrapbox.io/kuranogawa/SOUND_VOLTEX. Slams handled well can rotate
  the lane.
- [R4] TV Tropes, SOUND VOLTEX: https://tvtropes.org/pmwiki/pmwiki.php/VideoGame/SOUNDVOLTEX. Lane effects described
  as "fancy interface effects that can potentially screw over the player"; KAC finals. Low weight.
- [R5] voltexes.com, EXCEED GEAR play options guide: https://voltexes.com/exceed-gear-play-options-guide/. No camera or
  tilt option.
- [A1] ArcCreate, GPL-3.0, read only, commit a00f94ca602b990bc15d7ca530434da9f875d152:
  https://github.com/Arcthesia/ArcCreate/tree/a00f94ca602b990bc15d7ca530434da9f875d152.
  - `Assets/Scripts/ChartFormat/Types/RawCamera.cs`, `Gameplay/Data/CameraType.cs`,
    `Gameplay/Data/Events/CameraEvent.cs:58-80`, `Gameplay/Camera/CameraService.cs:184-273`.
  - `Compose/EventsEditor/Scenecontrol/CommandTypes/` (`enwidencamera`, `trackdisplay` and others).
- [S1] StepMania 5, MIT, commit 825467bcd81c812b33ad684dc04dd151b2d5dec3:
  - `src/ArrowEffects.cpp:179-260,738-770,1206-1210`;
  - `Themes/_fallback/metrics.ini` `[ArrowEffects]`;
  - `src/PlayerOptions.cpp:484-485,1101-1102`;
  - `src/ReceptorArrowRow.cpp:79-83`.

  https://github.com/stepmania/stepmania/tree/825467bcd81c812b33ad684dc04dd151b2d5dec3.
- [Y1] YARG, LGPL-3.0, commit 8e5c4a81f4ed80ae4360605017d71273584e7c86:
  - `Assets/Script/Settings/SettingsManager.Settings.cs:640-641`;
  - `Assets/StreamingAssets/lang/en-US.json` ("Enable Highway Animation", "Drum Kick Bounce Multiplier");
  - camera motion detail in `game-yarg-clonehero.md`.
- [O1] osu! (this repository, MIT):
  - `osu.Game.Rulesets.Osu/Skinning/Default/KiaiFlash.cs:16-42`;
  - `osu.Game/Skinning/LegacyKiaiFlashingDrawable.cs:23,46-55`;
  - `osu.Game/Screens/Play/KiaiGameplayFountains.cs:55-82`, `osu.Game/Screens/Play/Player.cs:456`;
  - `osu.Game/Configuration/OsuConfigManager.cs:153`;
  - `osu.Game.Rulesets.ManiaOrtho/Highway/KiaiSway.cs`, `HighwaySettings.cs` (`MAX_KIAI_SWAY`),
    `HighwayLayout.cs` (`MAX_SWAY`, `LeanTowards`), `HighwayProjection.cs` (`YAt`).
- [O2] osu! wiki, Kiai time: https://osu.ppy.sh/wiki/en/Gameplay/Kiai_time.
- [P1] Project Sekai support, "What are Fever Chance and Fever Time":
  https://support-pjsekai.sega.com/hc/en-us/articles/16601029648025-What-are-Fever-Chance-and-Fever-Time.
- [P2] Game8 (JP), Project Sekai FEVER: https://game8.jp/pjsekai/595324. Co-op only, SUPER FEVER conditions.
- [P3] Sonolus Project Sekai engines, both MIT, searched for fever, skill and camera code (none found):
  - https://github.com/NonSpicyBurrito/sonolus-pjsekai-engine/tree/0f6f37fc9b512b9d156ec4c00d998fe3916b157d;
  - https://github.com/Next-SEKAI/sonolus-next-sekai-engine/tree/bfe61449e20484fd403d5314aba61a740aab3486.
- [B1] BanG Dream! wiki, Girls Band Party! In-Depth Guide:
  https://bandori.fandom.com/wiki/Girls_Band_Party!_In-Depth_Guide. Fever sprinklers, audience colours, Fever Effects
  HI/LO.
- [G1] Niconico Pedia, オンゲキ: https://dic.nicovideo.jp/a/%E3%82%AA%E3%83%B3%E3%82%B2%E3%82%AD. Lanes narrow or move;
  the field can break.
- [N1] `game-chunithm.md` (camera does not move), `game-chunithm-feedback.md` (ExTAP background directions).
- Related notes: `game-usc.md`, `game-sdvx.md` (P5), `game-arcaea.md` (camera motion), `game-yarg-clonehero.md`
  (camera motion), `game-sekai.md`, `game-djmax.md`.
- Method: USC's landscape rig and our `HighwayProjection` formulas were re-implemented in throwaway Python scripts in
  the session scratchpad. They produced every screen number above, and the KSH counts came from a parser in the same
  place. Nothing was committed.
