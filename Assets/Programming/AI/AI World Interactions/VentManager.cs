using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BehaviorDesigner.Runtime.ObjectDrawers;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace CoreCraft.Enemy
{
    public class VentManager : MonoBehaviour
    {
        #region Odin Only

        [TabGroup("Level Design Tool"),
         SerializeField,
         InlineButton("ResetVentCount")]
        private string _ventName;

        [Space,
         TabGroup("Level Design Tool"),
         ShowInInspector,
         ValueDropdown("_ventObjectList"),
         InlineButton("SelectVentObject", "Select Vent Object"),
         InlineButton("DestroySelectedVentObject", "X")]
        private GameObject _previewVentStruct;

        [Space,
         TabGroup("Level Design Tool"),
         SerializeField,
         Range(0.0f,15.0f)]
        private float _spawnDistance;

        [TabGroup("Level Design Tool"),
         SerializeField,
         Range(0.0f, 7.0f)]
        private float _spawnHeight;

        [SerializeField, HideInInspector, SceneObjectsOnly]
        private List<GameObject> _ventObjectList = new List<GameObject>();

        [SerializeField, HideInInspector]
        private int _ventCount = 0;

        #endregion

        [TabGroup("Prefab Data"),
         SerializeField,
         AssetsOnly]
        private GameObject _ventPrefabObject;

        private List<VentStruct> _ventList = new List<VentStruct>();

        void Start()
        {
            CleanUpVentList();
        }

        void Update()
        {

        }

        private void CleanUpVentList()
        {
            for (int i = _ventList.Count - 1; i >= 0; i--)
            {
                if (_ventList[i].VentObject == null)
                    _ventList.RemoveAt(i);
            }
        }

        #region Odin

        [TabGroup("Clear Manager"),
        Button("CLEAR VENT MANAGER", ButtonSizes.Large)]
        private void ClearAll()
        {
            for (int i = _ventObjectList.Count - 1; i >= 0; i--)
            {
                if (_ventObjectList[i] != null)
                    DestroyImmediate(_ventObjectList[i]);
            }
            CleanUpVentList();
            ClearVentObjectList();
            ResetVentCount();
        }

        private void ResetVentCount()
        {
            _ventCount = 0;
        }

        private void DestroySelectedVentObject(GameObject ventObject)
        {
            DestroyImmediate(ventObject);
            CleanUpVentList();
            ClearVentObjectList();
        }

        private void SelectVentObject(GameObject selectVent)
        {
            if (selectVent != null)
                UnityEditor.Selection.activeObject = selectVent;
        }

        [TabGroup("Level Design Tool"),
         Button("Cleaning up vent list", ButtonSizes.Medium),
         Tooltip("Cleaning up list if vent was manually removed from the scene ")]
        private void ClearVentObjectList()
        {
            for (int i = _ventObjectList.Count - 1; i >= 0; i--)
            {
                if (_ventObjectList[i] == null)
                    _ventObjectList.RemoveAt(i);
            }
        }

        [TabGroup("Level Design Tool"),
         Button("Spawn vent object", ButtonSizes.Medium)]
        private void SpawnVentObjectInEditor()
        {
            VentStruct ventStruct = new VentStruct();
            Transform sceneCameraTransform = UnityEditor.EditorWindow.GetWindow<SceneView>().camera.transform;
            ventStruct.VentObject = Instantiate(_ventPrefabObject, sceneCameraTransform.position + sceneCameraTransform.forward * _spawnDistance + Vector3.up * _spawnHeight, Quaternion.identity, this.transform);

            ventStruct.VentObject.name = $"{((_ventName.Length > 0) ? $"{_ventName}" : $"Vent: {_ventCount}")}";

            while (_ventObjectList.Any(o => o.name == ventStruct.VentObject.name))
                ventStruct.VentObject.name += $" {_ventCount}";

            _ventList.Add(ventStruct);
            _ventObjectList.Add(ventStruct.VentObject);
            _ventCount++;
        }

        #endregion
    }
    struct VentStruct
    {
        public Vent Vent
        {
            set { VentObject.GetComponent<Vent>(); }
        }
        public GameObject VentObject;
    }
}
