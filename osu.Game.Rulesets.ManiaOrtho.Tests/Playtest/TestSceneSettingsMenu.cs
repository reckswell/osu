// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.ManiaOrtho.Configuration;
using osu.Game.Rulesets.ManiaOrtho.Highway;
using osu.Game.Rulesets.ManiaOrtho.UI;
using osu.Game.Tests.Visual;
using osuTK.Input;

namespace osu.Game.Rulesets.ManiaOrtho.Tests.Playtest
{
    /// <summary>
    /// Goes through maniaortho's settings in the full game's settings overlay with the mouse, as a player would (Alpha
    /// 2026-11-08: "extensively check through the menu"): the headings, the game list, SDVX without a wall, Chunithm's wall
    /// back after SDVX, the folds and a search. Under the playtest harness each stop is captured:
    /// <c>dotnet osu.Game.Rulesets.ManiaOrtho.Tests.dll playtest --scene TestSceneSettingsMenu --out dir --size 1280x960</c>.
    /// </summary>
    public partial class TestSceneSettingsMenu : OsuGameTestScene
    {
        private const string ortho = "maniaortho";

        [Resolved]
        private GameHost host { get; set; } = null!;

        private readonly PlaytestOptions options = PlaytestOptions.FromEnvironment();

        private HighwayConfigManager highway = null!;

        private int captures;

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("select maniaortho", () => Game.Ruleset.Value = Game.Dependencies.Get<RulesetStore>().GetRuleset(ortho)!);
            AddStep("its settings, as a fresh install", () =>
            {
                var ruleset = Game.Ruleset.Value.CreateInstance();
                highway = ((ManiaOrthoRulesetConfigManager)Game.Dependencies.Get<IRulesetConfigCache>().GetConfigFor(ruleset)!).Highway;
                highway.SetValue(HighwaySetting.Preset, HighwayPreset.Chunithm);
                highway.ForgetGames();
            });
            AddStep("open settings", () => Game.Settings.Show());
            AddUntilStep("maniaortho's settings loaded", () => Game.Settings.ChildrenOfType<ManiaOrthoSettingsSubsection>().Any(s => s.IsLoaded));
        }

        [Test]
        public void TestMenu()
        {
            scrollTo(() => group("Settings: orthomania"));
            capture("orthomania settings");
            AddAssert("orthomania's settings first, then the game preset", () => sections(), () => Is.EqualTo(new[] { "Settings: orthomania", "Game preset" }));

            scrollTo(() => group("Game preset"));
            capture("game preset, Chunithm");

            AddStep("a wall in Chunithm", () => highway.SetValue(HighwaySetting.Wall, 0.3f));

            clickOn(() => gameDropdown.ChildrenOfType<DropdownHeader>().Single(), "open the game list");
            // Alpha 2026-11-08: "i didn't see an option for project sekai, just sdvx and chunithm".
            AddUntilStep("every game listed, Project Sekai too", () => gameItems().Select(i => i.Item.Text.Value.ToString()),
                () => Is.SupersetOf(new[] { "Chunithm", "SDVX", "Project Sekai", "beatmania IIDX", "DJMAX", "Quaver", "StepMania", "osu!mania", "Custom" }));
            capture("game list");

            pick("SDVX");
            AddAssert("SDVX: no wall", () => (highway.Get<float>(HighwaySetting.Wall), group("Wall").IsPresent), () => Is.EqualTo((0f, false)));
            capture("game preset, SDVX");

            clickOn(() => gameDropdown.ChildrenOfType<DropdownHeader>().Single(), "open the game list");
            pick("Project Sekai");
            AddAssert("Project Sekai: no wall", () => group("Wall").IsPresent, () => Is.False);
            capture("game preset, Project Sekai");

            clickOn(() => gameDropdown.ChildrenOfType<DropdownHeader>().Single(), "open the game list");
            pick("Chunithm");
            AddAssert("Chunithm: its wall back", () => (highway.Get<float>(HighwaySetting.Wall), group("Wall").IsPresent), () => Is.EqualTo((0.3f, true)));
            scrollTo(() => group("Wall"));
            capture("Chunithm, its wall back");

            scrollTo(() => group("Other settings"));
            AddAssert("other settings folded", () => group("Other settings").ContentShown, () => Is.False);
            clickOn(() => group("Other settings").ChildrenOfType<OsuClickableContainer>().First(), "unfold other settings");
            AddUntilStep("unfolded", () => group("Other settings").ContentShown);
            scrollTo(() => group("Other settings"));
            capture("other settings unfolded");

            AddStep("search \"angle\"", () => searchBox.Current.Value = "angle");
            AddUntilStep("lane tilt found, and nothing else of ours", () => (onScreen(setting("Lane tilt")), onScreen(setting("Judgement line height")), onScreen(setting("Notes"))),
                () => Is.EqualTo((true, false, false)));
            AddStep("to the top", () => Game.Settings.SectionsContainer.ScrollToTop());
            AddWaitStep("settle", 5);
            capture("search angle");

            AddStep("search \"pjsk\"", () => searchBox.Current.Value = "pjsk");
            AddUntilStep("the game list found", () => (onScreen(gameDropdown), onScreen(setting("Lane tilt"))), () => Is.EqualTo((true, false)));
            AddWaitStep("settle", 5);
            capture("search pjsk");

            AddStep("clear the search", () => searchBox.Current.Value = string.Empty);
        }

        /// <summary>
        /// The settings overlay's own search box (its key binding panel has another, hidden).
        /// </summary>
        private SettingsSearchTextBox searchBox => Game.Settings.ChildrenOfType<SettingsSearchTextBox>().Single(onScreen);

        private SettingsItemV2 setting(string caption) => subsection.ChildrenOfType<SettingsItemV2>().Single(i => i.Control.FilterTerms.Any(t => t.ToString() == caption));

        private ManiaOrthoSettingsSubsection subsection => Game.Settings.ChildrenOfType<ManiaOrthoSettingsSubsection>().Single();

        private FormEnumDropdown<HighwayPreset> gameDropdown => subsection.ChildrenOfType<FormEnumDropdown<HighwayPreset>>().Single();

        private OrthoSettingsGroup group(string title) => subsection.ChildrenOfType<OrthoSettingsGroup>().Single(g => g.Title.ToString() == title);

        private string[] sections() => subsection.ChildrenOfType<OrthoSettingsGroup>().Where(g => g.Style == SettingsGroupStyle.Section).Select(g => g.Title.ToString()).ToArray();

        private IEnumerable<Menu.DrawableMenuItem> gameItems() => gameDropdown.ChildrenOfType<Menu.DrawableMenuItem>().Where(i => i.IsPresent);

        private void pick(string game)
        {
            AddUntilStep("the game list open", () => gameItems().Any());
            clickOn(() => gameItems().Single(i => i.Item.Text.Value.ToString() == game), $"pick {game}");
            AddUntilStep($"{game} chosen", () => gameDropdown.Current.Value.GetDescription(), () => Is.EqualTo(game));
            AddWaitStep("settle", 3);
        }

        private void scrollTo(System.Func<Drawable> target)
        {
            AddStep("scroll", () => Game.Settings.SectionsContainer.ScrollTo(target()));
            AddWaitStep("scrolled", 10);
        }

        private void clickOn(System.Func<Drawable> target, string what)
        {
            AddStep(what, () =>
            {
                InputManager.MoveMouseTo(target());
                InputManager.Click(MouseButton.Left);
            });
        }

        private static bool onScreen(Drawable drawable)
        {
            for (Drawable? d = drawable; d != null; d = d.Parent)
            {
                if (!d.IsPresent)
                    return false;
            }

            return true;
        }

        private void capture(string name)
        {
            Task? task = null;

            AddStep($"capture {name}", () =>
            {
                // No window (headless run) or no output folder: nothing to capture.
                if (options.OutputDirectory == null || host.Window == null)
                    return;

                Directory.CreateDirectory(options.OutputDirectory);
                string path = Path.Combine(options.OutputDirectory, $"{++captures:00} {name}.png");

                task = Task.Run(async () =>
                {
                    using (var image = await host.TakeScreenshotAsync().ConfigureAwait(false))
                        await SixLabors.ImageSharp.ImageExtensions.SaveAsPngAsync(image, path).ConfigureAwait(false);
                });
            });
            AddUntilStep($"{name} written", () => task == null || task.IsCompleted);
            AddAssert($"{name} not failed", () => task?.IsFaulted != true);
        }
    }
}
