// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;

namespace ManiaOrtho.Launcher
{
    /// <summary>
    /// Where the packaged game and its data live, all in the folder that holds the package (Alpha 2026-11-08: nothing goes
    /// into an osu! install). The launcher sits in the package folder with the game in <see cref="GAME_FOLDER"/> below it;
    /// the data is in <see cref="DATA_FOLDER"/> next to the package folder, so a new package (or a fresh clone) keeps it:
    /// <code>
    /// Documents\Projects\osu!\osu\osu.exe        the launcher
    /// Documents\Projects\osu!\osu\game\osu!.exe  the game
    /// Documents\Projects\osu!\ortho-data\        the game's data and screenshots, also used by scripts\ortho\play.ps1
    /// </code>
    /// </summary>
    public sealed class PackageLayout
    {
        public const string GAME_FOLDER = "game";
        public const string GAME_EXECUTABLE = "osu!.exe";
        public const string DATA_FOLDER = "ortho-data";

        /// <summary>
        /// The folder the launcher is in.
        /// </summary>
        public string Package { get; }

        /// <summary>
        /// The game's data folder (database, settings, imported files, replays, screenshots, logs).
        /// </summary>
        public string Data { get; }

        public string Game => Path.Combine(Package, GAME_FOLDER);

        public string GameExecutable => Path.Combine(Game, GAME_EXECUTABLE);

        /// <summary>
        /// Temporary files of the game, kept inside the package folder.
        /// </summary>
        public string Temp => Path.Combine(Package, "temp");

        public string Log => Path.Combine(Package, "launcher.log");

        public PackageLayout(string package, string data)
        {
            Package = trim(Path.GetFullPath(package));
            Data = trim(Path.GetFullPath(data));
        }

        /// <summary>
        /// The layout around a launcher in <paramref name="package"/>.
        /// </summary>
        public static PackageLayout ForPackage(string package)
        {
            string full = trim(Path.GetFullPath(package));
            string parent = Path.GetDirectoryName(full) ?? full;

            return new PackageLayout(full, Path.Combine(parent, DATA_FOLDER));
        }

        private static string trim(string path)
        {
            string root = Path.GetPathRoot(path) ?? string.Empty;
            return path.Length > root.Length ? path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) : path;
        }
    }
}
