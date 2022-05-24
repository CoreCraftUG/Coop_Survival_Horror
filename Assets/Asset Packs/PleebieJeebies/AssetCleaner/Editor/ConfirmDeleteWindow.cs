using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ConfirmDeleteWindow : EditorWindow
{
    private static GUIStyle deleteStyle, buttonStyle;

    public static void ShowWindow(Rect position)
    {
        deleteStyle = new GUIStyle(EditorStyles.miniButton);
        deleteStyle.hover.textColor = Color.red;
        buttonStyle = new GUIStyle(EditorStyles.miniButton);
        var window = GetWindow<ConfirmDeleteWindow>();
        window.titleContent = new GUIContent("Confirm Delete ?");
        Vector2 size = new Vector2(600, 180);
        window.minSize = size;
        window.maxSize = size;
        window.position = position;
        window.Show();
    }

    private void OnGUI()
    {
        ShowNotification(new GUIContent("Are you sure you want to delete the assets ?"));
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("CANCEL", buttonStyle))
            {
                if (AssetCleaner.useDebugging) Debug.Log("Delete Not Confirmed - Asset Clean Cancelled.");
                this.Close();
            } 
            if (GUILayout.Button("DELETE ASSETS", deleteStyle))
            {
                int toDelete = CountSelected();
                DeleteAssets();
                if (AssetCleaner.useDebugging) Debug.Log("Deleted " + toDelete + " Assets.");
                this.Close();
                if (AssetCleaner.hasProtectedAssets == true) EditorUtility.DisplayDialog("Protected Assets Not Deleted", "Some assets were not deleted due to being dependancies of excluded files or folders.", "OK"); 
            }
        }
    }

    private int CountSelected()
    {
        int returnValue = 0;
        for (int i = 0; i < CleaningTreeView.itemSelections.Length; i++)
        {
            if (CleaningTreeView.itemSelections[i].Selected) returnValue++;
        }
        return returnValue;
    }

    private void DeleteAssets()
    {
        bool debug = false;
        if (AssetCleaner.useDebugging && SettingsWindow.DebugAssetFilesDelete) debug = true;
        float progress = 0f;
        float j;
        float k = CleaningTreeView.itemSelections.Length;
        float startTime = Time.realtimeSinceStartup;
        float timer, minutes;
        int minutesNoDecimals, minutesToSeconds, secondsLeft;
        string progressTitle = "Deleting";
        string path = "";
        bool after2019 = Application.unityVersion.StartsWith("202");
        for (int i = 0; i < CleaningTreeView.itemSelections.Length; i++)
        {
            ItemSelection item = CleaningTreeView.itemSelections[i];
            string name = CleaningTreeView.treeViewItems[i].displayName;
            if (debug) Debug.Log("Unused Asset = " + name);
            if (item.Selected)
            {
                if (debug) Debug.Log("Item was selected for deletion.");
                path = AssetCleaner.unusedAssetPathNames[item.ID];
                if (debug) Debug.Log("Item Path = " + path);
                if (!AssetDatabase.IsValidFolder(path))
                {
                    if (debug) Debug.Log("Asset is not a folder so will be deleted.");
                    j = i;
                    progress = j / k;
                    timer = Time.realtimeSinceStartup - startTime;
                    timer = Mathf.FloorToInt(timer);
                    progressTitle = "Deleting (busy for " + timer + "s)...";                    
                    if (after2019)
                    {
                        if (timer >= 10) progressTitle = "Deleting ";
                        EditorUtility.ClearProgressBar();
                        EditorUtility.DisplayProgressBar(progressTitle, path, progress);
                    }
                    else
                    {
                        if (timer >= 60)
                        {
                            minutes = timer / 60;
                            minutesNoDecimals = Mathf.FloorToInt(minutes);
                            minutesToSeconds = minutesNoDecimals * 60;
                            secondsLeft = (int)(timer - minutesToSeconds);
                            if (secondsLeft < 10)
                            {
                                progressTitle = "Deleting (busy for " + minutesNoDecimals + ":0" + secondsLeft + ")...";
                            }
                            else
                            {
                                progressTitle = "Deleting (busy for " + minutesNoDecimals + ":" + secondsLeft + ")...";
                            }
                        }
                        if (EditorUtility.DisplayCancelableProgressBar(progressTitle, path, progress)) break;
                    }
                    AssetDatabase.DeleteAsset(path);
                }
            }
            else
            {
                if (debug) Debug.Log("Item was not selected for deletion.");
                if (after2019) EditorUtility.ClearProgressBar();
                if (after2019) if (EditorUtility.DisplayCancelableProgressBar(progressTitle, path, progress)) break;
            }
        }        
        if (SettingsWindow.DeleteEmptyFolders) DeleteEmptyFolders();
        EditorUtility.ClearProgressBar();
        
    }

    private void DeleteEmptyFolders()
    {
        bool debug = false;
        if (AssetCleaner.useDebugging && SettingsWindow.DebugEmptyFoldersDelete) debug = true;
        if (debug) Debug.Log("Starting Search for Empty Folders.....");

        string[] searchFolders = new string[1];
        searchFolders[0] = "Assets";
        List<string> folders = new List<string>();
        foreach (var guid in AssetDatabase.FindAssets("", searchFolders))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (AssetDatabase.IsValidFolder(path))
            {
                folders.Add(path);
            }
        }
        if (debug)
        {
            Debug.Log("Found Folders :");
            foreach (string s in folders)
            {
                Debug.Log(s);
            }
        } 
        for (int i = folders.Count - 1; i >= 0; i--)
        {
            if (debug) Debug.Log("Folder Path = " + folders[i]);
            string[] searchFolder = new string[1];
            searchFolder[0] = folders[i];
            string[] assetsInFolder = AssetDatabase.FindAssets("", searchFolder);
            if (assetsInFolder.Length > 0)
            {
                if (debug) Debug.Log("Folder contains assets and will not be removed.");
            }
            else
            {
                if (debug) Debug.Log("Folder is empty and will be removed.");
                AssetDatabase.DeleteAsset(folders[i]);
            }
        }
    }
}
