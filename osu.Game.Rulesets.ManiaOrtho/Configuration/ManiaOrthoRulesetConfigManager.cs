// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Configuration.Tracking;
using osu.Game.Configuration;
using osu.Game.Rulesets.Configuration;
using osu.Game.Rulesets.Mania.Configuration;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.ManiaOrtho.Highway;

namespace osu.Game.Rulesets.ManiaOrtho.Configuration
{
    /// <summary>
    /// This ruleset's settings: osu!mania's own (scroll speed, note colouring), which <c>DrawableManiaRuleset</c> reads
    /// from this type, plus the highway settings in <see cref="Highway"/>.
    /// </summary>
    /// <remarks>
    /// Highway settings need their own enum, and a <see cref="RulesetConfigManager{TLookup}"/> has exactly one, so they live
    /// in a second manager stored under the same ruleset. Keys are enum member names and the two enums share none.
    /// None of these settings affect judgement or scoring (CLAUDE.md invariant 3).
    /// </remarks>
    public class ManiaOrthoRulesetConfigManager : ManiaRulesetConfigManager
    {
        public HighwayConfigManager Highway { get; }

        public ManiaOrthoRulesetConfigManager(SettingsStore? settings, RulesetInfo ruleset, int? variant = null)
            : base(settings, ruleset, variant)
        {
            Highway = new HighwayConfigManager(settings, ruleset, variant);

            keepFineScrollSpeed();

            // The camera defines the direction (SPEC 3): notes always come down the track, whatever was stored.
            SetValue(ManiaRulesetSetting.ScrollDirection, ManiaScrollingDirection.Down);
            GetOriginalBindable<ManiaScrollingDirection>(ManiaRulesetSetting.ScrollDirection).Disabled = true;
        }

        protected override void InitialiseDefaults()
        {
            base.InitialiseDefaults();

            // Finer and up to faster than osu!mania's (0.1, 1-40), so every speed scale's numbers land on their own speeds
            // (SpeedScale). Display only: the time range is all a speed changes.
            var speed = (BindableDouble)GetOriginalBindable<double>(ManiaRulesetSetting.ScrollSpeed);
            speed.MaxValue = SpeedScales.MAX_MANIA_SPEED;
            speed.Precision = SpeedScales.MANIA_SPEED_PRECISION;
        }

        /// <summary>
        /// Brings back the stored speed at maniaortho's precision and range, and keeps a copy of it from now on.
        /// </summary>
        /// <remarks>
        /// osu!mania's defaults read the stored speed into its own limits (0.1, up to 40) before <see cref="InitialiseDefaults"/>
        /// widens them, so the finer, faster speed would be lost on every start. The copy is used only while osu!mania's limits
        /// would read it as the stored speed, so a speed stored without one is kept.
        /// </remarks>
        private void keepFineScrollSpeed()
        {
            var speed = GetOriginalBindable<double>(ManiaRulesetSetting.ScrollSpeed);
            double fine = Highway.Get<double>(HighwaySetting.FineScrollSpeed);

            // Within one of osu!mania's 0.1 steps: its rounding of a half step may go either way.
            if (fine >= 1 && Math.Abs(Math.Min(fine, mania_max_speed) - speed.Value) < 0.1 + 1e-6)
                speed.Value = fine;

            speed.BindValueChanged(e => Highway.SetValue(HighwaySetting.FineScrollSpeed, e.NewValue), true);
        }

        /// <summary>
        /// osu!mania's own fastest speed.
        /// </summary>
        private const double mania_max_speed = 40;

        /// <summary>
        /// None: mania's scroll speed toast shows <c>11485 / speed</c>, not the time notes are visible on the highway. A play's
        /// <see cref="UI.HighwayToasts"/> shows the speed and wall toasts instead, with the real visible time.
        /// </summary>
        public override TrackedSettings CreateTrackedSettings() => new TrackedSettings();

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (isDisposing)
                Highway.Dispose();
        }
    }

    public class HighwayConfigManager : RulesetConfigManager<HighwaySetting>
    {
        private bool applyingPreset;

        public HighwayConfigManager(SettingsStore? settings, RulesetInfo ruleset, int? variant = null)
            : base(settings, ruleset, variant)
        {
            var preset = GetOriginalBindable<HighwayPreset>(HighwaySetting.Preset);

            keepWidthByLane(preset);

            // A named preset always shows its current values, so tuning a preset in code reaches everyone using it. Its
            // game's numbers (speed and wall scales) are set when it is chosen, and stay as the player leaves them.
            ApplyPreset(preset.Value);

            preset.ValueChanged += e =>
            {
                ApplyPreset(e.NewValue);
                ApplyPresetScales(e.NewValue);
            };

            // Changing any value by hand turns the preset into "Custom".
            GetOriginalBindable<HighwayCameraMode>(HighwaySetting.CameraMode).ValueChanged += _ => onPresetValueChanged();
            GetOriginalBindable<float>(HighwaySetting.LaneTilt).ValueChanged += _ => onPresetValueChanged();
            GetOriginalBindable<float>(HighwaySetting.LaneLength).ValueChanged += _ => onPresetValueChanged();
            GetOriginalBindable<float>(HighwaySetting.Approach).ValueChanged += _ => onPresetValueChanged();
            GetOriginalBindable<float>(HighwaySetting.JudgementLineHeight).ValueChanged += _ => onPresetValueChanged();
            GetOriginalBindable<float>(HighwaySetting.TrackWidth).ValueChanged += _ => onPresetValueChanged();
            GetOriginalBindable<float>(HighwaySetting.Wall).ValueChanged += _ => onPresetValueChanged();
            GetOriginalBindable<float>(HighwaySetting.NoteThickness).ValueChanged += _ => onPresetValueChanged();
            GetOriginalBindable<HighwayWidthMode>(HighwaySetting.WidthMode).ValueChanged += _ => onPresetValueChanged();
            GetOriginalBindable<float>(HighwaySetting.LaneWidth).ValueChanged += _ => onPresetValueChanged();

            void onPresetValueChanged()
            {
                if (!applyingPreset && preset.Value != HighwayPreset.Custom && !matches(preset.Value))
                    preset.Value = HighwayPreset.Custom;
            }

            // Choosing a game's wall scale also gives the wall that game's speed behaviour (Chunithm and osu!mania keep the
            // speed, IIDX the visible time); the toggle can still be changed afterwards.
            GetOriginalBindable<WallScale>(HighwaySetting.WallScale).ValueChanged += e =>
            {
                if (!applyingPreset && WallScales.KeepsVisibleTime(e.NewValue) is bool keepsVisibleTime)
                    SetValue(HighwaySetting.WallKeepsVisibleTime, keepsVisibleTime);
            };

            ApplySkinPortDefaults();
            applySkinLayoutDefault();
            applyPresetScalesOnce();
            keepCustomCameraAsTilt();
        }

        /// <summary>
        /// Once per install, when the lane's tilt and length replaced the far edge's width and height (Alpha 2026-11-08): a
        /// Custom camera keeps its shape as the tilt and length that draw it (<see cref="HighwayCamera.FromLayout"/>). A named
        /// preset already shows its own.
        /// </summary>
        private void keepCustomCameraAsTilt()
        {
            var applied = GetOriginalBindable<bool>(HighwaySetting.LaneTiltApplied);

            if (applied.Value)
                return;

            if (Get<HighwayPreset>(HighwaySetting.Preset) == HighwayPreset.Custom)
            {
                bool perspective = Get<HighwayCameraMode>(HighwaySetting.CameraMode) == HighwayCameraMode.Perspective;
                var camera = HighwayCamera.FromLayout(1 - Get<float>(HighwaySetting.JudgementLineHeight), Get<float>(HighwaySetting.FarEdgeHeight),
                    Get<float>(HighwaySetting.FarWidthRatio), perspective);

                SetValue(HighwaySetting.LaneTilt, camera.Tilt);

                // A parallel camera's tilt alone sets its height; the length stays for a later perspective camera.
                if (perspective)
                    SetValue(HighwaySetting.LaneLength, camera.LaneLength);
            }

            applied.Value = true;
        }

        /// <summary>
        /// Once per install, when presets started to carry their game's numbers (Alpha 2026-11-08): the preset in use sets its
        /// speed and wall scales, as choosing it now does. Custom changes nothing.
        /// </summary>
        /// <summary>
        /// Once, on an install from before presets set the width (Alpha 2026-11-08): "Width by" Lane was a preference outside
        /// the presets then, and every preset is "Width by" Track or has its own lane width now. Applying the preset would
        /// throw the player's lane width away, so such an install moves to Custom and keeps it. Runs before the scales'
        /// once-per-install step marks the install as done.
        /// </summary>
        private void keepWidthByLane(Bindable<HighwayPreset> preset)
        {
            if (GetOriginalBindable<bool>(HighwaySetting.PresetScalesApplied).Value || preset.Value == HighwayPreset.Custom)
                return;

            var values = HighwayPresets.Get(preset.Value);

            if (Get<HighwayWidthMode>(HighwaySetting.WidthMode) != values.WidthMode
                || (values.WidthMode == HighwayWidthMode.Lane && !near(Get<float>(HighwaySetting.LaneWidth), values.LaneWidth)))
                preset.Value = HighwayPreset.Custom;
        }

        private void applyPresetScalesOnce()
        {
            var applied = GetOriginalBindable<bool>(HighwaySetting.PresetScalesApplied);

            if (applied.Value)
                return;

            ApplyPresetScales(Get<HighwayPreset>(HighwaySetting.Preset));
            applied.Value = true;
        }

        /// <summary>
        /// Once per install: the note, hit effect and track styles were stored as "Highway" (the default before the skin
        /// porter) the first time this ruleset ran, so the new "Skin (ported)" default would never reach an existing install.
        /// Styles still on "Highway" move to "Skin (ported)" (skins without legacy mania images keep the highway's look
        /// anyway); a style set to anything else is left alone. Runs once, so choosing "Highway" again sticks.
        /// </summary>
        public void ApplySkinPortDefaults()
        {
            var applied = GetOriginalBindable<bool>(HighwaySetting.SkinPortDefaultsApplied);

            if (applied.Value)
                return;

            if (Get<HighwayNoteStyle>(HighwaySetting.NoteStyle) == HighwayNoteStyle.Highway)
                SetValue(HighwaySetting.NoteStyle, HighwayNoteStyle.Ported);
            if (Get<HighwayHitEffectStyle>(HighwaySetting.HitEffects) == HighwayHitEffectStyle.Highway)
                SetValue(HighwaySetting.HitEffects, HighwayHitEffectStyle.Ported);
            if (Get<HighwayTrackStyle>(HighwaySetting.Track) == HighwayTrackStyle.Highway)
                SetValue(HighwaySetting.Track, HighwayTrackStyle.Ported);

            applied.Value = true;
        }

        /// <summary>
        /// Once per install, for the same reason as the styles above: the stage fit's default moved from the camera's width
        /// ("Skin position") to the skin's own ("Skin position and width"), so an install still on the old default moves; a
        /// fit chosen afterwards sticks.
        /// </summary>
        private void applySkinLayoutDefault()
        {
            var applied = GetOriginalBindable<bool>(HighwaySetting.SkinLayoutDefaultApplied);

            if (applied.Value)
                return;

            if (Get<SkinStageFit>(HighwaySetting.StageFit) == SkinStageFit.SkinPosition)
                SetValue(HighwaySetting.StageFit, SkinStageFit.SkinLayout);

            applied.Value = true;
        }

        protected override void InitialiseDefaults()
        {
            base.InitialiseDefaults();

            var defaults = HighwayPresets.Get(HighwayPresets.DEFAULT);

            // Fine enough for measured preset values such as a 7.4% judgement line.
            const float precision = 0.001f;

            SetDefault(HighwaySetting.Preset, HighwayPresets.DEFAULT);
            SetDefault(HighwaySetting.CameraMode, defaults.CameraMode);
            SetDefault(HighwaySetting.LaneTilt, defaults.LaneTilt, HighwayCamera.MIN_TILT, HighwayCamera.MAX_TILT, 0.1f);
            SetDefault(HighwaySetting.LaneLength, defaults.LaneLength, HighwayCamera.MIN_LANE_LENGTH, HighwayCamera.MAX_LANE_LENGTH, 0.01f);
            SetDefault(HighwaySetting.Approach, defaults.Approach, ApproachCurve.PERSPECTIVE, ApproachCurve.EVEN, 0.05f);
            SetDefault(HighwaySetting.JudgementLineHeight, defaults.JudgementLineHeight, 0f, HighwaySettings.MAX_JUDGEMENT_LINE_HEIGHT, precision);
            // Read once, by keepCustomCameraAsTilt: the far edge as set before the lane's tilt and length (Chunithm's then).
            SetDefault(HighwaySetting.FarWidthRatio, 0.24f, HighwayLayout.MIN_FAR_WIDTH_RATIO, 1f, precision);
            SetDefault(HighwaySetting.FarEdgeHeight, 0f, 0f, 0.5f, precision);
            SetDefault(HighwaySetting.LaneTiltApplied, false);
            SetDefault(HighwaySetting.TrackWidth, defaults.TrackWidth, 0.2f, 1f, precision);
            // Fine enough for every scale's units (a white number is a thousandth of the lane on screen).
            SetDefault(HighwaySetting.Wall, defaults.Wall, 0f, HighwayScroll.MAX_WALL, 0.0001f);
            SetDefault(HighwaySetting.NoteThickness, defaults.NoteThickness, 0.005f, HighwaySettings.MAX_NOTE_THICKNESS, 0.0005f);
            SetDefault(HighwaySetting.WallKeepsVisibleTime, true);
            SetDefault(HighwaySetting.LaneGuides, true);
            SetDefault(HighwaySetting.SkinHitTarget, false);
            SetDefault(HighwaySetting.NoteStyle, defaults.NoteStyle);
            SetDefault(HighwaySetting.HitEffects, defaults.HitEffects);
            SetDefault(HighwaySetting.Track, defaults.Track);
            SetDefault(HighwaySetting.KeyArea, defaults.KeyArea);
            SetDefault(HighwaySetting.StageFit, defaults.StageFit);
            SetDefault(HighwaySetting.SideStagePull, defaults.SideStagePull, 0f, 1f, 0.01f);
            SetDefault(HighwaySetting.SideStageLean, defaults.SideStageLean, 0f, 1f, 0.01f);
            SetDefault(HighwaySetting.SkinLayoutDefaultApplied, false);
            SetDefault(HighwaySetting.SkinNoteProportions, defaults.SkinNoteProportions);
            SetDefault(HighwaySetting.FarFade, defaults.FarFade, 0f, HighwaySettings.MAX_FAR_FADE, 0.01f);
            SetDefault(HighwaySetting.HoldOpacity, defaults.HoldOpacity, HighwaySettings.MIN_HOLD_OPACITY, 1f, 0.01f);
            SetDefault(HighwaySetting.TrackOpacity, defaults.TrackOpacity, 0f, 1f, 0.01f);
            SetDefault(HighwaySetting.KeyBeam, defaults.KeyBeam);
            SetDefault(HighwaySetting.KiaiSway, defaults.KiaiSway, 0f, 1f, 0.01f);
            SetDefault(HighwaySetting.KiaiLift, defaults.KiaiLift, 0f, 1f, 0.01f);
            SetDefault(HighwaySetting.WallScale, defaults.WallScale);
            SetDefault(HighwaySetting.ShareManiaKeys, true);
            SetDefault(HighwaySetting.SpeedScale, defaults.SpeedScale);
            SetDefault(HighwaySetting.WidthMode, defaults.WidthMode);
            SetDefault(HighwaySetting.LaneWidth, defaults.LaneWidth, HighwaySettings.MIN_LANE_WIDTH, HighwaySettings.MAX_LANE_WIDTH, 0.001f);
            SetDefault(HighwaySetting.SkinPortDefaultsApplied, false);
            SetDefault(HighwaySetting.PresetScalesApplied, false);
            SetDefault(HighwaySetting.FineScrollSpeed, 0.0, 0, SpeedScales.MAX_MANIA_SPEED, SpeedScales.MANIA_SPEED_PRECISION);
        }

        /// <summary>
        /// The current values as one snapshot.
        /// </summary>
        public HighwaySettings Current => new HighwaySettings
        {
            CameraMode = Get<HighwayCameraMode>(HighwaySetting.CameraMode),
            LaneTilt = Get<float>(HighwaySetting.LaneTilt),
            LaneLength = Get<float>(HighwaySetting.LaneLength),
            Approach = Get<float>(HighwaySetting.Approach),
            JudgementLineHeight = Get<float>(HighwaySetting.JudgementLineHeight),
            TrackWidth = Get<float>(HighwaySetting.TrackWidth),
            Wall = Get<float>(HighwaySetting.Wall),
            NoteThickness = Get<float>(HighwaySetting.NoteThickness),
            WallKeepsVisibleTime = Get<bool>(HighwaySetting.WallKeepsVisibleTime),
            LaneGuides = Get<bool>(HighwaySetting.LaneGuides),
            SkinHitTarget = Get<bool>(HighwaySetting.SkinHitTarget),
            NoteStyle = Get<HighwayNoteStyle>(HighwaySetting.NoteStyle),
            HitEffects = Get<HighwayHitEffectStyle>(HighwaySetting.HitEffects),
            Track = Get<HighwayTrackStyle>(HighwaySetting.Track),
            KeyArea = Get<SkinKeyArea>(HighwaySetting.KeyArea),
            StageFit = Get<SkinStageFit>(HighwaySetting.StageFit),
            SideStagePull = Get<float>(HighwaySetting.SideStagePull),
            SideStageLean = Get<float>(HighwaySetting.SideStageLean),
            SkinNoteProportions = Get<bool>(HighwaySetting.SkinNoteProportions),
            FarFade = Get<float>(HighwaySetting.FarFade),
            HoldOpacity = Get<float>(HighwaySetting.HoldOpacity),
            TrackOpacity = Get<float>(HighwaySetting.TrackOpacity),
            KeyBeam = Get<KeyBeamLength>(HighwaySetting.KeyBeam),
            KiaiSway = Get<float>(HighwaySetting.KiaiSway),
            KiaiLift = Get<float>(HighwaySetting.KiaiLift),
            WallScale = Get<WallScale>(HighwaySetting.WallScale),
            SpeedScale = Get<SpeedScale>(HighwaySetting.SpeedScale),
            WidthMode = Get<HighwayWidthMode>(HighwaySetting.WidthMode),
            LaneWidth = Get<float>(HighwaySetting.LaneWidth),
        };

        /// <summary>
        /// Writes a preset's look into the individual settings: camera (lane tilt, length and note approach), width, line and
        /// note thickness, and takes the wall down for a game without one (a game with one leaves it to the player). Leaves
        /// <see cref="HighwaySetting.WallKeepsVisibleTime"/>, <see cref="HighwaySetting.LaneGuides"/>,
        /// <see cref="HighwaySetting.SkinHitTarget"/>, <see cref="HighwaySetting.NoteStyle"/>, <see cref="HighwaySetting.HitEffects"/>,
        /// <see cref="HighwaySetting.Track"/>, <see cref="HighwaySetting.FarFade"/>, <see cref="HighwaySetting.HoldOpacity"/>,
        /// <see cref="HighwaySetting.TrackOpacity"/>, <see cref="HighwaySetting.KeyBeam"/>, <see cref="HighwaySetting.KiaiSway"/>,
        /// <see cref="HighwaySetting.KiaiLift"/> and the skin porter's settings alone (preferences, not part of a game's look). The game's speed and wall scales are
        /// <see cref="ApplyPresetScales"/>.
        /// </summary>
        public void ApplyPreset(HighwayPreset preset)
        {
            if (preset == HighwayPreset.Custom)
                return;

            var values = HighwayPresets.Get(preset);

            bool wasApplying = applyingPreset;
            applyingPreset = true;

            try
            {
                SetValue(HighwaySetting.CameraMode, values.CameraMode);
                SetValue(HighwaySetting.LaneTilt, values.LaneTilt);
                SetValue(HighwaySetting.LaneLength, values.LaneLength);
                SetValue(HighwaySetting.Approach, values.Approach);
                SetValue(HighwaySetting.JudgementLineHeight, values.JudgementLineHeight);
                SetValue(HighwaySetting.TrackWidth, values.TrackWidth);
                SetValue(HighwaySetting.NoteThickness, values.NoteThickness);
                SetValue(HighwaySetting.WidthMode, values.WidthMode);
                SetValue(HighwaySetting.LaneWidth, values.LaneWidth);

                // A game with a wall leaves it to the player, as the arcade games do; one without takes it down.
                if (!HighwayPresets.HasWall(preset))
                    SetValue(HighwaySetting.Wall, 0f);
            }
            finally
            {
                applyingPreset = wasApplying;
            }
        }

        /// <summary>
        /// Shows the speed and the wall in a preset's game's numbers (<see cref="HighwaySettings.SpeedScale"/>,
        /// <see cref="HighwaySettings.WallScale"/>; the wall's only if the game has one). Display only: the speed, the wall and
        /// whether it keeps the visible time stay as they are.
        /// </summary>
        public void ApplyPresetScales(HighwayPreset preset)
        {
            if (preset == HighwayPreset.Custom)
                return;

            var values = HighwayPresets.Get(preset);

            SetValue(HighwaySetting.SpeedScale, values.SpeedScale);

            // A game without a wall leaves the wall's units as they are, for the next game with one.
            if (!HighwayPresets.HasWall(preset))
                return;

            // The wall's speed behaviour stays the player's (SPEC 4: visible time kept by default), unlike choosing a scale.
            bool wasApplying = applyingPreset;
            applyingPreset = true;

            try
            {
                SetValue(HighwaySetting.WallScale, values.WallScale);
            }
            finally
            {
                applyingPreset = wasApplying;
            }
        }

        /// <summary>
        /// Whether the current look is still the preset's. The wall counts only for a game without one (it must stay down); the
        /// lane width only when the preset sets the width by lane (it is hidden and unused otherwise); the lane length and the
        /// note approach only under a perspective camera (a parallel one ignores them). The track width always counts: ported
        /// and warped skin stages use it in every width mode.
        /// </summary>
        private bool matches(HighwayPreset presetName)
        {
            var preset = HighwayPresets.Get(presetName);
            var current = Current;
            bool wallMatches = HighwayPresets.HasWall(presetName) || near(current.Wall, 0);

            // Flat ignores the geometry, so any geometry matches the osu!mania preset.
            if (preset.CameraMode == HighwayCameraMode.Flat)
                return current.CameraMode == HighwayCameraMode.Flat && wallMatches;

            bool perspective = preset.CameraMode == HighwayCameraMode.Perspective;

            return current.CameraMode == preset.CameraMode
                   && near(current.LaneTilt, preset.LaneTilt)
                   && (!perspective || (near(current.LaneLength, preset.LaneLength) && near(current.Approach, preset.Approach)))
                   && near(current.JudgementLineHeight, preset.JudgementLineHeight)
                   && near(current.TrackWidth, preset.TrackWidth)
                   && current.WidthMode == preset.WidthMode
                   && (preset.WidthMode != HighwayWidthMode.Lane || near(current.LaneWidth, preset.LaneWidth))
                   && wallMatches
                   && near(current.NoteThickness, preset.NoteThickness);
        }

        private static bool near(float a, float b) => Math.Abs(a - b) < 1e-4f;
    }

    public enum HighwaySetting
    {
        Preset,
        CameraMode,

        /// <summary>
        /// Not shown: the far end's width before <see cref="LaneTilt"/> and <see cref="LaneLength"/> replaced it, read once to
        /// keep a Custom camera's shape (<c>keepCustomCameraAsTilt</c>).
        /// </summary>
        FarWidthRatio,

        JudgementLineHeight,

        /// <summary>
        /// Not shown: the far edge's height before <see cref="LaneTilt"/> and <see cref="LaneLength"/> replaced it, read once to
        /// keep a Custom camera's shape (<c>keepCustomCameraAsTilt</c>).
        /// </summary>
        FarEdgeHeight,

        TrackWidth,
        Wall,
        WallKeepsVisibleTime,
        LaneGuides,
        SkinHitTarget,
        NoteStyle,
        NoteThickness,
        HitEffects,
        Track,
        KeyArea,
        StageFit,
        SkinNoteProportions,
        FarFade,
        HoldOpacity,
        TrackOpacity,
        KeyBeam,
        KiaiSway,
        WallScale,
        SideStagePull,
        SideStageLean,

        /// <summary>
        /// The units the scroll speed is set and shown in (<see cref="Highway.SpeedScale"/>); the speed itself stays osu!mania's
        /// scroll speed setting.
        /// </summary>
        SpeedScale,

        /// <summary>
        /// Whether the track width or the lane width sets the highway track's width (<see cref="HighwaySettings.WidthMode"/>).
        /// </summary>
        WidthMode,

        /// <summary>
        /// One lane's width at the judgement line in lane mode (<see cref="HighwaySettings.LaneWidth"/>).
        /// </summary>
        LaneWidth,

        /// <summary>
        /// Whether maniaortho plays with the keys bound for osu!mania, the two key binding sections kept equal
        /// (<see cref="KeyBindingMirror"/>), or with its own.
        /// </summary>
        ShareManiaKeys,

        /// <summary>
        /// Not shown: whether <see cref="HighwayConfigManager.ApplySkinPortDefaults"/> has run on this install.
        /// </summary>
        SkinPortDefaultsApplied,

        /// <summary>
        /// Not shown: whether the preset in use has set its game's speed and wall scales on this install
        /// (<c>applyPresetScalesOnce</c>, Alpha 2026-11-08).
        /// </summary>
        PresetScalesApplied,

        /// <summary>
        /// Not shown: osu!mania's scroll speed at maniaortho's precision and range (0 before it is first stored), which
        /// osu!mania's limits would round and cap as they read it (<see cref="ManiaOrthoRulesetConfigManager"/>).
        /// </summary>
        FineScrollSpeed,

        /// <summary>
        /// Not shown: whether the one-time move of a stored "Skin position" stage fit to the skin's own width
        /// (<c>applySkinLayoutDefault</c>, Alpha 2026-11-04) has run on this install.
        /// </summary>
        SkinLayoutDefaultApplied,

        /// <summary>
        /// How far the lane leans back, in degrees (<see cref="HighwaySettings.LaneTilt"/>).
        /// </summary>
        LaneTilt,

        /// <summary>
        /// How long the lane is, in screen heights (<see cref="HighwaySettings.LaneLength"/>).
        /// </summary>
        LaneLength,

        /// <summary>
        /// How notes travel down the lane (<see cref="HighwaySettings.Approach"/>).
        /// </summary>
        Approach,

        /// <summary>
        /// How far the lane lifts in kiai sections (<see cref="HighwaySettings.KiaiLift"/>).
        /// </summary>
        KiaiLift,

        /// <summary>
        /// Not shown: whether a Custom camera's far edge has been turned into a lane tilt and length on this install
        /// (<c>keepCustomCameraAsTilt</c>, Alpha 2026-11-08).
        /// </summary>
        LaneTiltApplied,
    }
}
