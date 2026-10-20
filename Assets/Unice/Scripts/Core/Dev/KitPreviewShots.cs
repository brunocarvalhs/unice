#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unice
{
    /// <summary>
    /// One-off visual check for the kit catalog migration: opens "Minha criança", cycles through a handful of kits
    /// (the default look plus one per special drawing behavior — cape, cap, beanie, ears, backpack, glasses, teddy)
    /// and saves a PNG of the page per kit. Run with <c>Unice.KitPreviewShotsLauncher.Run</c> (batchmode, no -quit)
    /// and <c>-unice-kit-shots &lt;folder&gt;</c>; does nothing otherwise. Not part of the shipped game.
    /// </summary>
    sealed class KitPreviewShots : MonoBehaviour
    {
        const int ShotWidth = 540, ShotHeight = 960;

        /// <summary>One shot: a label for the file name, and the kit (skin, hair, hairColor, outfit, outfitColor, hat, accessory).</summary>
        static readonly (string name, int[] kit)[] Shots =
        {
            ("default", new[] { 0, 0, 0, 0, 0, 0, 0 }),
            ("cape", new[] { 0, 0, 0, 4, 1, 0, 0 }),
            ("cap", new[] { 1, 1, 1, 0, 2, 1, 0 }),
            ("beanie", new[] { 2, 2, 2, 1, 3, 2, 0 }),
            ("ears", new[] { 3, 3, 3, 2, 4, 5, 0 }),
            ("backpack", new[] { 0, 1, 4, 3, 5, 3, 1 }),
            ("glasses", new[] { 1, 2, 0, 0, 6, 4, 2 }),
            ("teddy", new[] { 2, 3, 1, 1, 7, 0, 3 }),
        };

        string folder;
        App app;
        PanelSettings panel;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-unice-kit-shots");
            if (index < 0 || index + 1 >= args.Length) return;
            new GameObject("KitPreviewShots").AddComponent<KitPreviewShots>().folder = args[index + 1];
        }

        IEnumerator Start()
        {
            Directory.CreateDirectory(folder);
            while (App.Instance == null || App.Instance.ui == null) yield return null;
            app = App.Instance; panel = Resources.Load<PanelSettings>("UI/UnicePanel");
            yield return Wait(1);

            ClickByText("menu.mykid");
            yield return Wait(1);

            foreach (var (name, kit) in Shots)
            {
                app.SetKit(kit);
                yield return null; yield return null; yield return Wait(.1f);   // let the preview pick up the new kit
                yield return Capture(name);
            }

            Debug.Log("[KitPreviewShots] done: " + folder);
            EditorApplication.Exit(0);
        }

        static WaitForSeconds Wait(float seconds) => new WaitForSeconds(seconds);

        void ClickByText(string localizationKey)
        {
            string text = Localization.Get(localizationKey);
            var button = app.ui.root.Query<Button>().Where(each => each.text == text && !float.IsNaN(each.worldBound.width) && each.worldBound.width > 0).First();
            if (button == null) { Debug.LogWarning("[KitPreviewShots] no visible button: " + localizationKey); return; }
            using (var submit = NavigationSubmitEvent.GetPooled()) { submit.target = button; button.SendEvent(submit); }
        }

        IEnumerator Capture(string name)
        {
            var texture = new RenderTexture(ShotWidth, ShotHeight, 24, RenderTextureFormat.ARGB32);
            panel.targetTexture = texture; panel.clearColor = true; panel.colorClearValue = new Color(0.18f, 0.14f, 0.11f, 1);
            yield return null; yield return null; yield return new WaitForEndOfFrame();

            var previous = RenderTexture.active; RenderTexture.active = texture;
            var result = new Texture2D(ShotWidth, ShotHeight, TextureFormat.RGBA32, false);
            result.ReadPixels(new Rect(0, 0, ShotWidth, ShotHeight), 0, 0); result.Apply();
            RenderTexture.active = previous;

            File.WriteAllBytes(Path.Combine(folder, name + ".png"), result.EncodeToPNG());
            panel.targetTexture = null;
            Destroy(result); texture.Release();
        }
    }
}
#endif
