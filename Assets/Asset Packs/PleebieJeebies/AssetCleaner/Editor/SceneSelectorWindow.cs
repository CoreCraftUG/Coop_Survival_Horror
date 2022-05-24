using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

public class SceneSelectorWindow : EditorWindow
{
    private static List<TreeViewItem> treeViewItems;
    [SerializeField] TreeViewState m_TreeViewState;
    private static SceneTreeView m_SceneTreeView;
    private static GUIStyle buttonStyle;
    public static List<string> nameOfScenes = new List<string>();
    public static bool scanCancelled = false;

    private Rect TopButtonbarRect
    {
        get { return new Rect(10f, 5f, position.width - 20f, 20f); }
    }

    private Rect SceneSelectRect
    {
        get { return new Rect(10f, 25f, position.width - 20, position.height - 60); }
    }

    private Rect BottomButtonbarRect
    {
        get { return new Rect(10f, position.height - 25f, position.width - 20f, 16f); }
    }

    private void TopButtonBar(Rect rect)
    {
        GUILayout.BeginArea(rect);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Expand All", buttonStyle))
            {
                m_SceneTreeView.ExpandAll();
            }

            if (GUILayout.Button("Collapse All", buttonStyle))
            {
                m_SceneTreeView.CollapseAll();
            }
        }
        GUILayout.EndArea();
    }

    private void BottomButtonBar(Rect rect)
    {
        GUILayout.BeginArea(rect);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Cancel", buttonStyle))
            {
                if (AssetCleaner.useDebugging) Debug.Log("Scan Cancelled");
                this.Close();
            }
            if (GUILayout.Button("View Unused Assets", buttonStyle))
            {
                AssetCleaner.ScanSelectedScenes();
                this.Close();
            }
        }
        GUILayout.EndArea();
    }

    private void DoTreeView(Rect rect)
    {
        if (treeViewItems.Count > 0)
        {
            m_SceneTreeView.OnGUI(rect);
        }
        else
        {
            Rect noAssets = new Rect(rect.x, rect.y, rect.width, 20);
            EditorGUI.LabelField(noAssets, "0 unused assets.");
        }
    }

    public static void ShowWindow(List<string> sceneNames)
    {
        if (!scanCancelled)
        {
            treeViewItems = AssetCleaner.BuildTreeViewItemsList(sceneNames);
            var window = GetWindow<SceneSelectorWindow>();
            window.titleContent = new GUIContent("Select Scenes In Project");
            window.minSize = new Vector2(350, 250);
            window.Show();
            nameOfScenes = sceneNames;   
            for (int i = 0; i < SceneTreeView.itemSelections.Length; i++)
            {
                SceneTreeView.itemSelections[i].Selected = true;
            }
        }
        else
        {
            scanCancelled = false;
        }
    }

    private void OnEnable()
    {
        buttonStyle = new GUIStyle(EditorStyles.miniButton);
        // Check whether there is already a serialized view state (state 
        // that survived assembly reloading)
        if (m_TreeViewState == null) m_TreeViewState = new TreeViewState();
        m_SceneTreeView = new SceneTreeView(m_TreeViewState, treeViewItems);
        m_SceneTreeView.ExpandAll();
    }

    void OnGUI()
    {
        TopButtonBar(TopButtonbarRect);
        DoTreeView(SceneSelectRect);
        BottomButtonBar(BottomButtonbarRect);
    }
}
