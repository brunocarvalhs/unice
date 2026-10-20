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
    /// Development tool: plays through the main screens like a user and saves one PNG per screen and per device shape, to
    /// review the layout without a phone in hand. Run it with <c>Unice.LayoutShotsLauncher.Run</c> (batchmode, no -quit) and
    /// <c>-unice-shots &lt;folder&gt;</c>; it does nothing otherwise. The UI and the world render into textures of each device's
    /// aspect (the panel is matched by height, so the layout depends on the aspect only), and the two are composed.
    /// </summary>
    sealed class LayoutShots : MonoBehaviour
    {
        /// <summary>Shots are this many pixels tall; the width follows the aspect.</summary>
        const int ShotHeight = 720;

        struct Device { public string name; public float aspect; public bool touch; }

        static readonly Device[] Devices =
        {
            new Device { name = "phone-20x9", aspect = 20f / 9, touch = true },
            new Device { name = "phone-16x9", aspect = 16f / 9, touch = true },
            new Device { name = "tablet-4x3", aspect = 4f / 3, touch = true },
            new Device { name = "laptop-16x10", aspect = 16f / 10, touch = false },
            new Device { name = "desktop-16x9", aspect = 16f / 9, touch = false },
            new Device { name = "ultrawide-21x9", aspect = 21f / 9, touch = false },
        };

        string folder;
        App app;
        PanelSettings panel;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-unice-shots");
            if (index < 0 || index + 1 >= args.Length) return;
            new GameObject("LayoutShots").AddComponent<LayoutShots>().folder = args[index + 1];
        }

        IEnumerator Start()
        {
            Directory.CreateDirectory(folder);
            while (App.Instance == null || App.Instance.ui == null) yield return null;
            app = App.Instance; panel = Resources.Load<PanelSettings>("UI/UnicePanel");
            yield return Wait(2);

            yield return Shoot("home");
            Click("menu.credits"); yield return Wait(3); yield return Shoot("credits");
            Click("menu.back"); yield return Wait(.5f);
            Click("menu.howto"); yield return Wait(.5f); yield return Shoot("howto");
            Click("menu.back");
            Click("menu.mykid"); yield return Wait(.5f); yield return Shoot("mykid");
            Click("menu.ready");
            Click("menu.online"); yield return Wait(.5f); yield return Shoot("online");
            Click("menu.back");

            app.PlaySolo(); yield return Wait(1.5f); yield return Shoot("lobby");
            Click("tab.appearance"); yield return Wait(.5f); yield return Shoot("lobby-appearance");
            Click("tab.match");
            app.game.hideTime = 6; app.game.huntTime = 8;
            app.StartMatch(5, 1);
            while (app.game.phase != Phase.Hide) yield return null;
            yield return Wait(2); yield return Shoot("hide");
            while (app.game.phase != Phase.Hunt) yield return null;
            yield return Wait(2); yield return Shoot("hunt");
            ClickNamed(Hud.MapButtonName); yield return Wait(.5f); yield return Shoot("hunt-map");
            ClickNamed(Hud.MapButtonName);
            app.TogglePause(); yield return Wait(.5f); yield return Shoot("pause"); app.TogglePause();
            while (app.game.phase != Phase.Result && app.game.phase != Phase.Final) yield return null;
            yield return Wait(2); yield return Shoot(app.game.phase == Phase.Final ? "final" : "result");
            Debug.Log("[LayoutShots] done: " + folder);
            EditorApplication.Exit(0);
        }

        static WaitForSeconds Wait(float seconds) => new WaitForSeconds(seconds);

        /// <summary>Presses the visible button with this text (a localization key), as the keyboard's Enter would.</summary>
        void Click(string key)
        {
            string text = Localization.Get(key);
            var button = app.ui.root.Query<Button>().Where(each => each.text == text && !float.IsNaN(each.worldBound.width) && each.worldBound.width > 0).First();
            if (button == null) { Debug.LogWarning("[LayoutShots] no visible button: " + key); return; }
            using (var submit = NavigationSubmitEvent.GetPooled()) { submit.target = button; button.SendEvent(submit); }
        }

        /// <summary>Same as <see cref="Click"/> for a button that has an element name instead of text (icon buttons).</summary>
        void ClickNamed(string elementName)
        {
            var button = app.ui.root.Q<Button>(elementName);
            if (button == null) { Debug.LogWarning("[LayoutShots] no button named: " + elementName); return; }
            using (var submit = NavigationSubmitEvent.GetPooled()) { submit.target = button; button.SendEvent(submit); }
        }

        IEnumerator Shoot(string state)
        {
            foreach (var device in Devices) yield return Capture(state, device);
        }

        IEnumerator Capture(string state, Device device)
        {
            int width = Mathf.RoundToInt(ShotHeight * device.aspect);
            var uiTexture = new RenderTexture(width, ShotHeight, 24, RenderTextureFormat.ARGB32);
            var worldTexture = new RenderTexture(width, ShotHeight, 24, RenderTextureFormat.ARGB32);
            app.ui.ForceTouch(device.touch);
            panel.targetTexture = uiTexture; panel.clearColor = true; panel.colorClearValue = Color.clear;
            app.cameraRig.Camera.targetTexture = worldTexture;
            yield return null; yield return null; yield return Wait(.25f);
            yield return new WaitForEndOfFrame();

            var ui = Read(uiTexture); var world = Read(worldTexture);
            var uiPixels = ui.GetPixels32(); var worldPixels = world.GetPixels32();
            for (int i = 0; i < worldPixels.Length; i++)
            {
                var top = uiPixels[i]; var back = worldPixels[i]; float alpha = top.a / 255f;
                worldPixels[i] = new Color32((byte)(top.r * alpha + back.r * (1 - alpha)), (byte)(top.g * alpha + back.g * (1 - alpha)), (byte)(top.b * alpha + back.b * (1 - alpha)), 255);
            }
            world.SetPixels32(worldPixels); world.Apply();
            File.WriteAllBytes(Path.Combine(folder, state + "_" + device.name + ".png"), world.EncodeToPNG());

            panel.targetTexture = null; app.cameraRig.Camera.targetTexture = null;
            Destroy(ui); Destroy(world); uiTexture.Release(); worldTexture.Release();
        }

        static Texture2D Read(RenderTexture texture)
        {
            var previous = RenderTexture.active; RenderTexture.active = texture;
            var result = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
            result.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0); result.Apply();
            RenderTexture.active = previous;
            return result;
        }
    }
}
#endif
