using UnityEngine;
using UnityEditor;

public class Uninstall : MonoBehaviour
{
    private static bool debug;

    [MenuItem("Tools/Asset Cleaner/Uninstall", false, 1000)]
    private static void RemoveCleaner()
    {
        int option = EditorUtility.DisplayDialogComplex("Uninstall Asset Cleaner", "Are you sure you want to uninstall the Pleebie Jeebies Asset Cleaner?", "UNINSTALL", "CANCEL", "");
        switch (option)
        {
            // Uninstall.
            case 0:
                if(AssetCleaner.useDebugging) Debug.Log("UNINSTALL STARTED");
                UninstallCleaner();
                break;

            // Cancel.
            case 1:
                if (AssetCleaner.useDebugging) Debug.Log("CANCELLED UNINSTALL");
                break;

            default:
                Debug.LogError("Unrecognized option.");
                break;
        }
    }

    private static void UninstallCleaner()
    {
        debug = AssetCleaner.useDebugging;
        DeleteFiles();
        DeleteFolder();
        CleanupRegistry();
        EditorUtility.DisplayDialog("Uninstall Asset Cleaner", "Uninstall Complete", "OK");
    }

    private static void DeleteFiles()
    {
        if (debug) Debug.Log("Deleting Asset Cleaner Files.");
        string[] searchFolders = new string[1];
        searchFolders[0] = "Assets/PleebieJeebies/AssetCleaner";
        string[] assetGUIDs = AssetDatabase.FindAssets("", searchFolders);
        for (int i = 0; i < assetGUIDs.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);
            AssetDatabase.DeleteAsset(path);
        }
    }

    private static void DeleteFolder()
    {
        if (debug) Debug.Log("Removing Asset Cleaner Folder");
        AssetDatabase.DeleteAsset("Assets/PleebieJeebies/AssetCleaner");
        //check to see if pleebie jeebies is empty to be removed as well
        string[] searchFolder = new string[1];
        string pj = "Assets/PleebieJeebies";
        searchFolder[0] = pj;
        string[] assetsInFolder = AssetDatabase.FindAssets("", searchFolder);
        if (assetsInFolder.Length == 0)
        {
            AssetDatabase.DeleteAsset(pj);
        }
    }

    private static void CleanupRegistry()
    {
        if (debug) Debug.Log("Cleaning Up Registry.");
        EditorPrefs.DeleteKey("Asset Cleaner/Console Logging");
        EditorPrefs.DeleteKey("debugSceneAndAssetSearch");
        EditorPrefs.DeleteKey("debugUsedAssetScan");
        EditorPrefs.DeleteKey("debugTreeViewConstruction");
        EditorPrefs.DeleteKey("debugAssetFilesDelete");
        EditorPrefs.DeleteKey("debugEmptyFoldersDelete");
        EditorPrefs.DeleteKey("excludeScripts");
        EditorPrefs.DeleteKey("excludeDocuments");
        EditorPrefs.DeleteKey("documentExtensions");
        EditorPrefs.DeleteKey("excludeCustomFileExtensions");
        EditorPrefs.DeleteKey("customExclusions");
        string key = SettingsHelper.GetProjectKeyName("excludedFolders");
        EditorPrefs.DeleteKey(key);
        EditorPrefs.DeleteKey("deleteEmptyFolders");
        EditorPrefs.DeleteKey("noPopupOnLaunch");
        EditorPrefs.DeleteKey("assetCleanerPrefsInitialized");
    }
}
