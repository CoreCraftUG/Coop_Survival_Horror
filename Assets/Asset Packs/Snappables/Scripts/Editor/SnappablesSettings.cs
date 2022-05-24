#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Snappables {
	public class SnappablesSettings : ScriptableObject {
		public SnappablesGroup lastUsedGroup;
		public float buttonsSize = 80f;
		public Color buttonSelectedColor = Color.cyan;
		public float handlesSize = 0.1f;
		public Color handlesPointColor = Color.blue;
		public Color handlesSelectedColor = Color.cyan;
		public Material previewMaterial;
		[Header("Keybinds")]
		public KeyCode searchMenu = KeyCode.G;
		public KeyCode cyclePoint = KeyCode.C;
		public KeyCode cycleTarget = KeyCode.V;
		public KeyCode rotatePrefab = KeyCode.B;
		public KeyCode spawnPrefab = KeyCode.Alpha1;

		public void SetDefaultSettings() {
			Undo.RecordObject(this, "Restore Default Snappables Settings");
			buttonsSize = 80f;
			buttonSelectedColor = Color.cyan;
			handlesSize = 0.1f;
			handlesPointColor = Color.blue;
			handlesSelectedColor = Color.cyan;
			previewMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Snappables/Materials/Placement.mat");

			searchMenu = KeyCode.G;
			cyclePoint = KeyCode.C;
			cycleTarget = KeyCode.V;
			rotatePrefab = KeyCode.B;
			spawnPrefab = KeyCode.BackQuote;
	}
	}
}
#endif