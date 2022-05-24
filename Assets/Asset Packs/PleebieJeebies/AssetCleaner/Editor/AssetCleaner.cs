using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

public class AssetCleaner
{
    public static bool useDebugging = false;
    private static List<string> assetPathNames;
    private static List<string> usedAssetPathNames;
    public static List<string> unusedAssetPathNames;
    private static List<string> scenesInProject;
    private static List<string> protectedAssets;
    public static bool hasProtectedAssets;
    

    [MenuItem("Tools/Asset Cleaner/Scan Project For Unused Assets...", false, 1)]
    private static void ScanForScenes()
    {
        RightClickFolder.useFolderFilter = false;
        if (!SettingsWindow.NoPopupOnLaunch) EditorUtility.DisplayDialog("Asset Cleaner", "Backup up your project before deleting any files.", "OK");
        scenesInProject = IndexFilesInProject();
        if (scenesInProject != null)
        {
            if (useDebugging) Debug.Log("Launching Scene Selector.");
            SceneSelectorWindow.ShowWindow(scenesInProject);
        }
        else EditorUtility.DisplayDialog("Asset Cleaner", "ERROR : No Scenes Found In Project", "OK");
    }

    public static void FolderScanForScenes()
    {
        if (!SettingsWindow.NoPopupOnLaunch) EditorUtility.DisplayDialog("Asset Cleaner", "Backup up your project before deleting any files.", "OK");
        scenesInProject = IndexFilesInProject();
        if (scenesInProject != null)
        {
            if (useDebugging) Debug.Log("Launching Scene Selector.");
            SceneSelectorWindow.ShowWindow(scenesInProject);
        }
        else EditorUtility.DisplayDialog("Asset Cleaner", "ERROR : No Scenes Found In Project", "OK");
    }

    private static bool CheckForExcludedPath(string path)
    {
        for (int i = 0; i < SettingsWindow.ExcludedFolders.Count; i++)
        {
            string tmp = SettingsWindow.ExcludedFolders[i];
            tmp = tmp.Substring(7, tmp.Length - 7);
            if (path.Contains(tmp)) return true;
        }
        return false;
    }

    private static bool IsSceneAsset(string path, List<string> scenePaths)
    {
        if (scenePaths.Contains(path)) return true;
        else return false;
    }

    private static List<string> IndexFilesInProject()
    {
        bool debug = SettingsWindow.DebugSceneAndAssetSearch;
        if (useDebugging && debug) Debug.Log("Asset Indexing Started.");
        assetPathNames = new List<string>();
        List<string> scenes = new List<string>();
        List<string> scenePaths = TypeFinder.GetAllScenes<SceneAsset>();
        string[] searchFolders = new string[1];
        searchFolders[0] = "Assets";
        string[] assetGUIDs = AssetDatabase.FindAssets("", searchFolders);
        float progress, a;
        float totalAssets = assetGUIDs.Length;
        float startTime = Time.realtimeSinceStartup;
        float timer, minutes;
        int minutesNoDecimals, minutesToSeconds, secondsLeft;
        string progressTitle;
        string path;
        bool after2019 = Application.unityVersion.StartsWith("202");        
        for (int i = 0; i < assetGUIDs.Length; i++)
        {
            path = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);
            a = i;
            progress = a / totalAssets;
            timer = Time.realtimeSinceStartup - startTime;
            timer = Mathf.RoundToInt(timer);
            progressTitle = "Indexing (busy for " + timer + "s)...";
            if (after2019 && timer >= 10)
            {
                progressTitle = "Indexing ";
                if (EditorUtility.DisplayCancelableProgressBar(progressTitle, path, progress))
                {
                    SceneSelectorWindow.scanCancelled = true;
                    break;
                }
            }
            else if (timer >= 60)
                {
                    minutes = timer / 60;
                    minutesNoDecimals = Mathf.FloorToInt(minutes);
                    minutesToSeconds = minutesNoDecimals * 60;
                    secondsLeft = (int)(timer - minutesToSeconds);
                    if (secondsLeft < 10)
                    {
                        progressTitle = "Indexing (busy for " + minutesNoDecimals + ":0" + secondsLeft + ")...";
                    }
                    else
                    {
                        progressTitle = "Indexing (busy for " + minutesNoDecimals + ":" + secondsLeft + ")...";
                    }
                }
                if (EditorUtility.DisplayCancelableProgressBar(progressTitle, path, progress))
                {
                    SceneSelectorWindow.scanCancelled = true;
                    break;
                }
                       
            if (!CheckForExcludedPath(path)) //Exclude Asset Cleaner Files
            {
                bool processAsset = true;
                //check for excluded documents
                if (SettingsWindow.ExcludeDocuments)
                {
                    for (int j = 0; j < SettingsWindow.DocumentExtensions.Count; j++)
                    {
                        if (path.EndsWith(SettingsWindow.DocumentExtensions[j], true, null))
                        {
                            processAsset = false;
                            break;
                        }
                    }
                }
                //check for excluded file extensions
                if (SettingsWindow.ExcludeCustomFileExtensions)
                {
                    for (int j = 0; j < SettingsWindow.CustomExclusions.Count; j++)
                    {
                        if (path.EndsWith(SettingsWindow.CustomExclusions[j], true, null))
                        {
                            processAsset = false;
                            break;
                        }
                    }
                }
                //process asset if not an excluded file type
                if (processAsset)
                {
                    try
                    {
                        string typeString = AssetDatabase.GetMainAssetTypeAtPath(path).ToString();
                        if (useDebugging && debug) Debug.Log("Asset found at : " + path);
                        if (useDebugging && debug) Debug.Log("Asset type is : " + typeString);
                        if (typeString == "UnityEditor.DefaultAsset")
                        {
                            if (!AssetDatabase.IsValidFolder(path))
                            {
                                if (path.EndsWith(".cs", true, null))
                                {
                                    if (useDebugging && debug) Debug.Log("Script found at : " + path);
                                    if (SettingsWindow.ExcludeScripts)
                                    {
                                        if (useDebugging && debug) Debug.Log(path + " is an excluded file type.");
                                    }
                                    else
                                    {
                                        if (useDebugging && debug) Debug.Log(path + " added to list of found assets.");
                                        assetPathNames.Add(path);
                                    }
                                }
                                else
                                {
                                    if (useDebugging && debug) Debug.Log("Default Asset found at : " + path);
                                    if (useDebugging && debug) Debug.Log(path + " added to list of found assets.");
                                    assetPathNames.Add(path);
                                }
                            }
                            else
                            {
                                assetPathNames.Add(path);
                                scenes.Add(path);
                                if (useDebugging && debug) Debug.Log("Folder found at : " + path);
                                if (useDebugging && debug) Debug.Log(path + " added to list of found assets.");
                            }

                        }
                        else if (typeString == "UnityEditor.SceneAsset")
                        {
                            scenes.Add(path);
                            assetPathNames.Add(path);
                            if (useDebugging && debug) Debug.Log("Scene found at : " + path);
                            if (useDebugging && debug) Debug.Log(path + " added to list of scenes and found assets.");
                        }
                        else if (typeString == "UnityEditor.MonoScript")
                        {
                            if (useDebugging && debug) Debug.Log("Script found at : " + path);
                            if (SettingsWindow.ExcludeScripts)
                            {
                                if (useDebugging && debug) Debug.Log(path + " is an excluded file type.");
                            }
                            else
                            {
                                assetPathNames.Add(path);
                                if (useDebugging && debug) Debug.Log(path + " added to list of found assets.");
                            }
                        }
                        else if (typeString == "UnityEngine.TextAsset")
                        {
                            if (useDebugging && debug) Debug.Log("Text Asset found at : " + path);
                            assetPathNames.Add(path);
                            if (useDebugging && debug) Debug.Log(path + " added to list of found assets.");
                        }
                        else if (typeString == "UnityEngine.Material")
                        {
                            if (useDebugging && debug) Debug.Log("Material found at : " + path);
                            if (SettingsWindow.ExcludeMaterials)
                            {
                                if (useDebugging && debug) Debug.Log(path + " is an excluded file type.");
                            }
                            else
                            {
                                assetPathNames.Add(path);
                                if (useDebugging && debug) Debug.Log(path + " added to list of found assets.");
                            }
                        }
                        else
                        {
                            if (useDebugging && debug) Debug.Log("Asset found with type: " + typeString + " at : " + path);
                            assetPathNames.Add(path);
                            if (useDebugging && debug) Debug.Log(path + " added to list of found assets.");
                        }
                    }
                    catch
                    {
                        if (useDebugging && debug) Debug.Log("Asset found with no type at : " + path);
                        assetPathNames.Add(path);
                        if (useDebugging && debug) Debug.Log(path + " added to list of found assets.");
                    }
                }
            }
            else
            {
                if (IsSceneAsset(path, scenePaths))
                {
                    scenes.Add(path);
                    if (useDebugging && debug)
                    {
                        if (useDebugging && debug) Debug.Log("Scene found in an excluded folder at : " + path);
                        if (useDebugging && debug) Debug.Log(path + " added to list of scenes.");
                    }

                }
                else if (AssetDatabase.IsValidFolder(path))
                {
                    assetPathNames.Add(path);
                    scenes.Add(path);
                    if (useDebugging && debug) Debug.Log("Folder found at : " + path);
                    if (useDebugging && debug) Debug.Log(path + " added to list of found assets.");
                }
                else
                {
                    if (useDebugging && debug)
                    {
                        if (path.Contains("PleebieJeebies/AssetCleaner")) Debug.Log(path + " is part of the Asset Cleaner and will be excluded.");
                        else Debug.Log(path + " is part of an excluded folder structure and will be excluded.");
                    }
                }
            }
        }
        if(SettingsWindow.ExcludeScriptables)
        {
            RemoveScriptableObjects();
        }

        EditorUtility.ClearProgressBar();
        if (useDebugging) Debug.Log("Assets Have Been Indexed.");

        List<string> scenesFound = TypeFinder.GetAllScenes<SceneAsset>();
        

        if (useDebugging && debug)
        {
            Debug.Log("Total Assets Found = " + assetPathNames.Count);
            Debug.Log("Total Scenes Found = " + scenesFound.Count);
        }
        for (int i = 0; i < scenes.Count; i++)
        {
            if (useDebugging && debug) Debug.Log("Scene " + i + " is located at : " + scenes[i]);
        }
        if (scenes.Count > 0) return scenes;
        else return null;
    }
        
    private static void RemoveScriptableObjects()
    {
        List<string> scriptables = TypeFinder.GetAllScriptableObjects<ScriptableObject>();
        foreach (string s in scriptables)
        {
            bool removing = true;
            while (removing)
            {
                removing = assetPathNames.Remove(s);
            }
        }
    }

    public static void ScanSelectedScenes()
    {
        bool debug = SettingsWindow.DebugUsedAssetScan;
        usedAssetPathNames = new List<string>();
        for (int i = 0; i < SceneTreeView.itemSelections.Length; i++)
        {
            if (SceneTreeView.itemSelections[i].Selected)
            {
                string scenePath = SceneSelectorWindow.nameOfScenes[i];
                if (!AssetDatabase.IsValidFolder(scenePath))
                {
                    if (useDebugging && debug) Debug.Log("Scene " + scenePath + " was selected for a scan of used assets.");
                    string[] usedAssets = AssetDatabase.GetDependencies(scenePath, true);
                    if (useDebugging && debug) Debug.Log("Scene " + scenePath + " has " + usedAssets.Length + " Dependencies.");                    
                    for (int j = 0; j < usedAssets.Length; j++)
                    {
                        EditorUtility.DisplayProgressBar("Loading Scene Dependencies...", "", 0f);
                        string assetPath = usedAssets[j];
                        if (!usedAssetPathNames.Contains(assetPath) && !AssetDatabase.IsValidFolder(assetPath)) usedAssetPathNames.Add(assetPath);
                    }
                }
            }
            else
            {
                if (useDebugging && debug) Debug.Log("Scene " + SceneSelectorWindow.nameOfScenes[i] + " was not selected for a scan of used assets.");
            }
        }

        if (useDebugging && debug)
        {
            Debug.Log("There are " + usedAssetPathNames.Count + " used assets in the scenes scanned.");
            Debug.Log("The used assets are :");
            foreach (string s in usedAssetPathNames) Debug.Log(s);
        }
        BuildListOfUnusedAssets();
        BuildListOfProtectedAssets();
        RemoveProtectedFilesFromUnusedAssets();
        if (useDebugging)
        {
            Debug.Log("Index Scan Complete...");
            Debug.Log("Total Assets In Index : " + assetPathNames.Count.ToString());
            Debug.Log("Total Unused Assets : " + unusedAssetPathNames.Count.ToString());
        }
        LaunchCleaningWindow();
    }

    private static void LaunchCleaningWindow()
    {
        if (assetPathNames.Count > usedAssetPathNames.Count)
        {
            List<TreeViewItem> items = BuildTreeViewItemsList();
            if (useDebugging) Debug.Log("Displaying Unused Assets");
            AssetCleanerWindow.ShowWindow(items);
        }
        else EditorUtility.DisplayDialog("Asset Cleaner", "ERROR : No Unused Assets In Project", "OK");
    }

    private static void BuildListOfUnusedAssets()
    {
        bool debug = SettingsWindow.DebugUsedAssetScan;
        unusedAssetPathNames = new List<string>();
        for (int i = 0; i < assetPathNames.Count; i++)
        {
            string s = assetPathNames[i];        
            EditorUtility.DisplayProgressBar("Building List Of Unused Assets...", "", 0.33f);
            if (!usedAssetPathNames.Contains(s))
            {
                if (RightClickFolder.useFolderFilter)
                {
                    if (AssetDatabase.IsValidFolder(s))
                    {
                        unusedAssetPathNames.Add(s);
                    }
                    else
                    {
                        for (int j = 0; j < RightClickFolder.pathNames.Count; j++)
                        {
                            if (s.StartsWith(RightClickFolder.pathNames[j]))
                            {
                                string[] splitS = s.Split('/', '/');
                                string[] splitJ = RightClickFolder.pathNames[j].Split('/', '/');
                                int targetLength = splitJ.Length + 1;
                                if (splitS.Length == targetLength) unusedAssetPathNames.Add(s);
                            }
                        }
                    }
                }
                else unusedAssetPathNames.Add(s);
            }
        }

        if (useDebugging && debug)
        {
            Debug.Log("There are " + unusedAssetPathNames.Count + " unused assets by the scenes scanned.");
            Debug.Log("The unused assets are :");
            foreach (string s in unusedAssetPathNames) Debug.Log(s);
        }
        unusedAssetPathNames = RemoveDuplicatesFromList(unusedAssetPathNames);

    }

    private static List<string> RemoveDuplicatesFromList(List<string> dupeList)
    {
        List<string> returnList = new List<string>();
        while (dupeList.Count > 0)
        {
            string tmp = dupeList[0];
            returnList.Add(tmp);
            bool removing = true;
            while (removing)
            {
                removing = dupeList.Remove(tmp);
            }
        }
        return returnList;
    }

    public static List<TreeViewItem> BuildTreeViewItemsList()
    {
        bool debug = SettingsWindow.DebugTreeViewConstruction;
        List<TreeViewItem> allItems = new List<TreeViewItem>();
        string assets = "Assets/";
        char[] assetsPathArray = assets.ToCharArray();
        unusedAssetPathNames = PruneEmptyFoldersFromTree(unusedAssetPathNames);
        for (int i = 0; i < unusedAssetPathNames.Count; i++)
        {
            EditorUtility.DisplayProgressBar("Building TreeView...", "", 0.66f);
            //get itemPath i
            string itemPath = unusedAssetPathNames[i];
            if (useDebugging && debug) Debug.Log("Item Path = " + itemPath);
            //trim "Assets/" from itemPath
            itemPath = itemPath.TrimStart(assetsPathArray);
            //split string to calculate path Depth and trim all / from the strings in the array
            string[] splitString = itemPath.Split('/', '/');
            //get Depth
            int indexDepth = splitString.Length - 1;
            //get displayName
            string displayName = splitString[indexDepth];
            //create new TreeViewItem using i for ID, currentDepeth for depth and name string for displayName
            TreeViewItem newItem = new TreeViewItem(i, indexDepth, displayName); 
            //add new TreeViewItem to allItems List
            allItems.Add(newItem);
            if (useDebugging && debug) Debug.Log("Item " + displayName + " was added to the tree with a depth of " + indexDepth);
        }
        EditorUtility.ClearProgressBar();
        // Return List<TreeViewItem>
        return allItems;
    }

    public static List<TreeViewItem> BuildTreeViewItemsList(List<string> scenesFilter)
    {
        bool debug = SettingsWindow.DebugTreeViewConstruction;
        List<TreeViewItem> allItems = new List<TreeViewItem>();
        List<string> treeViewItems =  scenesFilter;
        string assets = "Assets/";
        char[] assetsPathArray = assets.ToCharArray();
        treeViewItems = PruneEmptyFoldersFromTree(treeViewItems);     
        string[] lastPath = new string[0];
        string lastPathName = "";
        for (int i = 0; i < treeViewItems.Count; i++)
        {
            EditorUtility.DisplayProgressBar("Building TreeView...", "", 0.66f);
            //get itemPath i
            string itemPath = scenesFilter[i];
            if (useDebugging && debug) Debug.Log("Item Path = " + itemPath);
            //trim "Assets/" from itemPath
            itemPath = itemPath.TrimStart(assetsPathArray);
            //split string to calculate path Depth and trim all / from the strings in the array
            string[] splitString = itemPath.Split('/', '/');
            //get Depth
            int indexDepth = splitString.Length - 1;
            //get displayName
            string displayName = splitString[indexDepth];
            //get pathName
            string pathName = "";
            for (int j = 0; j < splitString.Length;j++)
            {
                pathName += splitString[j];
            }
            if (lastPathName == "")
            {
                lastPathName = pathName;
            }
            if (splitString.Length != lastPath.Length && pathName.Contains(lastPathName))
            {

            }
            //create new TreeViewItem using i for ID, currentDepth for depth and name string for displayName
            TreeViewItem newItem = new TreeViewItem(i, indexDepth, displayName);
            //add new TreeViewItem to allItems List
            allItems.Add(newItem);
            if (useDebugging && debug) Debug.Log("Item " + displayName + " was added to the tree with a depth of " + indexDepth);
        }
        EditorUtility.ClearProgressBar();
        // Return List<TreeViewItem>
        return allItems;
    }

    private static List<string> PruneEmptyFoldersFromTree(List<string> tree)
    {
        bool debug = SettingsWindow.DebugTreeViewConstruction;
        //build list of folders
        List<string> foldersToCheck = new List<string>();
        foreach (string s in tree)
        {
            if (AssetDatabase.IsValidFolder(s)) foldersToCheck.Add(s);
        }
        if(useDebugging && debug) Debug.Log("Total Folders in Unused Assets = " + foldersToCheck.Count + ". The List of Folders are :");
        if(useDebugging && debug) foreach (string s in foldersToCheck) Debug.Log(s);


        //iterate backwards thru folders to check and remove ones that contain no files in unusedAssets
        for (int i = foldersToCheck.Count - 1; i >= 0; i--)
        {
            bool foundItem = false;
            //iterate thru unused assets and check folder for files contained in folder
            for (int j = 0; j < tree.Count ;j++)
            {
                string s = tree[j];
                if (s.StartsWith(foldersToCheck[i]) && s != foldersToCheck[i])
                {
                    foundItem = true;
                    break;
                }
            }
            if (!foundItem)
            {
                tree.Remove(foldersToCheck[i]);
                if (useDebugging && debug) Debug.Log(foldersToCheck[i] + " is folder with no unused assets and has been pruned from the TreeViewItems.");
            }
        }
        return tree;
    }


    private static void BuildListOfProtectedAssets()
    {
        protectedAssets = new List<string>();
        //Get Excluded folders
        int folderCount = SettingsWindow.ExcludedFolders.Count;        
        string[] searchFolders = new string[folderCount];
        //process excluded folders to find all the excluded files
        for (int i = 0; i < folderCount; i++)
        {
            searchFolders[i] = SettingsWindow.ExcludedFolders[i];
        }
        string[] assetGUIDs = AssetDatabase.FindAssets("", searchFolders);
        //iterate thru excluded files and find all the dependencies
        string path;
        //iterate thru each file in an excluded folder
        for (int i = 0; i < assetGUIDs.Length; i++)
        {
            path = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);
            //get array of dependancies
            string[] tmpDependancies = AssetDatabase.GetDependencies(path, true);
            //add dependancies to protected files list
            foreach (string s in tmpDependancies) AddNewProtectedFile(s);
        }
    }

    private static void AddNewProtectedFile(string newFile)
    {
        if (!protectedAssets.Contains(newFile)) protectedAssets.Add(newFile);
    }

    private static void RemoveProtectedFilesFromUnusedAssets() 
    {
        hasProtectedAssets = false;
        for (int j = 0; j < protectedAssets.Count; j++)
        {
            string s = protectedAssets[j];
            for (int i = unusedAssetPathNames.Count - 1; i >= 0; i--)
            {
                if (unusedAssetPathNames[i].Equals(s))
                {
                    unusedAssetPathNames.RemoveAt(i);
                    if (!s.Contains("Assets/PleebieJeebies/AssetCleaner")) hasProtectedAssets = true;
                }
            }
        }
    }


}
