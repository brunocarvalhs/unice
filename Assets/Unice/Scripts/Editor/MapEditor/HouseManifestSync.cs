using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;

namespace Unice
{
    /// <summary>
    /// Keeps a house room's painted floor in <c>Resources/Art/manifest.json</c> lined up with its <see cref="Room"/> rect after
    /// the map editor moves or resizes it (map editor save, house map only). <see cref="SyncFloors"/> is pure (string in, string
    /// out, no file I/O) so it is unit tested directly; <see cref="SyncFile"/> is the thin wrapper that reads/writes the tracked asset.
    /// Surgical string replace only, never parse-and-reserialize the whole document: every floor entry in the shipped manifest is
    /// exactly <c>{"id":"&lt;id&gt;","file":"floor_&lt;id&gt;","x":N,"y":N,"w":N,"h":N}</c>, and reserializing the whole thing risks
    /// reordering keys or losing fields (<c>layers</c>, <c>kid</c>, <c>unice</c>, …) that have nothing to do with this.
    /// </summary>
    public static class HouseManifestSync
    {
        const string ManifestPath = "Assets/Unice/Resources/Art/manifest.json";

        /// <summary>A plain integer or decimal, captured so it can be swapped for the room's current value.</summary>
        const string NumberGroup = "(-?\\d+(?:\\.\\d+)?)";

        /// <summary>
        /// For each room, replaces its matching floor entry's x/y/w/h numbers in <paramref name="manifestJson"/> with the room's
        /// current values — only those four numbers; every other byte of the string is untouched. A room id with no matching
        /// <c>floor_&lt;id&gt;</c> entry (a brand-new room with no painted floor yet) is a no-op for that room, not an error.
        /// </summary>
        public static string SyncFloors(string manifestJson, Room[] rooms)
        {
            foreach (var room in rooms)
            {
                var pattern = new Regex(
                    "(\\{\"id\":\"" + Regex.Escape(room.id) + "\",\"file\":\"floor_" + Regex.Escape(room.id) + "\",\"x\":)" + NumberGroup +
                    "(,\"y\":)" + NumberGroup + "(,\"w\":)" + NumberGroup + "(,\"h\":)" + NumberGroup + "(\\})");

                var match = pattern.Match(manifestJson);
                if (!match.Success) continue;   // no painted floor for this room id (yet): leave the file alone

                string replacement = match.Groups[1].Value + FormatNumber(room.x) + match.Groups[3].Value + FormatNumber(room.y) +
                    match.Groups[5].Value + FormatNumber(room.w) + match.Groups[7].Value + FormatNumber(room.h) + match.Groups[9].Value;
                manifestJson = manifestJson.Substring(0, match.Index) + replacement + manifestJson.Substring(match.Index + match.Length);
            }
            return manifestJson;
        }

        /// <summary>Plain integer when the value is whole (every number in the shipped file is), otherwise the shortest round-trip decimal.</summary>
        static string FormatNumber(float value) =>
            value == MathF.Floor(value) ? ((long)value).ToString(CultureInfo.InvariantCulture) : value.ToString(CultureInfo.InvariantCulture);

        /// <summary>Reads, syncs and overwrites <c>Resources/Art/manifest.json</c> on disk (no-op if nothing changed), and refreshes it as a tracked asset.</summary>
        public static void SyncFile(Room[] rooms)
        {
            var noBom = new UTF8Encoding(false);
            string current = File.ReadAllText(ManifestPath, noBom);
            string updated = SyncFloors(current, rooms);
            if (updated == current) return;
            File.WriteAllText(ManifestPath, updated, noBom);
            AssetDatabase.Refresh();
        }
    }
}
