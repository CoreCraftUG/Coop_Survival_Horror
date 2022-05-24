using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class DebugOption {

    private const string MENU_NAME = "Tools/Asset Cleaner/Console Output";
    private static bool enabled_;

    static DebugOption()
    {
        enabled_ = EditorPrefs.GetBool(MENU_NAME, false);
        EditorApplication.delayCall += () => {
            PerformAction(enabled_);
        };
    }

    [MenuItem(MENU_NAME, false, 100)]
    private static void ToggleAction()
    {
        PerformAction(!enabled_);
    }

    public static void PerformAction(bool enabled)
    {
        /// Set checkmark on menu item
        Menu.SetChecked(MENU_NAME, enabled);
        /// Saving editor state
        EditorPrefs.SetBool(MENU_NAME, enabled);
        enabled_ = enabled;
        AssetCleaner.useDebugging = enabled;
        if (enabled) Debug.Log("Asset Cleaner : Console Ouput Enabled");
        else Debug.Log("Asset Cleaner : Console Output Disabled");
    }
}
