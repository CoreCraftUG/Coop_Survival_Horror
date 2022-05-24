using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

public class AssetCleanerWindow : EditorWindow
{
    private static List<TreeViewItem> treeViewItems;
    [SerializeField] TreeViewState m_TreeViewState;
    CleaningTreeView m_CleaningTreeView;

    private static GUIStyle buttonStyle, deleteStyle;
    
    private Rect TopButtonbarRect
    {
        get { return new Rect(10f, 5f, position.width - 20f, 20f); }
    }

    private Rect TreeViewRect
    {
        get { return new Rect(10f, 25f, position.width - 20, position.height - 60); }
    }

    private Rect BottomButtonbarRect
    {
        get { return new Rect(10f, position.height - 25f, position.width - 20f, 16f); }
    }

    private void DoTreeView(Rect rect)
    {
        if (treeViewItems.Count > 0)
        {
            m_CleaningTreeView.OnGUI(rect);
        }
        else
        {
            Rect noAssets = new Rect(rect.x, rect.y, rect.width, 20);
            EditorGUI.LabelField(noAssets, "0 unused assets.");
        }
    }

    private void TopButtonBar(Rect rect)
    {
        GUILayout.BeginArea(rect);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Expand All", buttonStyle))
            {
                m_CleaningTreeView.ExpandAll();
            }

            if (GUILayout.Button("Collapse All", buttonStyle))
            {
                m_CleaningTreeView.CollapseAll();
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
                if (AssetCleaner.useDebugging) Debug.Log("Asset Clean Cancelled.");
                this.Close();
            }
            if (GUILayout.Button("DELETE SELECTED", deleteStyle))
            {
                if (AssetCleaner.useDebugging) Debug.Log("Delete Confirmation Requested.");
                //Show Popup window to confirm delete
                Rect confirmPosition = new Rect(position);
                confirmPosition.y += confirmPosition.height / 2;
                ConfirmDeleteWindow.ShowWindow(confirmPosition);
                //close this window
                this.Close();
            }
        }
        GUILayout.EndArea();
    }

    public static void ShowWindow(List<TreeViewItem> tree)
    {
        treeViewItems = tree;
        // Get existing open window or if none, make a new one:
        var window = GetWindow<AssetCleanerWindow>();
        window.titleContent = new GUIContent("Unused Assets");
        window.minSize = new Vector2(350, 500);
        window.Show();
    }

    private void OnEnable()
    {
        buttonStyle = new GUIStyle(EditorStyles.miniButton);
        deleteStyle = new GUIStyle(EditorStyles.miniButton);
        deleteStyle.hover.textColor = Color.red;
        // Check whether there is already a serialized view state (state 
        // that survived assembly reloading)
        if (m_TreeViewState == null) m_TreeViewState = new TreeViewState();
        m_CleaningTreeView = new CleaningTreeView(m_TreeViewState, treeViewItems);
        m_CleaningTreeView.ExpandAll();
}

    private void OnGUI()
    {
        TopButtonBar(TopButtonbarRect);
        DoTreeView(TreeViewRect);
        BottomButtonBar(BottomButtonbarRect);
    }
}
