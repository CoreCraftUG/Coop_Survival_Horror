using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

[InitializeOnLoad]
public class SettingsWindow : EditorWindow
{
    //settings loaded
    private static bool settingsLoaded = false;
    private static List<TreeViewItem> documentsTreeViewItems;
    private static List<TreeViewItem> customTreeViewItems;
    private static List<TreeViewItem> excludedFoldersTreeViewItems;
    private Vector2 scrollBarPosition1;
    private Vector2 scrollBarPosition2;
    private Vector3 scrollBarPosition3;
    private string newCustomExtension = "";
    private string newDocumentExtension = "";
    [SerializeField] TreeViewState m_DocumentsTreeViewState;
    [SerializeField] TreeViewState m_CustomTreeViewState;
    [SerializeField] TreeViewState m_ExcludedFoldersTreeViewState;
    private ExtensionsTreeView m_DocumentsTreeView;
    private ExtensionsTreeView m_CustomTreeView;
    private ExtensionsTreeView m_ExcludedFoldersTreeView;
    private GUIStyle buttonStyle;

    private void DoDocumentsTreeView(Rect rect)
    {
        m_DocumentsTreeView.OnGUI(rect);
    }

    private void DoCustomTreeView(Rect rect)
    {
        m_CustomTreeView.OnGUI(rect);
    }

    private void DoExcludedFoldersTreeView(Rect rect)
    {
        m_ExcludedFoldersTreeView.OnGUI(rect);
    }
 
    private static bool _debugSceneAndAssetSearch;
    public static bool DebugSceneAndAssetSearch
    {
        get { return _debugSceneAndAssetSearch; }
        set
        {
            if (_debugSceneAndAssetSearch != value)
            {
                _debugSceneAndAssetSearch = value;
            }

        }
    }

    private static bool _debugUsedAssetScan;
    public static bool DebugUsedAssetScan
    {
        get { return _debugUsedAssetScan; }
        set
        {
            if (_debugUsedAssetScan != value)
            {
                _debugUsedAssetScan = value;
            }
        }
    }

    private static bool _debugTreeViewConstruction;
    public static bool DebugTreeViewConstruction
    {
        get { return _debugTreeViewConstruction; }
        set
        {
            if (_debugTreeViewConstruction != value)
            {
                _debugTreeViewConstruction = value;
            }
        }
    }

    private static bool _debugAssetFilesDelete;
    public static bool DebugAssetFilesDelete
    {
        get { return _debugAssetFilesDelete; }
        set
        {
            if (_debugAssetFilesDelete != value)
            {
                _debugAssetFilesDelete = value;
            }
        }
    }

    private static bool _debugEmptyFoldersDelete;
    public static bool DebugEmptyFoldersDelete
    {
        get { return _debugEmptyFoldersDelete; }
        set
        {
            if (_debugEmptyFoldersDelete != value)
            {
                _debugEmptyFoldersDelete = value;
            }
        }
    }

    //exclusion options
    private static bool _excludeScripts;
    public static bool ExcludeScripts //exclude or include unused c# scripts
    {
        get { return _excludeScripts; }
        set
        {
            if (_excludeScripts != value)
            {
                _excludeScripts = value;
            }
        }
    }

    private static bool _excludeDocuments;
    public static bool ExcludeDocuments //include or exclude documents
    {
        get { return _excludeDocuments; }
        set
        {
            if (_excludeDocuments != value)
            {
                _excludeDocuments = value;
            }
        }
    }

    private static List<string> _documentExtensions = new List<string>();
    public static List<string> DocumentExtensions //list of document extensions to exclude
    {
        get { return _documentExtensions; }
        set
        {
            _documentExtensions = value;
        }
    }

    private static bool _excludeCustomFileExtensions;
    public static bool ExcludeCustomFileExtensions //include or exclude custom list of file extensions
    {
        get { return _excludeCustomFileExtensions; }
        set
        {
            if (_excludeCustomFileExtensions != value)
            {
                _excludeCustomFileExtensions = value;
            }
        }
    }

    private static List<string> _customExclusions = new List<string>();
    public static List<string> CustomExclusions //custom list of file extensions to exclude
    {
        get { return _customExclusions; }
        set
        {
            _customExclusions = value;
        }
    }

    private static List<string> _excludedFolders = new List<string>();
    public static List<string> ExcludedFolders
    {
        get { return _excludedFolders; }
        set
        {
            _excludedFolders = value;
        }
    }

    private static bool _deleteEmptyFolders;
    public static bool DeleteEmptyFolders
    {
        get { return _deleteEmptyFolders; }
        set
        {
            if (_deleteEmptyFolders != value)
            {
                _deleteEmptyFolders = value;
            }
        }
    }

    private static bool _noPopupOnLaunch;
    public static bool NoPopupOnLaunch
    {
        get { return _noPopupOnLaunch; }
        set
        {
            if (_noPopupOnLaunch != value)
            {
                _noPopupOnLaunch = value;
            }
        }
    }

    private static bool _excludeMaterials;
    public static bool ExcludeMaterials
    {
        get { return _excludeMaterials; }
        set
        {
            if (_excludeMaterials != value)
            {
                _excludeMaterials = value;
            }
        }
    }

    private static bool _excludeScriptables;
    public static bool ExcludeScriptables
    {
        get { return _excludeScriptables; }
        set
        {
            if (_excludeScriptables != value)
            {
                _excludeScriptables = value;
            }
        }
    }

    private static DefaultAsset _excludedFolder = null;
    public static DefaultAsset ExcludedFolder
    {
        get { return _excludedFolder; }
        set
        {
            string path = AssetDatabase.GetAssetPath(value);
            bool isFolder = AssetDatabase.IsValidFolder(path);
            if (isFolder)
            {
                _excludedFolder = value;
            }
            else
            {
                _excludedFolder = null;
            }
        }
    }

    [MenuItem("Tools/Asset Cleaner/Edit Settings...", false, 200)]
    private static void Settings()
    {
        List<TreeViewItem> documents = SettingsHelper.BuildTreeViewItemsFromList(DocumentExtensions);
        List<TreeViewItem> custom = SettingsHelper.BuildTreeViewItemsFromList(CustomExclusions);
        List<TreeViewItem> folders = SettingsHelper.BuildTreeViewItemsFromList(ExcludedFolders);
        ShowWindow(documents, custom, folders);
    }

    static SettingsWindow()
    {
        if (!settingsLoaded) LoadSettings();
    }

    /// <summary>
    /// RECTANGLES FOR LAYOUT
    /// </summary>
    private Rect MainOptionsRect
    {
        get { return new Rect(20f, 10f, 195f, 160f); }
    }

    private Rect DebugOptionsRect
    {
        get { return new Rect(225f, 10f, 195f, 160f); }
    }

    private Rect ExtensionsRect
    {
        get { return new Rect(20f, 180f, 400f, 600f); }
    }

    private Rect CustomBoxRect
    {
        get { return new Rect(5f, 2f, 175f, 90f); }
    }

    private Rect DocumentBoxRect
    {
        get { return new Rect(5f, 2f, 175f, 90f); }
    }

    private Rect FolderBoxRect
    {
        get { return new Rect(5f, 2f, 380f, 90f); }
    }

    private Rect BottomButtonbarRect
    {
        get { return new Rect(20f, position.height - 30f, position.width - 40f, 36f); }
    }

    private void DoMainOptionsView(Rect rect)
    {
        GUILayout.BeginArea(rect, EditorStyles.helpBox);
        GUILayout.Label("Options", "boldLabel");
        NoPopupOnLaunch = EditorGUILayout.ToggleLeft("No Warning On Launch", NoPopupOnLaunch);
        DeleteEmptyFolders = EditorGUILayout.ToggleLeft("Delete Empty Folders", DeleteEmptyFolders);
        ExcludeScripts = EditorGUILayout.ToggleLeft("Exclude Scripts", ExcludeScripts);
        ExcludeMaterials = EditorGUILayout.ToggleLeft("Exclude Materials", ExcludeMaterials);
        ExcludeScriptables = EditorGUILayout.ToggleLeft("Exclude ScriptableObjects", ExcludeScriptables);
        ExcludeDocuments = EditorGUILayout.ToggleLeft("Exclude Documents", ExcludeDocuments);
        ExcludeCustomFileExtensions = EditorGUILayout.ToggleLeft("Exclude Other Files", ExcludeCustomFileExtensions);
        GUILayout.EndArea();
    }

    private void DoDebugOptionsView(Rect rect)
    {
        GUILayout.BeginArea(rect, EditorStyles.helpBox);
        GUILayout.Label("Advanced Console Output", "boldLabel");
        DebugSceneAndAssetSearch = EditorGUILayout.ToggleLeft("Asset Indexing", DebugSceneAndAssetSearch);
        DebugUsedAssetScan = EditorGUILayout.ToggleLeft("Scan For Unused Assets", DebugUsedAssetScan);
        DebugTreeViewConstruction = EditorGUILayout.ToggleLeft("Unused Assets Hierarchy", DebugTreeViewConstruction);
        DebugAssetFilesDelete = EditorGUILayout.ToggleLeft("Unused Assets Clean", DebugAssetFilesDelete);
        DebugEmptyFoldersDelete = EditorGUILayout.ToggleLeft("Empty Folders Clean", DebugEmptyFoldersDelete);
        GUILayout.EndArea();
    }

    private void DoScrollWindowsView(Rect rectArea)
    {
        GUILayout.BeginArea(rectArea);
        GUILayout.Label("File Type Exclusions", "boldLabel");
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Document File Extensions", "boldLabel");
        using (new EditorGUILayout.HorizontalScope())
        {
            newDocumentExtension = GUILayout.TextField(newDocumentExtension, GUILayout.Width(60));
            EditorGUI.BeginDisabledGroup(newDocumentExtension.Length < 1);
            if (GUILayout.Button("Add New", buttonStyle))
            {
                string trimmed = newDocumentExtension.Trim(' ');
                if (trimmed != null && trimmed != "")
                {
                    int listSize = _documentExtensions.Count;
                    AddNewExtension(ref _documentExtensions, newDocumentExtension);
                    if (listSize < _documentExtensions.Count)
                    {
                        SettingsHelper.SaveSettings();
                        newDocumentExtension = "";
                        ResetTreeViews();
                        ResetExtensionsSelections();
                    }
                }
                else //show error
                {
                    EditorUtility.DisplayDialog("Asset Cleaner", "ERROR : Invalid format for file extensions", "OK");
                }
            }
            EditorGUI.EndDisabledGroup();
        }
        scrollBarPosition1 = GUILayout.BeginScrollView(scrollBarPosition1, false, false, GUILayout.Width(DocumentBoxRect.width + 10), GUILayout.Height(100));
        DoDocumentsTreeView(DocumentBoxRect);
        EditorGUILayout.EndScrollView();
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Load Defaults", buttonStyle))
            {
                DocumentExtensions = SettingsHelper.DecodeString(SettingsHelper.docExtensions);
                SettingsHelper.SaveSettings();
                ResetTreeViews();
                ResetExtensionsSelections();
            }
            EditorGUI.BeginDisabledGroup(DocumentExtensions.Count == 0);
            if (GUILayout.Button("Clear", buttonStyle))
            {
                DocumentExtensions.Clear();
                SettingsHelper.SaveSettings();
                ResetTreeViews();
                ResetExtensionsSelections();
            }
            EditorGUI.EndDisabledGroup();
        }
        EditorGUI.BeginDisabledGroup(!m_DocumentsTreeView.HasSelection());
        if (GUILayout.Button("Delete Selection", buttonStyle))
        {
            IList<int> selections = m_DocumentsTreeView.GetSelection();
            for (int i = m_DocumentsTreeView.treeViewItems.Count - 1; i >= 0; i--)
            {
                bool selected = false;
                for (int j = 0; j < selections.Count; j++)
                {
                    if (selections[j] == i)
                    {
                        selected = true;
                        break;
                    }
                }
                if (selected) DocumentExtensions.RemoveAt(i);
            }
            SettingsHelper.SaveSettings();
            ResetTreeViews();
            ResetExtensionsSelections();
        }
        EditorGUI.EndDisabledGroup();
        GUILayout.EndVertical();
        GUILayout.Space(10);
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Other File Extensions", "boldLabel");
        using (new EditorGUILayout.HorizontalScope())
        {
            newCustomExtension = GUILayout.TextField(newCustomExtension, GUILayout.Width(60));
            EditorGUI.BeginDisabledGroup(newCustomExtension.Length < 1);
            if (GUILayout.Button("Add New", buttonStyle))
            {
                string trimmed = newCustomExtension.Trim(' ');
                if (trimmed != null && trimmed != "")
                {
                    int listSize = _customExclusions.Count;
                    AddNewExtension(ref _customExclusions, newCustomExtension);
                    if (listSize < _customExclusions.Count)
                    {
                        SettingsHelper.SaveSettings();
                        newCustomExtension = "";
                        ResetTreeViews();
                        ResetExtensionsSelections();
                    }
                }
                else //show error
                {
                    EditorUtility.DisplayDialog("Asset Cleaner", "ERROR : Invalid format for file extensions", "OK");
                }
            }
            EditorGUI.EndDisabledGroup();
        }
        scrollBarPosition2 = GUILayout.BeginScrollView(scrollBarPosition2, false, false, GUILayout.Width(CustomBoxRect.width + 10), GUILayout.Height(100));
        DoCustomTreeView(CustomBoxRect);
        EditorGUILayout.EndScrollView();
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Load Defaults", buttonStyle))
            {
                CustomExclusions = SettingsHelper.DecodeString(SettingsHelper.pluginExtensions);
                SettingsHelper.SaveSettings();
                ResetTreeViews();
                ResetExtensionsSelections();
            }
            EditorGUI.BeginDisabledGroup(CustomExclusions.Count == 0);
            if (GUILayout.Button("Clear", buttonStyle))
            {
                CustomExclusions.Clear();
                SettingsHelper.SaveSettings();
                ResetTreeViews();
                ResetExtensionsSelections();
            }
            EditorGUI.EndDisabledGroup();
        }
        EditorGUI.BeginDisabledGroup(!m_CustomTreeView.HasSelection());
        if (GUILayout.Button("Delete Selection", buttonStyle))
        {
            IList<int> selections = m_CustomTreeView.GetSelection();
            for (int i = m_CustomTreeView.treeViewItems.Count - 1; i >= 0; i--)
            {
                bool selected = false;
                for (int j = 0; j < selections.Count; j++)
                {
                    if (selections[j] == i)
                    {
                        selected = true;
                        break;
                    }
                }
                if (selected) CustomExclusions.RemoveAt(i);
            }
            SettingsHelper.SaveSettings();
            ResetTreeViews();
            ResetExtensionsSelections();
        }
        EditorGUI.EndDisabledGroup();
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUILayout.Label("Excluded Folders", "boldLabel");
        GUILayout.BeginVertical(EditorStyles.helpBox);
        using (new EditorGUILayout.HorizontalScope())
        {
            ExcludedFolder = (DefaultAsset)EditorGUILayout.ObjectField(
           "",
           ExcludedFolder,
           typeof(DefaultAsset),
           false);
            EditorGUI.BeginDisabledGroup(ExcludedFolder == null);
            if (GUILayout.Button("Exclude Selected Folder", buttonStyle))
            {
                AddFolderExclusion();
                SettingsHelper.SaveSettings();
                ResetTreeViews();
                ResetExtensionsSelections();
            }
            EditorGUI.EndDisabledGroup();
        }
        scrollBarPosition3 = GUILayout.BeginScrollView(scrollBarPosition3, false, false, GUILayout.Width(FolderBoxRect.width + 10), GUILayout.Height(100));
        DoExcludedFoldersTreeView(FolderBoxRect);
        EditorGUILayout.EndScrollView();
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUI.BeginDisabledGroup(!m_ExcludedFoldersTreeView.HasSelection());
            if (GUILayout.Button("Remove Selected Exclusions", buttonStyle))
            {
                IList<int> selections = m_ExcludedFoldersTreeView.GetSelection();
                for (int i = m_ExcludedFoldersTreeView.treeViewItems.Count - 1; i > 0; i--)
                {
                    bool selected = false;
                    for (int j = 0; j < selections.Count; j++)
                    {
                        if (selections[j] == i)
                        {
                            selected = true;
                            break;
                        }
                    }
                    if (selected) ExcludedFolders.RemoveAt(i);
                }
                SettingsHelper.SaveSettings();
                ResetTreeViews();
                ResetExtensionsSelections();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(ExcludedFolders.Count <= 1);
            if (GUILayout.Button("Clear Exclusions", buttonStyle))
            {
                ExcludedFolders.Clear();
                ExcludedFolders.Add("Assets/PleebieJeebies/AssetCleaner");
                SettingsHelper.SaveSettings();
                ResetTreeViews();
                ResetExtensionsSelections();
            }
            EditorGUI.EndDisabledGroup();
        }


        GUILayout.EndVertical();


        GUILayout.EndArea();
    }

    static void AddFolderExclusion()
    {
        if (ExcludedFolder != null)
        {
            string path = AssetDatabase.GetAssetPath(ExcludedFolder);
            ExcludedFolders.Add(path);
            ExcludedFolder = null;
        }
    }

    private void ResetExtensionsSelections()
    {
        for (int i = 0; i < m_DocumentsTreeView.itemSelections.Length; i++)
        {
            m_DocumentsTreeView.itemSelections[i].Selected = false;
        }
        for (int i = 0; i < m_CustomTreeView.itemSelections.Length; i++)
        {
            m_CustomTreeView.itemSelections[i].Selected = false;
        }
        for (int i = 0; i < m_ExcludedFoldersTreeView.itemSelections.Length; i++)
        {
            m_ExcludedFoldersTreeView.itemSelections[i].Selected = false;
        }
        List<int> selections = new List<int>();
        m_CustomTreeView.SetSelection(selections);
        m_DocumentsTreeView.SetSelection(selections);
        m_ExcludedFoldersTreeView.SetSelection(selections);
    }

    public void ResetTreeViews()
    {
        documentsTreeViewItems = SettingsHelper.BuildTreeViewItemsFromList(DocumentExtensions);
        customTreeViewItems = SettingsHelper.BuildTreeViewItemsFromList(CustomExclusions);
        excludedFoldersTreeViewItems = SettingsHelper.BuildTreeViewItemsFromList(ExcludedFolders);
        OnEnable();
    }

    private void AddNewExtension(ref List<string> data, string newExtn)
    {
        if (newExtn != null && newExtn != "")
        {
            string extn = newExtn.Trim('.');
            if (SettingsHelper.CheckForValidExtension(extn))
            {
                extn = "." + extn;
                data.Add(extn);
            }
            else
            {
                //show error popup
                EditorUtility.DisplayDialog("Asset Cleaner", "ERROR : Invalid format for file extensions", "OK");
            }
        }
    }

    private void DoBottomButtonBar(Rect rect)
    {
        GUILayout.BeginArea(rect);
        using (new EditorGUILayout.HorizontalScope())
        {
            GUIStyle saveButton = new GUIStyle("button")
            {
                fontStyle = FontStyle.Bold
            };
            if (GUILayout.Button("Save and Exit", saveButton))
            {
                SettingsHelper.SaveSettings();
                this.Close();
            }
            if (GUILayout.Button("Exit", saveButton))
            {
                if (AssetCleaner.useDebugging) Debug.Log("Settings Not Saved.");
                this.Close();
            }
        }
        GUILayout.EndArea();
    }

    private void OnEnable()
    {
        buttonStyle = new GUIStyle(EditorStyles.miniButton);
        if (!settingsLoaded) LoadSettings();
        if (m_DocumentsTreeViewState == null) m_DocumentsTreeViewState = new TreeViewState();
        m_DocumentsTreeView = new ExtensionsTreeView(m_DocumentsTreeViewState, documentsTreeViewItems);
        m_DocumentsTreeView.ExpandAll();
        if (m_CustomTreeViewState == null) m_CustomTreeViewState = new TreeViewState();
        m_CustomTreeView = new ExtensionsTreeView(m_CustomTreeViewState, customTreeViewItems);
        m_CustomTreeView.ExpandAll();
        if (m_ExcludedFoldersTreeViewState == null) m_ExcludedFoldersTreeViewState = new TreeViewState();
        m_ExcludedFoldersTreeView = new ExtensionsTreeView(m_ExcludedFoldersTreeViewState, excludedFoldersTreeViewItems);
        m_ExcludedFoldersTreeView.ExpandAll();
    }

    public static void LoadSettings()
    {
        //EditorPrefs.DeleteKey("assetCleanerPrefsInitialized");
        //get debug setting
        bool debug = EditorPrefs.GetBool("Asset Cleaner/Console Logging", false);
        //do console message
        if (debug) Debug.Log("Loading Asset Cleaner....");
        //Check If Asset Cleaner Defaults Need Setting
        bool initialized = EditorPrefs.HasKey("assetCleanerPrefsInitialized");
        if (!initialized) SettingsHelper.InitPrefs();
        //Get Debug Options From EditorPrefs
        DebugSceneAndAssetSearch = EditorPrefs.GetBool("debugSceneAndAssetSearch");
        DebugUsedAssetScan = EditorPrefs.GetBool("debugUsedAssetScan");
        DebugTreeViewConstruction = EditorPrefs.GetBool("debugTreeViewConstruction");
        DebugAssetFilesDelete = EditorPrefs.GetBool("debugAssetFilesDelete");
        DebugEmptyFoldersDelete = EditorPrefs.GetBool("debugEmptyFoldersDelete");
        //Get Exclusion Options From EditorPrefs
        ExcludeScripts = EditorPrefs.GetBool("excludeScripts");
        ExcludeDocuments = EditorPrefs.GetBool("excludeDocuments");
        ExcludeMaterials = EditorPrefs.GetBool("excludeMaterials");
        ExcludeScriptables = EditorPrefs.GetBool("excludeScriptables");
        string data = EditorPrefs.GetString("documentExtensions");
        DocumentExtensions = SettingsHelper.DecodeString(data);
        ExcludeCustomFileExtensions = EditorPrefs.GetBool("excludeCustomFileExtensions");
        data = EditorPrefs.GetString("customExclusions");
        CustomExclusions = SettingsHelper.DecodeString(data);
        string key = SettingsHelper.GetProjectKeyName("excludedFolders");
        if (EditorPrefs.HasKey(key))
        {
            data = EditorPrefs.GetString(key);
            ExcludedFolders = SettingsHelper.DecodeString(data);
        }
        else
        {
            string projectPath = "Assets/PleebieJeebies/AssetCleaner";
            EditorPrefs.SetString(key, projectPath);
            ExcludedFolders = new List<string>
            {
                projectPath
            };
        }
        //clear folders option
        DeleteEmptyFolders = EditorPrefs.GetBool("deleteEmptyFolders");
        NoPopupOnLaunch = EditorPrefs.GetBool("noPopupOnLaunch");
        settingsLoaded = true;
    }

    public static void ShowWindow(List<TreeViewItem> documentExtensions, List<TreeViewItem> customExtensions, List<TreeViewItem> excludedFolders)
    {
        documentsTreeViewItems = documentExtensions;
        customTreeViewItems = customExtensions;
        excludedFoldersTreeViewItems = excludedFolders;
        var window = GetWindow<SettingsWindow>();
        window.titleContent = new GUIContent("Asset Cleaner Settings");
        window.minSize = new Vector2(440, 590);
        window.maxSize = new Vector2(440, 590);
        if (AssetCleaner.useDebugging) Debug.Log("Launching Settings Window.");
        window.Show();
    }

    private void OnGUI()
    {
        DoMainOptionsView(MainOptionsRect);
        DoDebugOptionsView(DebugOptionsRect);
        DoScrollWindowsView(ExtensionsRect);
        DoBottomButtonBar(BottomButtonbarRect);
    }
}
