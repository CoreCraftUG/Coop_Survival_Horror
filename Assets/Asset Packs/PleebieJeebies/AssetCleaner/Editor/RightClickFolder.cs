using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RightClickFolder : MonoBehaviour
{
    public static bool useFolderFilter = false;
    public static List<string> pathNames = null;

    [MenuItem("Assets/Scan Selected Folder(s) For Unused Assets", false, 2000)]
    private static void GetFolders()
    {
        string[] selectedFolders = Selection.assetGUIDs;
        pathNames = new List<string>();
        string path;
        foreach (string s in selectedFolders)
        {
            path = AssetDatabase.GUIDToAssetPath(s);
            if (AssetDatabase.IsValidFolder(path))
            {
                pathNames.Add(path);                
            }
        }
        if (pathNames.Count == 0) EditorUtility.DisplayDialog("Asset Cleaner", "No Folders Selected", "OK");
        else 
        {   
            //display scene select window with filtering on folders bool so can be used when clean is done
            useFolderFilter = true;
            AssetCleaner.FolderScanForScenes();
        }
    }

    [MenuItem("Tools/Asset Cleaner/Scan Selected Folder(s) for Unused Assets...", false, 2)]
    private static void ScanFolders()
    {
        GetFolders();
    }

}
