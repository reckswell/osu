// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;

namespace osu.Game.Rulesets.ManiaOrtho.UI
{
    /// <summary>
    /// A heading over a group of this ruleset's settings (Alpha 2026-11-08: "nice and organized"), or, larger, over a section
    /// of groups. While the settings are searched it shows only when its own name matches.
    /// </summary>
    public partial class SettingsGroupHeader : CompositeDrawable, IFilterable
    {
        public LocalisableString Text { get; }

        /// <summary>
        /// Whether it heads a section of groups rather than one group.
        /// </summary>
        public bool Section { get; }

        private bool matchingFilter = true;

        public SettingsGroupHeader(LocalisableString text, bool section = false)
        {
            Text = text;
            Section = section;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider? colourProvider, OsuColour colours)
        {
            InternalChild = new OsuSpriteText
            {
                Text = Text,
                Font = OsuFont.GetFont(size: Section ? 24 : 16, weight: FontWeight.Bold),
                Colour = Section ? colourProvider?.Content1 ?? colours.Gray9 : colourProvider?.Highlight1 ?? colours.Yellow,
                Margin = new MarginPadding { Top = Section ? 28 : 12, Bottom = Section ? 0 : 2 },
                Padding = SettingsPanel.CONTENT_PADDING,
            };
        }

        public IEnumerable<LocalisableString> FilterTerms => new[] { Text };

        public bool MatchingFilter
        {
            set
            {
                if (matchingFilter == value)
                    return;

                matchingFilter = value;
                Invalidate(Invalidation.Presence);
            }
        }

        public bool FilteringActive { get; set; }

        public override bool IsPresent => base.IsPresent && (!FilteringActive || matchingFilter);
    }
}
