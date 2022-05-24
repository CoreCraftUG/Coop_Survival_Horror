using System.Collections.Generic;
using UnityEngine;
using UnityEditor.IMGUI.Controls;

public class ExtensionsTreeView : TreeView
{
    public List<TreeViewItem> treeViewItems;
    public ItemSelection[] itemSelections;

    public ExtensionsTreeView(TreeViewState treeViewState, List<TreeViewItem> tree) : base(treeViewState)
    {
        treeViewItems = tree;
        itemSelections = new ItemSelection[tree.Count];
        for (int i = 0; i < tree.Count; i++)
        {
            itemSelections[i] = new ItemSelection(treeViewItems[i].id, false);
        }
        Reload();
    }

    private bool ToggleSelection(int id)
    {
        for (int i = 0; i < itemSelections.Length; i++)
        {
            if (itemSelections[i].ID == id)
            {
                bool newValue = !itemSelections[i].Selected;
                itemSelections[i].Selected = newValue;
                return true;
            }
        }
        return false;
    }

    protected override void RowGUI(RowGUIArgs args)
    {
        //Layout Setup
        Rect toggleRect = args.rowRect;
        toggleRect.width = 100f;
        // Event to Toggle Selection
        Event evt = Event.current;
        if (evt.type == EventType.MouseDown && toggleRect.Contains(evt.mousePosition))
        {
            ToggleSelection(args.item.id);
        }
        base.RowGUI(args);
    }

    protected override TreeViewItem BuildRoot()
    {
        var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
        SetupParentsAndChildrenFromDepths(root, treeViewItems);
        return root;
    }
}
