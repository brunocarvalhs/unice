using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unice
{
    /// <summary>
    /// Opens the home menu's UXML in the UI Builder once per Editor session, when the project opens (not in batch runs).
    /// Turn it off in the menu <c>Unice &gt; UI Builder ao abrir o projeto</c>; <c>Unice &gt; Abrir menu principal no UI Builder</c> opens it any time.
    /// </summary>
    [InitializeOnLoad]
    static class OpenMenuInUiBuilder
    {
        const string MenuPath = "Assets/Unice/Resources/UI/Screens/HomeMenu.uxml";
        const string OnOpenItem = "Unice/UI Builder ao abrir o projeto", OpenItem = "Unice/Abrir menu principal no UI Builder";
        const string OnOpenKey = "Unice.OpenMenuInUiBuilder", SessionKey = "Unice.MenuOpenedInUiBuilder";

        static OpenMenuInUiBuilder()
        {
            if (Application.isBatchMode || SessionState.GetBool(SessionKey, false) || !EditorPrefs.GetBool(OnOpenKey, true)) return;
            SessionState.SetBool(SessionKey, true);
            EditorApplication.delayCall += Open;
        }

        [MenuItem(OpenItem)]
        static void Open()
        {
            var menu = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(MenuPath);
            if (menu != null) AssetDatabase.OpenAsset(menu);   // what a double click on the file does: opens it in the UI Builder
        }

        [MenuItem(OnOpenItem)]
        static void Toggle() => EditorPrefs.SetBool(OnOpenKey, !EditorPrefs.GetBool(OnOpenKey, true));

        [MenuItem(OnOpenItem, true)]
        static bool ToggleChecked() { Menu.SetChecked(OnOpenItem, EditorPrefs.GetBool(OnOpenKey, true)); return true; }
    }
}
