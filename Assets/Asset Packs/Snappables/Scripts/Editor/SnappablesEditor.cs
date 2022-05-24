#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Snappables {
	public class SnappablesEditor : EditorWindow {
		#region Variables
		//Spawn Settings
		private Transform spawnedParent;
		private bool autoSelectSpawn = true;
		private bool rounding = true;
		private bool placementOverride = false;
		private float scaleFactor = 1f;
		private float rotationIncrement = 90f;
		private Axis rotationAxis;
		private float overlapRadius = 0.05f;

		//Group / Editor Settings
		private SnappablesGroup group;
		public SnappablesSettings settings;
		private bool editorSettings = false;
		private bool keybinds = false;
		private bool internalSettings = false;

		//Window
		private readonly string version = "1.1";
		private readonly string lastUpdate = "2021-03-28";
		private Vector2 scrollPos = Vector2.zero;

		//Main lists
		public List<GameObject> prefabs = new List<GameObject>();
		private List<Texture2D> prefabImages = new List<Texture2D>();
		private List<SnapPoint> snapPoints = new List<SnapPoint>();
		private SnapPoint[] previewSnapPoints;

		//Misc Refs
		private Transform selectedPoint;
		private int selectedPointInt;
		private int previewPointInt;
		private Vector3 previewScaleCache;
		private GameObject selectedPrefab;
		private GameObject prefabPreview;
		private GameObject lastSelectedGO;
		private bool spawn = false;
		private bool noGroup = false;
		#endregion

		#region GUI
		private void OnGUI() { //EditorWindow refresh
			if(noGroup) {
				EditorGUILayout.HelpBox("No Snappables Groups exist! Please create one using Assets/Create/ScriptablesObjects/Snappables Group and reopen the window!", MessageType.Error, true);
				return;
			}

			HandleInput();

			//List all prefabs
			EditorGUILayout.BeginVertical();
			scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUIStyle.none, GUI.skin.verticalScrollbar);

			int horizCount = (int)EditorGUIUtility.currentViewWidth / (int)settings.buttonsSize;
			float buttonSize = (EditorGUIUtility.currentViewWidth - 30f) / horizCount;
			int buttonHoriz = 0;
			for(int i = 0; i < prefabs.Count; i++) {
				//Kinda jank method for a "grid"
				if(buttonHoriz == horizCount) {
					EditorGUILayout.EndHorizontal();
					buttonHoriz = 0;
				}
				if(buttonHoriz == 0) {
					EditorGUILayout.BeginHorizontal(GUILayout.Width(horizCount * buttonSize + 20f));
				}
				buttonHoriz++;

				Color colorCache = GUI.backgroundColor;
				if(ReferenceEquals(selectedPrefab, prefabs[i])) GUI.backgroundColor = settings.buttonSelectedColor * 2f;

				GUIContent buttonContent = prefabImages[i] != null ? new GUIContent(prefabImages[i], prefabs[i].name) : new GUIContent(prefabs[i].name); //Weird, unknown edge case fix found by Colin (Thank you :) )
				if(GUILayout.Button(buttonContent, GUILayout.MaxWidth(buttonSize), GUILayout.MaxHeight(buttonSize))) {
					SelectPrefabButton(prefabs[i]);
				}

				GUI.backgroundColor = colorCache;
			}
			if(buttonHoriz != 0) { //If ended on a non-new line, end line
				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
			
			//Selected Text
			string selectedItem = selectedPrefab != null ? selectedPrefab.name : "None";
			string selected = selectedPoint != null || spawnedParent != null ? "Selected Prefab: " + selectedItem : "Select a point on a Snappable object in the scene or assign a Spawned Parent";
			GUILayout.Label(selected, EditorStyles.helpBox);

			//Selection Buttons
			if(selectedPoint == null && spawnedParent == null) EditorGUI.BeginDisabledGroup(true);
			if(GUILayout.Button(new GUIContent("Open Search Menu ( " + settings.searchMenu + " )", "Open the Snappables prefab search menu"))) OpenSearch();
			EditorGUI.EndDisabledGroup();
			using(new GUILayout.HorizontalScope()) {
				if(selectedPoint == null || snapPoints.Count <= 1) EditorGUI.BeginDisabledGroup(true);
				if(GUILayout.Button(new GUIContent("Cycle Point ( " + settings.cyclePoint + " )", "Cycle the selected point on the currently selected object"), GUILayout.Width(EditorGUIUtility.currentViewWidth / 2f - 4f))) CycleSelectedPoint();
				EditorGUI.EndDisabledGroup();
				if(prefabPreview == null) EditorGUI.BeginDisabledGroup(true);
				if(GUILayout.Button(new GUIContent("Cycle Target ( " + settings.cycleTarget + " )", "Cycle the target snap point on the preview object"), GUILayout.Width(EditorGUIUtility.currentViewWidth / 2f - 4f))) CyclePreviewPoint();
			}

			using(new GUILayout.HorizontalScope()) {
				if(GUILayout.Button(new GUIContent("Rotate Prefab ( " + settings.rotatePrefab + " )", "Rotate the preview prefab around the currently selected snap point"), GUILayout.Width(EditorGUIUtility.currentViewWidth / 2f - 4f))) RotatePreview(rotationAxis, rotationIncrement);
				if(GUILayout.Button(new GUIContent("Spawn Prefab ( " + settings.spawnPrefab + " )", "Spawn the currently selected prefab"), GUILayout.Width(EditorGUIUtility.currentViewWidth / 2f - 4f))) SpawnPrefab();
			}
			EditorGUI.EndDisabledGroup();

			//Spawn Settings
#if UNITY_2019_1_OR_NEWER //2018 doesn't allow spaces :(
			EditorGUILayout.Space(15f);
#endif
			GUILayout.Label("Spawn Settings", EditorStyles.boldLabel);
			EditorGUI.BeginChangeCheck();
			group = (SnappablesGroup)EditorGUILayout.ObjectField(new GUIContent("Selected Group", "The group of snappable objects which will be loaded and placable"), group, typeof(SnappablesGroup), group);
			if(EditorGUI.EndChangeCheck()) {
				settings.lastUsedGroup = group;
				RefreshAssets();
			}
			if(group == null)
				EditorGUILayout.HelpBox("Please assign a Snappables Group!", MessageType.Error, true);
			spawnedParent = (Transform)EditorGUILayout.ObjectField(new GUIContent("Spawned Parent", "The parent object to attach spawned prefabs to"), spawnedParent, typeof(Transform), spawnedParent);
			autoSelectSpawn = EditorGUILayout.Toggle(new GUIContent("Auto-select Spawned", "Automatically select the newly spawned prefab?"), autoSelectSpawn);
			using(new GUILayout.HorizontalScope(GUILayout.Width(EditorGUIUtility.currentViewWidth - 5f))) {
				rotationIncrement = EditorGUILayout.FloatField(new GUIContent("Rotation Increment", "Increment at which to rotate the preview object"), rotationIncrement);
				if(GUILayout.Button("Half ( [ )", EditorStyles.miniButtonLeft)) MultiplyRotation(0.5f);
				if(GUILayout.Button("Double ( ] )", EditorStyles.miniButtonRight)) MultiplyRotation(2f);
			}
			rotationAxis = (Axis)EditorGUILayout.EnumPopup(new GUIContent("Rotation Axis", "Axis of which to rotate the preview around"), rotationAxis);
			EditorGUI.BeginChangeCheck();
			scaleFactor = EditorGUILayout.FloatField(new GUIContent("Scale Factor", "Scale multiplier for spawned prefabs"), scaleFactor);
			if(EditorGUI.EndChangeCheck()) UpdatePrefabPreview();

			//Editor Settings
#if UNITY_2019_1_OR_NEWER
			EditorGUILayout.Space(15f);
#endif
			using(new GUILayout.HorizontalScope()) {
				GUILayout.Label("Editor Settings", EditorStyles.boldLabel);
				GUILayout.Space(EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth - EditorGUIUtility.fieldWidth);
				if(GUILayout.Button(new GUIContent("Reset to Default", "Reset all Snappables editor settings listed below to default (Besides Internal, it is not saved)"), GUILayout.ExpandWidth(false))) {
					settings.SetDefaultSettings();
				}
			}
			
			//Editor
			editorSettings = EditorGUILayout.Foldout(editorSettings, "Editor");
			if(editorSettings) {
				EditorGUI.BeginChangeCheck();
				settings.buttonsSize = EditorGUILayout.Slider(new GUIContent("Buttons Size", "Size of the button grid of prefabs from the current group"), settings.buttonsSize, 40f, 160f);
				if(EditorGUI.EndChangeCheck())
					Repaint();
				settings.buttonSelectedColor = EditorGUILayout.ColorField(new GUIContent("Buttons Selected Color", "Color of the currently selected prefab grid button"), settings.buttonSelectedColor);
				settings.handlesSize = EditorGUILayout.Slider(new GUIContent("Handles Size", "Size of the boxes indicating snap points in the scene view"), settings.handlesSize, 0f, 1f);
				settings.handlesPointColor = EditorGUILayout.ColorField(new GUIContent("Handles Point Color", "Color of the boxes indicating snap points"), settings.handlesPointColor);
				settings.handlesSelectedColor = EditorGUILayout.ColorField(new GUIContent("Handles Selected Color", "Color of the box of the current selected snap point"), settings.handlesSelectedColor);
				settings.previewMaterial = (Material)EditorGUILayout.ObjectField(new GUIContent("Preview Material", "Material to use for the preview of the prefab"), settings.previewMaterial, typeof(Material), settings.previewMaterial);
			}

			//Keybinds
			keybinds = EditorGUILayout.Foldout(keybinds, "Keybinds");
			if(keybinds) {
				settings.searchMenu = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Search Menu", "Key to hit to bring up the search menu"), settings.searchMenu);
				settings.cyclePoint = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Cycle Point", "Key to hit to cycle the point of the selected object"), settings.cyclePoint);
				settings.cycleTarget = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Cycle Target", "Key to hit to cycle the point of the prefab preview"), settings.cycleTarget);
				settings.rotatePrefab = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Rotate Prefab", "Key to hit to rotate the prefab preview around the defined rotation axis"), settings.rotatePrefab);
				settings.spawnPrefab = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Spawn Prefab", "Key to hit to spawn the selected prefab at the preview"), settings.spawnPrefab);
			}
			
			//Internal
			internalSettings = EditorGUILayout.Foldout(internalSettings, "Internal");
			if(internalSettings) {
				rounding = EditorGUILayout.Toggle(new GUIContent("Rounding", "Round placed objects to nearest hundredth decimal place to avoid floating point imprecision"), rounding);
				EditorGUI.BeginChangeCheck();
				placementOverride = EditorGUILayout.Toggle(new GUIContent("Link Placement Override", "Allows placement on points which were already placed on"), placementOverride);
				if(EditorGUI.EndChangeCheck())
					GetSnapPoints();
				overlapRadius = EditorGUILayout.FloatField(new GUIContent("Overlap Radius", "Radius of which to check at all points of spawned object for overlapping points to link"), overlapRadius);
				if(GUILayout.Button(new GUIContent("Refresh Assets", "Reload all prefabs from active group and placement material"), GUILayout.ExpandWidth(false))) {
					RefreshAssets();
				}
			}

			//Version Text
			EditorGUILayout.Space();
			GUILayout.Label("Snappables Editor " + version + "\nMade by Camobiwon | Last Update: " + lastUpdate, EditorStyles.miniLabel);
		}

		private void OnSceneGUI(SceneView sceneView) { //Scene view refresh
			HandleInput();

			if(Selection.activeGameObject == null) {
				DestroyImmediate(prefabPreview);
				lastSelectedGO = null;
				selectedPrefab = null;
				selectedPoint = null;
				Repaint();
				return;
			}

			if(!Selection.activeGameObject.Equals(lastSelectedGO) || lastSelectedGO == null) {
				//Re-setup if new GO was selected
				Repaint();

				if(!spawn) {
					//If selection changed not due to spawning, reset
					DestroyImmediate(prefabPreview);
					selectedPrefab = null;
				}

				lastSelectedGO = Selection.activeGameObject;
				GetSnapPoints();
				spawn = false;
			}

			//Draw handles
			foreach(SnapPoint obj in snapPoints) {
				if(obj == null)
					return;
				
				if(obj.transform.Equals(selectedPoint)) {
					Handles.color = settings.handlesSelectedColor;
				} else {
					Handles.color = settings.handlesPointColor;
				}

				if(Handles.Button(obj.transform.position, SceneView.lastActiveSceneView.rotation, settings.handlesSize, settings.handlesSize, Handles.RectangleHandleCap)) { //Snap point button
					SelectPoint(obj.gameObject);
					Repaint();
				}
			}
		}

		private void OnSelectionChanged() { //GameObject selection change
			if(Selection.activeGameObject == null) {
				DestroyImmediate(prefabPreview);
				lastSelectedGO = null;
				selectedPrefab = null;
				selectedPoint = null;
				Repaint();
				return;
			}

			if(!Selection.activeGameObject.Equals(lastSelectedGO) || lastSelectedGO == null) {
				//Re-setup if new GO was selected
				Repaint();

				if(!spawn) {
					//If selection changed not due to spawning, reset
					DestroyImmediate(prefabPreview);
					selectedPrefab = null;
				}

				lastSelectedGO = Selection.activeGameObject;
				GetSnapPoints();
				spawn = false;
			}
		}
		#endregion

		#region Preview / Spawning
		private void SelectPrefabButton(GameObject prefab) { //Exclusive method for buttons to avoid clicking on stuff in hierarcy and having it spawn
			if(prefabPreview != null && selectedPrefab != null && prefab.name == prefabPreview.name) {
				SpawnPrefab();
				return;
			}

			SelectPrefab(prefab);
		}

		public void SelectPrefab(GameObject prefab) { //Select prefab from list
			if(spawnedParent != null && selectedPoint == null && spawnedParent.childCount == 0) {
				selectedPoint = spawnedParent;
			}

			if(selectedPoint == null || prefab == null) {
				return;
			}

			DestroyImmediate(prefabPreview);

			//Instantiate
			selectedPrefab = prefab;
			prefabPreview = (GameObject)PrefabUtility.InstantiatePrefab(selectedPrefab);
			previewSnapPoints = prefabPreview.GetComponentsInChildren<SnapPoint>();
			previewPointInt = 0;
			previewScaleCache = prefabPreview.transform.localScale;

			if(previewSnapPoints.Length == 0) { //If item has no snap points, return
				Debug.LogWarning("Tried to spawn an item with no snap points!");
				selectedPrefab = null;
				DestroyImmediate(prefabPreview);
				return;
			}

			//Setup preview position / material
			UpdatePrefabPreview();
			SetPreviewMaterial();

			//Remove all colliders for better previewing
			Collider[] colliders = prefabPreview.GetComponentsInChildren<Collider>();
			foreach(Collider col in colliders) {
				DestroyImmediate(col);
			}
		}

		private void SpawnPrefab() { //Spawn prefab at prefab preview
			if(selectedPoint == null || selectedPrefab == null)
				return;

			if(spawnedParent != null)
				Undo.RegisterFullObjectHierarchyUndo(spawnedParent, "Updated Spawned Parent");

			if(ReferenceEquals(selectedPoint, spawnedParent)) { //If starting at parent, create new snap point and link to first spawned
				GameObject firstPoint = new GameObject();
				firstPoint.transform.SetParent(spawnedParent);
				firstPoint.transform.SetPositionAndRotation(spawnedParent.position, spawnedParent.rotation);
				snapPoints.Add(firstPoint.AddComponent<SnapPoint>());
				Undo.RegisterCreatedObjectUndo(firstPoint, "Created Parent Snap Point");
			}

			//Spawn the selected prefab
			GameObject spawned = PrefabUtility.InstantiatePrefab(selectedPrefab) as GameObject;
			
			if(prefabPreview == null) { //Weird edge case for keybind timing issues in OnGUI
				DestroyImmediate(spawned);
				return;
			}
			spawned.transform.localScale = RoundVec3(previewScaleCache * scaleFactor);
			spawned.transform.SetPositionAndRotation(RoundVec3(prefabPreview.transform.position), Quaternion.Euler(RoundVec3(prefabPreview.transform.rotation.eulerAngles)));
			spawned.transform.SetParent(spawnedParent);
			Undo.RegisterCreatedObjectUndo(spawned, "Spawned Prefab");

			//Set linked points
			if(snapPoints.Count != 0) { //Catch in case prefab is spawned from empty parent
				SnapPoint[] spawnedPoints = spawned.GetComponentsInChildren<SnapPoint>();
				snapPoints[selectedPointInt].linkedPoints.Add(spawnedPoints[previewPointInt]);
				EditorUtility.SetDirty(snapPoints[selectedPointInt]);
				spawnedPoints[previewPointInt].linkedPoints.Add(snapPoints[selectedPointInt]);
				EditorUtility.SetDirty(spawnedPoints[previewPointInt]);

				foreach(SnapPoint point in spawnedPoints) { //Link additional points with OverlapSphere
					Collider[] cols = Physics.OverlapSphere(point.transform.position, overlapRadius);
					foreach(Collider col in cols) { //Check each collider in overlap
						SnapPoint colPoint = col.GetComponent<SnapPoint>();
						if(colPoint != null && !ReferenceEquals(colPoint, point) && !ReferenceEquals(colPoint.transform.parent, point.transform.parent) && !ReferenceEquals(colPoint, snapPoints[selectedPointInt])) { //Check individial point
							colPoint.linkedPoints.Add(point);
							EditorUtility.SetDirty(colPoint);
							point.linkedPoints.Add(colPoint);
							EditorUtility.SetDirty(point);
							break;
						}
					}
				}
			}

			DestroyImmediate(prefabPreview);
			spawn = true;

			if(autoSelectSpawn)
				Selection.activeGameObject = spawned;
			else
				GetSnapPoints();
		}

		private void UpdatePrefabPreview() { //Update transform of prefab preview
			if(prefabPreview == null || group == null)
				return;

			//Set pos, rot, and parent of preview to selected point data
			GameObject previewHolderGO = new GameObject();
			Transform previewHolder = previewHolderGO.transform;
			prefabPreview.transform.localScale = previewScaleCache * scaleFactor;
			previewHolder.SetPositionAndRotation(previewSnapPoints[previewPointInt].transform.position, previewSnapPoints[previewPointInt].transform.rotation);
			prefabPreview.transform.SetParent(previewHolder);
			previewHolder.SetPositionAndRotation(selectedPoint.position, selectedPoint.rotation);
			previewHolder.RotateAround(selectedPoint.position, Quaternion.Euler(selectedPoint.eulerAngles) * Vector3.right, 180f);
			prefabPreview.transform.SetParent(null);
			DestroyImmediate(previewHolderGO);

			RotatePreview(Axis.ZAxis, 180f); //For some reason things were spawning upside down

			if(group.spawnRotation != 0f) //If group spawn rotation is set, rotate prefab to default value
				RotatePreview(Axis.ZAxis, group.spawnRotation);
		}

		private void RotatePreview(Axis axis, float increment) { //Rotate prefab preview around current selected snap point
			if(prefabPreview == null)
				return;

			//Rotate preview around selection point using rotation axis (Setting originally from group main axis)
			GameObject previewHolderGO = new GameObject();
			Transform previewHolder = previewHolderGO.transform;
			previewHolder.SetPositionAndRotation(previewSnapPoints[previewPointInt].transform.position, previewSnapPoints[previewPointInt].transform.rotation);
			prefabPreview.transform.SetParent(previewHolder);
			previewHolder.RotateAround(selectedPoint.position, Quaternion.Euler(selectedPoint.eulerAngles) * GetAxis(axis), increment);
			prefabPreview.transform.SetParent(selectedPoint);
			DestroyImmediate(previewHolderGO);
		}

		private void SetPreviewMaterial() { //Apply preview material to prefab preview
			MeshRenderer[] meshes = prefabPreview.GetComponentsInChildren<MeshRenderer>();
			foreach(MeshRenderer mesh in meshes) {
				Material[] mats = new Material[mesh.sharedMaterials.Length];
				for(int i = 0; i < mesh.sharedMaterials.Length; i++) {
					mats[i] = settings.previewMaterial;
				}
				mesh.sharedMaterials = mats;
			}
		}
		#endregion

		#region Editor / Points
		private void SelectPoint(GameObject go) { //Select snap point on current object
			selectedPoint = go.transform;
			UpdatePrefabPreview();
			for(int i = 0; i < snapPoints.Count; i++) {
				if(ReferenceEquals(go, snapPoints[i].gameObject)) {
					selectedPointInt = i;
					return;
				}
			}
		}

		private void CycleSelectedPoint() { //Keybind / button to cycle between available points on current object
			if(snapPoints.Count == 0)
				return;

			selectedPointInt++;
			if(selectedPointInt > snapPoints.Count - 1)
				selectedPointInt = 0;

			selectedPoint = snapPoints[selectedPointInt].transform;
			UpdatePrefabPreview();
		}

		private void CyclePreviewPoint() { //Keybind / button to cycle between available points on prefab preview
			if(prefabPreview == null)
				return;

			previewPointInt++;
			if(previewPointInt > previewSnapPoints.Length - 1)
				previewPointInt = 0;

			UpdatePrefabPreview();
		}

		private void GetSnapPoints() { //Get all snap points on current selection
			if(Selection.activeGameObject == null)
				return;

			SnapPoint[] allPoints = Selection.activeGameObject.GetComponentsInChildren<SnapPoint>();
			snapPoints.Clear();
			foreach(SnapPoint point in allPoints) {
				for(int i = point.linkedPoints.Count - 1; i >= 0; i--) { //Remove any null points
					if(point.linkedPoints[i] == null) {
						point.linkedPoints.RemoveAt(i);
					}
				}

				if(point.linkedPoints.Count == 0 || point.overrideLink || placementOverride) {
					snapPoints.Add(point);
				}
			}

			selectedPoint = null;
			selectedPointInt = 0;
			CycleSelectedPoint();
			SelectPrefab(selectedPrefab);
		}
		#endregion

		#region Internals
		private void RefreshAssets() { //Reload all prefabs from group / preview material
			if(group == null)
				return;

			//Clear old selection
			DestroyImmediate(prefabPreview);
			selectedPrefab = null;

			//Using group path, get all prefabs
			string[] prefabsStrings;

			try {
				prefabsStrings = Directory.GetFiles(group.prefabsPath, "*.prefab", SearchOption.TopDirectoryOnly);
			} catch {
				Debug.LogError("Group prefab path is not valid! Please check <color=yellow>" + group.name + "</color> group settings to ensure proper prefab path");
				return;
			}

			prefabs.Clear();
			prefabImages.Clear();

			for(int i = 0; i < prefabsStrings.Length; i++) {
				//Load indivdual prefab
				GameObject prefab = (GameObject)AssetDatabase.LoadMainAssetAtPath(prefabsStrings[i]);
				if(prefab.GetComponentInChildren<SnapPoint>() != null) { //Only add items with snap points
					prefabs.Add(prefab);
				} else {
					continue;
				}

				//Generate preview image
				Editor editor = Editor.CreateEditor(prefab);
				Texture2D texture = editor.RenderStaticPreview(prefabsStrings[i], null, 200, 200);
				DestroyImmediate(editor);
				prefabImages.Add(texture);
			}

			rotationAxis = Axis.ZAxis; //Set rotation axis to current group main axis

			//If reloaded assets with new preview material defined, update preview prefab
			if(prefabPreview != null) {
				SetPreviewMaterial();
			}
		}

		private void MultiplyRotation(float rotateFactor) { //Keybinds to double or half rotation
			rotationIncrement *= rotateFactor;
			Repaint();
		}

		private Vector3 GetAxis(Axis axis) { //Utility to get axis from enum
            return axis switch {
                Axis.XAxis => Vector3.right,
                Axis.YAxis => Vector3.up,
                Axis.ZAxis => Vector3.forward,
                _ => Vector3.zero,
            };
        }

		private Vector3 RoundVec3(Vector3 vec3) { //Rounding
			if(rounding) {
				return new Vector3(
					Mathf.Round(vec3.x * 100f) / 100f,
					Mathf.Round(vec3.y * 100f) / 100f,
					Mathf.Round(vec3.z * 100f) / 100f);
			} else { //If no rounding, return original
				return vec3;
			}
		}

		private void HandleInput() { //Main keybind handling
			Event current = Event.current;
			if(current.type != EventType.KeyDown)
				return;

			//Keybinds
			switch(current.keyCode) { //Holy cursed, but switch statements need a constant value otherwise
				case KeyCode key when key == settings.cyclePoint:
					CycleSelectedPoint();
					break;
				case KeyCode key when key == settings.cycleTarget:
					CyclePreviewPoint();
					break;
				case KeyCode key when key == settings.rotatePrefab:
					RotatePreview(rotationAxis, rotationIncrement);
					break;
				case KeyCode key when key == settings.spawnPrefab:
					SpawnPrefab();
					break;
				case KeyCode key when key == settings.searchMenu:
					OpenSearch();
					break;
				case KeyCode.LeftBracket:
					MultiplyRotation(0.5f);
					break;
				case KeyCode.RightBracket:
					MultiplyRotation(2f);
					break;
			}
		}

		[MenuItem("Tools/Snappables")]
		public static void SnappablesWindow() { //Main window
			//Create or get existing window
			SnappablesEditor window = (SnappablesEditor)GetWindow(typeof(SnappablesEditor), false, "Snappables");

			try { //Get settings
				window.settings = (SnappablesSettings)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("t:SnappablesSettings")[0]), typeof(SnappablesSettings)); //Get settings
			} catch { //If no settings file, create and assign
				SnappablesSettings created = ScriptableObject.CreateInstance<SnappablesSettings>();
				AssetDatabase.CreateAsset(created, "Assets/Snappables/SnappablesSettings.asset");
				window.settings = AssetDatabase.LoadAssetAtPath<SnappablesSettings>("Assets/Snappables/SnappablesSettings.asset"); //CreateAsset doesn't return anything :(
				window.settings.SetDefaultSettings();
			}
			if(window.settings.lastUsedGroup != null) { //If default group, use that
				window.group = window.settings.lastUsedGroup;
			} else {
				try { //If no default group, get first
					window.group = (SnappablesGroup)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("t:SnappablesGroup")[0]), typeof(SnappablesGroup));
				} catch {
					window.noGroup = true;
				}
			}
			window.RefreshAssets();

			window.Show();
		}

		private void OpenSearch() {
			if(selectedPoint == null && spawnedParent == null)
				return;

			Vector2 mousePos = new Vector2(Event.current.mousePosition.x, Event.current.mousePosition.y - position.y);
			SnappablesSearch search = new SnappablesSearch();
			search.editor = this;
			PopupWindow.Show(new Rect(mousePos.x, mousePos.y, 200f, 100f), search);
		}

		private void OnEnable() { //Window open
#if UNITY_2019_1_OR_NEWER //Basic 2018 support
			SceneView.duringSceneGui += OnSceneGUI;
#else
			SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif
			Selection.selectionChanged += OnSelectionChanged;

			OnSelectionChanged(); //Call once to check current object
		}
		private void OnDisable() { //Window close
			EditorUtility.SetDirty(settings); //Make editor settings savable
#if UNITY_2019_1_OR_NEWER
			SceneView.duringSceneGui -= OnSceneGUI;
#else
			SceneView.onSceneGUIDelegate -= OnSceneGUI;
#endif
			Selection.selectionChanged -= OnSelectionChanged;

			DestroyImmediate(prefabPreview);
		}

		public enum Axis {
			XAxis, YAxis, ZAxis
		}
#endregion
	}
}
#endif