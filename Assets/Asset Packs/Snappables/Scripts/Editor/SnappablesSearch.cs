#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Snappables {
	public class SnappablesSearch : PopupWindowContent {
		public SnappablesEditor editor;
		private List<GameObject> prefabs = new List<GameObject>();
		private string search;
		private Vector2 scrollPos = Vector2.zero;
		GameObject firstObject = null;

		public override void OnGUI(Rect rect) {
			if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return) { //If Enter is hit, select top option
				editor.SelectPrefab(firstObject);
				editor.Repaint();
				editorWindow.Close();
			}

			GUILayout.Label("Prefab Search", EditorStyles.boldLabel);
			GUILayout.Label("Enter to select top option", EditorStyles.miniLabel);
			
			GUI.SetNextControlName("SearchField");
			search = EditorGUILayout.TextField("", search); //Search bar
			GUI.FocusControl("SearchField"); //Force focus search bar, dumb but works

			scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUIStyle.none, GUI.skin.verticalScrollbar);
			TextAnchor alignCache = GUI.skin.button.alignment;
			GUI.skin.button.alignment = TextAnchor.MiddleLeft;
			firstObject = null;
			Color colorCache = GUI.backgroundColor;

			for(int i = 0; i < prefabs.Count; i++) {
				bool searchPassed = true;
				if(!string.IsNullOrEmpty(search)) { //Basic search algorithm, just splits the search and check if it contains any parts of it
					string[] searchSplit = search.Split(' ');
					foreach(string s in searchSplit) {
						if(!prefabs[i].name.ToLower().Contains(s.ToLower())) {
							searchPassed = false;
							break;
						}
					}
				}

				if(!searchPassed)
					continue;

				if(firstObject == null) { //Selected color on first button + assign as first object
					firstObject = prefabs[i];
					GUI.backgroundColor = editor.settings.buttonSelectedColor * 1.5f;
				}

				if(GUILayout.Button(prefabs[i].name)) { //Individual prefab button
					editor.SelectPrefab(prefabs[i]);
					editor.Repaint();
					editorWindow.Close();
				}
				GUI.backgroundColor = colorCache;
			}

			GUI.skin.button.alignment = alignCache;
			EditorGUILayout.EndScrollView();
		}

		public override void OnOpen() {
			foreach(GameObject prefab in editor.prefabs) { //Get all prefabs as strings
				prefabs.Add(prefab);
			}

			prefabs = prefabs.OrderBy(x => x.name).ThenBy(x => x.name.Length).ToList(); //Sort by alphabetical, then length

			editorWindow.Focus();
		}
	}
}
#endif