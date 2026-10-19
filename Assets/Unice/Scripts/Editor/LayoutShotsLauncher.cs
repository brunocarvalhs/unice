using UnityEditor;

namespace Unice
{
    /// <summary>Starts Play mode for the layout screenshots (see <c>LayoutShots</c>): <c>Unity -batchmode -projectPath . -executeMethod Unice.LayoutShotsLauncher.Run -unice-shots out</c>.</summary>
    public static class LayoutShotsLauncher
    {
        public static void Run() => EditorApplication.isPlaying = true;
    }
}
