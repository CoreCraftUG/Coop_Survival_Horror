using System.Collections.Generic;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using UnityEditor;

public class SceneTreeView : TreeView
{
    public static List<TreeViewItem> treeViewItems;
    public static ItemSelection[] itemSelections;

    public SceneTreeView(TreeViewState treeViewState, List<TreeViewItem> tree) : base(treeViewState)
    {
        treeViewItems = tree;
        itemSelections = new ItemSelection[tree.Count];
        for (int i = 0; i < tree.Count; i++)
        {
            itemSelections[i] = new ItemSelection(treeViewItems[i].id, true);
        }
        Reload();
    }

    private ItemSelection GetItem(int id)
    {
        for (int i = 0; i < treeViewItems.Count; i++)
        {
            if (itemSelections[i].ID == id)
            {
                return itemSelections[i];
            }
        }
        //BAD IF THIS HAPPENS
        return new ItemSelection(-1, true);
    }

    private bool ToggleSelection(int id)
    {
        for (int i = 0; i < itemSelections.Length; i++)
        {
            if (itemSelections[i].ID == id)
            {
                bool newValue = !itemSelections[i].Selected;
                itemSelections[i].Selected = newValue;
                if (!newValue) ToggleParent(id, newValue);
                ToggleChildren(id, newValue);
                return true;
            }
        }
        return false;
    }

    private bool ToggleChildren(int id, bool selection)
    {
        TreeViewItem item = null;
        for (int i = 0; i < treeViewItems.Count; i++)
        {
            if (treeViewItems[i].id == id)
            {
                item = treeViewItems[i];
                break;
            }
        }
        if (item == null || !item.hasChildren) return false;
        List<TreeViewItem> children = item.children;
        if (children.Count > 0 && children != null)
        {
            foreach (TreeViewItem t in children)
            {
                for (int i = 0; i < itemSelections.Length; i++)
                {
                    if (itemSelections[i].ID == t.id)
                    {
                        itemSelections[i].Selected = selection;
                        break;
                    }
                }
                if (t.hasChildren) ToggleChildren(t.id, selection);
            }
            return true;
        }
        return false;
    }

    private void ToggleParent(int id, bool selection)
    {
        for (int i = 0; i < treeViewItems.Count; i++)
        {
            if (treeViewItems[i].id == id)
            {
                TreeViewItem item = treeViewItems[i];
                if (item.parent != null && item.parent.depth >= 0)
                {
                    for (int j = 0; j < itemSelections.Length; j++)
                    {
                        if (itemSelections[j].ID == item.parent.id)
                        {
                            itemSelections[j].Selected = selection;
                            if (item.parent.parent != null) ToggleParent(item.parent.id, selection);
                        }
                    }
                }
            }
        }
    }

    protected override void RowGUI(RowGUIArgs args)
    {
        //Layout Setup
        extraSpaceBeforeIconAndLabel = 20f;
        Rect toggleRect = args.rowRect;
        toggleRect.x += GetContentIndent(args.item);
        toggleRect.width = 16f;
        // Event to Toggle Selection
        Event evt = Event.current;
        if (evt.type == EventType.MouseDown && toggleRect.Contains(evt.mousePosition))
        {
            ToggleSelection(args.item.id);
        }
        // Item enabled toggle 
        ItemSelection item = GetItem(args.item.id);
        if (item.ID == -1)
        {
            if (AssetCleaner.useDebugging) Debug.Log("Bad Item");
            return;
        }
        EditorGUI.Toggle(toggleRect, item.Selected);
        base.RowGUI(args);
    }

    protected override TreeViewItem BuildRoot()
    {
        var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
        SetupParentsAndChildrenFromDepths(root, treeViewItems);
        return root;
    }
}
