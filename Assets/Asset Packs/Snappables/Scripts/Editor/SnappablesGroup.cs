#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Snappables {
	[CreateAssetMenu(fileName = "ObjectsGroup", menuName = "ScriptableObjects/Snappables Group")]
	public class SnappablesGroup : ScriptableObject {
		[Tooltip("Asset path to folder of setup prefabs")]
		public string prefabsPath = "Assets/Prefabs/YOUR_PREFABS_HERE";
		[Tooltip("Initial rotation to spawn with, useful if prefab is spawning in a way where it needs to be rotated")]
		public float spawnRotation = 0f;
	}

	[CustomEditor(typeof(SnappablesGroup))]
	public class SnappablesGroupGUI : Editor {
		SnappablesGroup group;

		public override void OnInspectorGUI() {
			group = (SnappablesGroup)target;

			using(new GUILayout.HorizontalScope()) {
				if(GUILayout.Button(new GUIContent("Browse Prefab Path...", "Open file explorer to browse for the path of the prefabs"), GUILayout.ExpandWidth(false))) {
					string path = EditorUtility.OpenFolderPanel("Group Prefabs Path", "Assets", "");
					if(!string.IsNullOrEmpty(path)) {
						Undo.RecordObject(this, "Modified Group Settings");
						group.prefabsPath = path.Substring(path.IndexOf("Assets"));
					}

					EditorUtility.SetDirty(group);
				}
			}

			base.OnInspectorGUI();
		}
	}
}
#endif