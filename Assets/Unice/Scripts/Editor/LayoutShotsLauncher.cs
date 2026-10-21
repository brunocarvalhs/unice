using UnityEditor;

namespace Unice
{
    /// <summary>Starts Play mode for the layout screenshots (see <c>LayoutShots</c>): <c>Unity -batchmode -projectPath . -executeMethod Unice.LayoutShotsLauncher.Run -unice-shots out</c>.</summary>
    public static class LayoutShotsLauncher
    {
        /// <summary>
        /// Entering Play from the command line with the domain reload disabled forced a reload anyway, with the App already created and left half built.
        /// So this run enters with the normal reload, and puts the project's setting back when Play ends.
        /// </summary>
        public static void Run()
        {
            bool optionsEnabled = EditorSettings.enterPlayModeOptionsEnabled;
            EditorSettings.enterPlayModeOptionsEnabled = false;
            EditorApplication.playModeStateChanged += change =>
            {
                if (change == PlayModeStateChange.ExitingPlayMode) EditorSettings.enterPlayModeOptionsEnabled = optionsEnabled;
            };
            EditorApplication.isPlaying = true;
        }
    }
}
