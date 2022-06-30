using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using BehaviorDesigner.Runtime.ObjectDrawers;
using CoreCraft.Core;
using Sirenix.OdinInspector;
using SplineMesh;
using UnityEditor;
using UnityEngine;
using DG.Tweening;
using Unity.Collections;

namespace CoreCraft.Enemy
{
    public class VentManager : MonoBehaviour
    {
        #region Odin Only

        [TabGroup("ParentGroup", "Level Design Tool",false, 0),
         SerializeField,
         InlineButton("ResetVentCount")]
        private string _ventName;

        [Space,
         TabGroup("ParentGroup", "Level Design Tool",false, 0),
         ShowInInspector,
         ValueDropdown("_ventObjectList"),
         InlineButton("SelectVentObject", "Select Vent Object"),
         InlineButton("DestroySelectedVentObject", "X")]
        private GameObject _previewVent1;

        [TabGroup("ParentGroup", "Level Design Tool",false, 0),
         ShowInInspector,
         ValueDropdown("_ventObjectList"),
         InlineButton("SelectVentObject", "Select Vent Object"),
         InlineButton("DestroySelectedVentObject", "X")]
        private GameObject _previewVent2;

        [Space,
         TabGroup("ParentGroup", "Level Design Tool",false, 0),
         ShowInInspector,
         SerializeField,
         MinValue(1),
         MaxValue(3)]
        private int _pathDirectionX;
        [TabGroup("ParentGroup", "Level Design Tool",false, 0),
         ShowInInspector,
         SerializeField,
         MinValue(1),
         MaxValue(3)]
        private int _pathDirectionY;
        [TabGroup("ParentGroup", "Level Design Tool",false, 0),
         ShowInInspector,
         SerializeField,
         MinValue(1),
         MaxValue(3)]
        private int _pathDirectionZ;

        [Space,
         TabGroup("ParentGroup", "Level Design Tool",false, 0),
         SerializeField]
        private bool _automaticMirrorPath;

        [TabGroup("ParentGroup", "Level Design Tool",false, 0),
         SerializeField]
        private bool _automaticDebugPath;

        [Space,
         TabGroup("ParentGroup", "Level Design Tool",false, 0),
         SerializeField,
         Range(0.0f,15.0f)]
        private float _spawnDistance;

        [TabGroup("ParentGroup", "Level Design Tool",false, 0),
         SerializeField,
         Range(0.0f, 7.0f)]
        private float _spawnHeight;

        [SerializeField, HideInInspector, SceneObjectsOnly]
        private List<GameObject> _ventObjectList = new List<GameObject>();

        [SerializeField, HideInInspector]
        private int _ventCount = 0;

        private bool _previewVentBool => _previewVent1 != null && _previewVent2 != null && _previewVent1 != _previewVent2;

        #endregion

        #region Debug

        [TabGroup("ParentGroup", "Debug",false, 1),
         ShowInInspector,
         ValueDropdown("_ventObjectList"),
         InlineButton("SelectVentObject", "Select Vent Object"),
         InlineButton("DestroySelectedVentObject", "X")]
        private GameObject _debugVent1;

        [TabGroup("ParentGroup", "Debug",false, 1),
         ShowInInspector,
         ValueDropdown("_ventObjectList"),
         InlineButton("SelectVentObject", "Select Vent Object"),
         InlineButton("DestroySelectedVentObject", "X")]
        private GameObject _debugVent2;

        [Space,
         TabGroup("ParentGroup", "Debug",false, 1),
         SerializeField]
        private float _soundTravelSpeed;

        [Space,
         TabGroup("ParentGroup", "Debug",false, 1),
         ShowInInspector,
         ValueDropdown("_ventCrawlingSFXList")]
        private AudioClip _debugVentAudioClip;

        [SerializeField,
         HideInInspector]
        private GameObject _ventPathObject;

        private bool _playMode => UnityEditor.EditorApplication.isPlaying;
        private bool _debugVentBool => _debugVent1 != null && _debugVent2 != null && _debugVent1 != _debugVent2;

        #endregion

        [TabGroup("ParentGroup", "Prefab Data", false, 2),
         SerializeField,
         AssetsOnly]
        private GameObject _ventPrefabObject;

        [TabGroup("ParentGroup", "Prefab Data", false, 2),
         SerializeField,
         AssetsOnly]
        private GameObject _ventSoundPrefabObject;

        [TabGroup("ParentGroup", "Prefab Data",false,2),
         SerializeField,
         AssetsOnly]
        private GameObject _ventPathPrefabObject;

        [TabGroup("ParentGroup", "Prefab Data", false, 2),
         SerializeField,
         AssetsOnly,
         InlineEditor(InlineEditorModes.SmallPreview)]
        private List<AudioClip> _ventCrawlingSFXList;

        [SerializeField,
         HideInInspector]
        private VentDictionary _ventDictionary;

        void Start()
        {
            CleanUpVentDictionary();
        }

        void Update()
        {

        }

        private void CleanUpVentDictionary()
        {
            if (_ventDictionary == null)
            {
                Debug.Log($"Vent Dictionary: {_ventDictionary}");
                return;
            }

            List<int> removeList = new List<int>();
            foreach (KeyValuePair<int, VentStruct> keyValuePair in _ventDictionary)
            {
                if (keyValuePair.Value.VentObject == null)
                    removeList.Add(keyValuePair.Key);
            }

            foreach (int i in removeList)
            {
                _ventDictionary.Remove(i);
            }
        }

        #region Odin

        #region Debug

        [EnableIf("_debugVentBool"),
         BoxGroup("ParentGroup/Debug/InnerGroup", false),
         Button("Debug Move Path", ButtonSizes.Medium)]
        private void DebugMoveObjectPath()
        {
            if (_ventDictionary == null || _debugVent1 == null || _debugVent2 == null)
            {
                Debug.Log($"Vent Dictionary: {_ventDictionary} Debug Vent 1: {_debugVent1} Debug Vent 2: {_debugVent2}");
                return;
            }

            Vector2Int Ids = GetDebugVentId1And2();
            if (Ids == Vector2Int.zero)
                return;

            int ventId1 = Ids.x;
            int ventId2 = Ids.y;

            if (_ventPathObject != null)
                DestroyImmediate(_ventPathObject);

            _ventPathObject = Instantiate(_ventPathPrefabObject, _debugVent1.transform.position, Quaternion.identity);

            int[] directionValues = GetDirectionValues(ventId1, ventId2);

            Dictionary<int, Vector3> positions = GetDirectionDictionary(directionValues);

            LineRenderer line = _ventPathObject.GetComponent<LineRenderer>();

            line.positionCount = 4;

            Vector3[] linePath = { _debugVent1.transform.position - _ventPathObject.transform.position,
                positions[directionValues[0]],
                positions[directionValues[1]],
                positions[directionValues[2]]};


            line.SetPositions(linePath);
            Debug.Log($"X: {directionValues[0]} Y: {directionValues[1]} Z: {directionValues[2]}");
        }

        [EnableIf("@this._ventPathObject != null"),
         BoxGroup("ParentGroup/Debug/InnerGroup", false),
         Button("Destroy Path", ButtonSizes.Medium)]
        private void DestroyPathObject()
        {
            if (_ventPathObject != null)
                DestroyImmediate(_ventPathObject);
        }

        [BoxGroup("ParentGroup/Debug/InnerGroup", false),
         Button("Debug Vent List", ButtonSizes.Medium)]
        private void DebugVentList()
        {
            Debug.Log($"Debug Started");
            if (_ventDictionary != null)
            {
                Debug.Log($"Dictionary length: {_ventDictionary.Count}");
                int i = 0;
                foreach (KeyValuePair<int, VentStruct> pair in _ventDictionary)
                {
                    Debug.Log($"Vent Dictionary position {i}: Key: {pair.Key} Value Object: {pair.Value.VentObject}");
                }
            }

            if (_ventObjectList != null)
            {
                Debug.Log($"Debug Dictionary finished \n VentObjectList length: {_ventObjectList.Count}");
                foreach (GameObject o in _ventObjectList)
                {
                    Debug.Log($"{o.name}");
                }
            }
        }

        [EnableIf("@this._debugVentBool && this._playMode"),
         BoxGroup("ParentGroup/Debug/Play Mode Debug", true),
         Button("Debug Object Move", ButtonSizes.Medium)]
        private void DebugMoveObject()
        {
            if (_ventDictionary == null || _debugVent1 == null || _debugVent2 == null)
            {
                Debug.Log($"Vent Dictionary: {_ventDictionary} Debug Vent 1: {_debugVent1} Debug Vent 2: {_debugVent2}");
                return;
            }

            GameObject obj = Instantiate(_ventSoundPrefabObject, _debugVent1.transform.position, Quaternion.identity);
            AudioSource source = obj.GetComponent<AudioSource>();
            source.clip = _debugVentAudioClip;
            source.Play();

            Vector2Int Ids = GetDebugVentId1And2();
            if (Ids == Vector2Int.zero)
                return;

            int ventId1 = Ids.x;
            int ventId2 = Ids.y;

            if (_ventDictionary.Any(p => p.Value.VentObject == _debugVent1))
            {
                ventId1 = _ventDictionary.Single(p => p.Value.VentObject == _debugVent1).Key;
            }
            else
            {
                Debug.Log("Debug Vent Object 1 == null");
                return;
            }
            if (_ventDictionary.Any(p => p.Value.VentObject == _debugVent2))
            {
                ventId2 = _ventDictionary.Single(p => p.Value.VentObject == _debugVent2).Key;
            }
            else
            {
                Debug.Log("Debug Vent Object 2 == null");
                return;
            }


            int[] directionValues = GetDirectionValues(ventId1, ventId2);

            Dictionary<int, Vector3> positions = new Dictionary<int, Vector3>();
            switch (directionValues[0])
            {
                case 1:
                    switch (directionValues[1])
                    {
                        case 2:
                            positions.Add(1, new Vector3(_debugVent2.transform.position.x - _debugVent1.transform.position.x, 0, 0));
                            positions.Add(2, new Vector3(0, _debugVent2.transform.position.y - _debugVent1.transform.position.y, 0));
                            positions.Add(3, new Vector3(0, 0, _debugVent2.transform.position.z - _debugVent1.transform.position.z));
                            break;
                        case 3:
                            positions.Add(1, new Vector3(_debugVent2.transform.position.x - _debugVent1.transform.position.x, 0, 0));
                            positions.Add(2, new Vector3(0, _debugVent2.transform.position.y - _debugVent1.transform.position.y, 0));
                            positions.Add(3, new Vector3(0, 0, _debugVent2.transform.position.z - _debugVent1.transform.position.z));
                            break;
                    }
                    break;
                case 2:
                    switch (directionValues[1])
                    {
                        case 1:
                            positions.Add(1, new Vector3(_debugVent2.transform.position.x - _debugVent1.transform.position.x, 0, 0));
                            positions.Add(2, new Vector3(0, _debugVent2.transform.position.y - _debugVent1.transform.position.y, 0));
                            positions.Add(3, new Vector3(0, 0, _debugVent2.transform.position.z - _debugVent1.transform.position.z));
                            break;
                        case 3:
                            positions.Add(3, new Vector3(_debugVent2.transform.position.x - _debugVent1.transform.position.x, 0, 0));
                            positions.Add(1, new Vector3(0, _debugVent2.transform.position.y - _debugVent1.transform.position.y, 0));
                            positions.Add(2, new Vector3(0, 0, _debugVent2.transform.position.z - _debugVent1.transform.position.z));
                            break;
                    }
                    break;
                case 3:
                    switch (directionValues[1])
                    {
                        case 1:
                            positions.Add(2, new Vector3(_debugVent2.transform.position.x - _debugVent1.transform.position.x, 0, 0));
                            positions.Add(3, new Vector3(0, _debugVent2.transform.position.y - _debugVent1.transform.position.y, 0));
                            positions.Add(1, new Vector3(0, 0, _debugVent2.transform.position.z - _debugVent1.transform.position.z));
                            break;
                        case 2:
                            positions.Add(1, new Vector3(_debugVent2.transform.position.x - _debugVent1.transform.position.x, 0, 0));
                            positions.Add(2, new Vector3(0, _debugVent2.transform.position.y - _debugVent1.transform.position.y, 0));
                            positions.Add(3, new Vector3(0, 0, _debugVent2.transform.position.z - _debugVent1.transform.position.z));
                            break;
                    }
                    break;
            }

            float[] durations = new float[3];
            durations[directionValues[0] - 1] = Mathf.Abs((_debugVent2.transform.position.x - _debugVent1.transform.position.x)) / _soundTravelSpeed;
            durations[directionValues[1] - 1] = Mathf.Abs((_debugVent2.transform.position.y - _debugVent1.transform.position.y)) / _soundTravelSpeed;
            durations[directionValues[2] - 1] = Mathf.Abs((_debugVent2.transform.position.z - _debugVent1.transform.position.z)) / _soundTravelSpeed;

            obj.transform.DOMove(obj.transform.position + positions[directionValues[0]], durations[0]).OnComplete((() =>
            {
                obj.transform.DOMove(obj.transform.position + positions[directionValues[1]], durations[1]).OnComplete(() =>
                {
                    obj.transform.DOMove(obj.transform.position + positions[directionValues[2]], durations[2]).OnComplete((() =>
                    {
                        Destroy(obj, 2);
                    }));
                });
            }));
        }

        #endregion

        #region CLEAR

        [TabGroup("ParentGroup", "Clear Manager", false, 3),
         Button("CLEAR VENT MANAGER", ButtonSizes.Large),
         GUIColor(1, 0, 0)]
        private void ClearAll()
        {
            for (int i = _ventObjectList.Count - 1; i >= 0; i--)
            {
                if (_ventObjectList[i] != null)
                    DestroyImmediate(_ventObjectList[i]);
            }
            ClearVentObjectList();
            CleanUpVentDictionary();
            ResetVentCount();
            if (_ventPathObject != null)
                DestroyImmediate(_ventPathObject);
        }

        #endregion

        #region Level Design

        [BoxGroup("ParentGroup/Level Design Tool/InnerGroup", false),
         Button("Spawn vent object", ButtonSizes.Medium)]
        private void SpawnVentObjectInEditor()
        {
            VentStruct ventStruct = new VentStruct();
            Transform sceneCameraTransform = UnityEditor.EditorWindow.GetWindow<SceneView>().camera.transform;
            ventStruct.VentObject = Instantiate(_ventPrefabObject, sceneCameraTransform.position + sceneCameraTransform.forward * _spawnDistance + Vector3.up * _spawnHeight, Quaternion.identity, this.transform);

            ventStruct.VentObject.name = $"{((_ventName.Length > 0) ? $"{_ventName}" : $"Vent: {_ventCount}")}";

            while (_ventObjectList.Any(o => o.name == ventStruct.VentObject.name))
                ventStruct.VentObject.name += $" {_ventCount}";

            int id = 0;

            while (_ventDictionary.ContainsKey(id))
                id++;

            ventStruct.VentID = id;
            ventStruct.PathDictionary = new PathOrderDictionary();
            _ventDictionary.Add(id, ventStruct);

            foreach (KeyValuePair<int, VentStruct> pair1 in _ventDictionary)
            {
                foreach (KeyValuePair<int, VentStruct> pair2 in _ventDictionary)
                {
                    if (pair1.Key == pair2.Key || (pair1.Value.PathDictionary != null && pair1.Value.PathDictionary.ContainsKey(pair2.Key)))
                        continue;

                    PathOrder order = new PathOrder();
                    order.PathOrderArray = new int[3];
                    order.PathOrderArray[0] = 2;
                    order.PathOrderArray[1] = 3;
                    order.PathOrderArray[2] = 1;

                    pair1.Value.PathDictionary.Add(pair2.Key, order);
                }
            }

            _ventObjectList.Add(ventStruct.VentObject);
            _ventCount++;
        }

        [EnableIf("_previewVentBool"),
         BoxGroup("ParentGroup/Level Design Tool/InnerGroup", false),
         Button("Change Path Order", ButtonSizes.Medium)]
        private void ChangePathOrder()
        {
            if (_ventDictionary == null || _debugVent1 == null || _debugVent2 == null)
            {
                Debug.Log($"Vent Dictionary: {_ventDictionary} Debug Vent 1: {_debugVent1} Debug Vent 2: {_debugVent2}");
                return;
            }

            Vector2Int Ids = GetPreviewVentId1And2();
            if (Ids == Vector2Int.zero)
                return;

            int ventId1 = Ids.x;
            int ventId2 = Ids.y;

            if (_pathDirectionX == _pathDirectionY || _pathDirectionX == _pathDirectionZ || _pathDirectionY == _pathDirectionZ)
            {
                Debug.Log($"Can't set two directions on the same Index\nDirection X: {_pathDirectionX}; Direction Y: {_pathDirectionY}; Direction Z: {_pathDirectionZ}");
                return;
            }

            _ventDictionary[ventId1].PathDictionary[ventId2].PathOrderArray[0] = _pathDirectionX;
            _ventDictionary[ventId1].PathDictionary[ventId2].PathOrderArray[1] = _pathDirectionY;
            _ventDictionary[ventId1].PathDictionary[ventId2].PathOrderArray[2] = _pathDirectionZ;

            if (_automaticMirrorPath)
                MirrorPath();
            if (_automaticDebugPath)
                DebugMoveObjectPath();
        }


        [EnableIf("_previewVentBool"),
         BoxGroup("ParentGroup/Level Design Tool/InnerGroup", false),
         Button("Mirror Path", ButtonSizes.Medium)]
        private void MirrorPath()
        {
            Vector2Int Ids = GetPreviewVentId1And2();
            if (Ids == Vector2Int.zero)
                return;

            int ventId1 = Ids.x;
            int ventId2 = Ids.y;

            int vent1Pos1 = 0;
            int vent1Pos2 = 0;
            int vent1Pos3 = 0;

            for (int i = 0; i < _ventDictionary[ventId1].PathDictionary[ventId2].PathOrderArray.Length; i++)
            {
                switch (_ventDictionary[ventId1].PathDictionary[ventId2].PathOrderArray[i])
                {
                    case 1:
                        vent1Pos1 = i;
                        break;
                    case 2:
                        vent1Pos2 = i;
                        break;
                    default:
                        vent1Pos3 = i;
                        break;
                }
            }

            _ventDictionary[ventId2].PathDictionary[ventId1].PathOrderArray[vent1Pos1] =
                _ventDictionary[ventId1].PathDictionary[ventId2].PathOrderArray[vent1Pos3];
            _ventDictionary[ventId2].PathDictionary[ventId1].PathOrderArray[vent1Pos2] =
                _ventDictionary[ventId1].PathDictionary[ventId2].PathOrderArray[vent1Pos2];
            _ventDictionary[ventId2].PathDictionary[ventId1].PathOrderArray[vent1Pos3] =
                _ventDictionary[ventId1].PathDictionary[ventId2].PathOrderArray[vent1Pos1];
        }

        [BoxGroup("ParentGroup/Level Design Tool/InnerGroup", false),
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

        #endregion

        #region Helper

        private Vector2Int GetDebugVentId1And2()
        {
            int ventId1 = 0;
            int ventId2 = 0;

            if (_ventDictionary.Any(p => p.Value.VentObject == _debugVent1))
            {
                ventId1 = _ventDictionary.Single(p => p.Value.VentObject == _debugVent1).Key;
            }
            else
            {
                Debug.Log("Debug Vent Object 1 == null");
                return Vector2Int.zero;
            }
            if (_ventDictionary.Any(p => p.Value.VentObject == _debugVent2))
            {
                ventId2 = _ventDictionary.Single(p => p.Value.VentObject == _debugVent2).Key;
            }
            else
            {
                Debug.Log("Debug Vent Object 2 == null");
                return Vector2Int.zero;
            }

            return new Vector2Int(ventId1, ventId2);
        }

        private Vector2Int GetPreviewVentId1And2()
        {
            int ventId1 = 0;
            int ventId2 = 0;

            if (_ventDictionary.Any(p => p.Value.VentObject == _previewVent1))
            {
                ventId1 = _ventDictionary.Single(p => p.Value.VentObject == _previewVent1).Key;
            }
            else
            {
                Debug.Log("Debug Vent Object 1 == null");
                return Vector2Int.zero;
            }
            if (_ventDictionary.Any(p => p.Value.VentObject == _previewVent2))
            {
                ventId2 = _ventDictionary.Single(p => p.Value.VentObject == _previewVent2).Key;
            }
            else
            {
                Debug.Log("Debug Vent Object 2 == null");
                return Vector2Int.zero;
            }

            return new Vector2Int(ventId1, ventId2);
        }

        private Dictionary<int, Vector3> GetDirectionDictionary(int[] directionValues)
        {
            if (_ventDictionary == null || _debugVent1 == null || _debugVent2 == null)
            {
                Debug.Log($"Vent Dictionary: {_ventDictionary} Debug Vent 1: {_debugVent1} Debug Vent 2: {_debugVent2}");
                return new Dictionary<int, Vector3>();
            }

            Vector3 posX = Vector3.zero;
            Vector3 posY = Vector3.zero;
            Vector3 posZ = Vector3.zero;

            Dictionary<int, Vector3> positions = new Dictionary<int, Vector3>();

            switch (directionValues[0])
            {
                case 1:
                    posX = new Vector3(_debugVent2.transform.position.x, _debugVent1.transform.position.y, _debugVent1.transform.position.z) - _ventPathObject.transform.position;
                    switch (directionValues[1])
                    {
                        case 2:
                            posY = new Vector3(_debugVent2.transform.position.x, _debugVent2.transform.position.y, _debugVent1.transform.position.z) - _ventPathObject.transform.position;
                            posZ = new Vector3(_debugVent2.transform.position.x, _debugVent2.transform.position.y, _debugVent2.transform.position.z) - _ventPathObject.transform.position;

                            positions.Add(1, posX);
                            positions.Add(2, posY);
                            positions.Add(3, posZ);
                            break;
                        case 3:
                            posY = new Vector3(_debugVent2.transform.position.x, _debugVent2.transform.position.y, _debugVent2.transform.position.z) - _ventPathObject.transform.position;
                            posZ = new Vector3(_debugVent2.transform.position.x, _debugVent1.transform.position.y, _debugVent2.transform.position.z) - _ventPathObject.transform.position;

                            positions.Add(1, posX);
                            positions.Add(2, posY);
                            positions.Add(3, posZ);
                            break;
                    }
                    break;
                case 2:
                    switch (directionValues[1])
                    {
                        case 1:
                            posX = new Vector3(_debugVent2.transform.position.x, _debugVent2.transform.position.y, _debugVent1.transform.position.z) - _ventPathObject.transform.position;
                            posY = new Vector3(_debugVent1.transform.position.x, _debugVent2.transform.position.y, _debugVent1.transform.position.z) - _ventPathObject.transform.position;
                            posZ = new Vector3(_debugVent2.transform.position.x, _debugVent2.transform.position.y, _debugVent2.transform.position.z) - _ventPathObject.transform.position;

                            positions.Add(1, posX);
                            positions.Add(2, posY);
                            positions.Add(3, posZ);
                            break;
                        case 3:
                            posX = new Vector3(_debugVent2.transform.position.x, _debugVent1.transform.position.y, _debugVent2.transform.position.z) - _ventPathObject.transform.position;
                            posY = new Vector3(_debugVent2.transform.position.x, _debugVent2.transform.position.y, _debugVent2.transform.position.z) - _ventPathObject.transform.position;
                            posZ = new Vector3(_debugVent1.transform.position.x, _debugVent1.transform.position.y, _debugVent2.transform.position.z) - _ventPathObject.transform.position;

                            positions.Add(1, posY);
                            positions.Add(2, posZ);
                            positions.Add(3, posX);
                            break;
                    }
                    break;
                case 3:
                    switch (directionValues[1])
                    {
                        case 1:
                            posX = new Vector3(_debugVent2.transform.position.x, _debugVent2.transform.position.y, _debugVent2.transform.position.z) - _ventPathObject.transform.position;
                            posY = new Vector3(_debugVent1.transform.position.x, _debugVent2.transform.position.y, _debugVent1.transform.position.z) - _ventPathObject.transform.position;
                            posZ = new Vector3(_debugVent1.transform.position.x, _debugVent2.transform.position.y, _debugVent2.transform.position.z) - _ventPathObject.transform.position;

                            positions.Add(1, posZ);
                            positions.Add(2, posX);
                            positions.Add(3, posY);
                            break;
                        case 2:
                            posX = new Vector3(_debugVent2.transform.position.x, _debugVent2.transform.position.y, _debugVent2.transform.position.z) - _ventPathObject.transform.position;
                            posY = new Vector3(_debugVent1.transform.position.x, _debugVent2.transform.position.y, _debugVent2.transform.position.z) - _ventPathObject.transform.position;
                            posZ = new Vector3(_debugVent1.transform.position.x, _debugVent1.transform.position.y, _debugVent2.transform.position.z) - _ventPathObject.transform.position;

                            positions.Add(1, posX);
                            positions.Add(2, posY);
                            positions.Add(3, posZ);
                            break;
                    }
                    break;
            }

            return positions;
        }

        private int[] GetDirectionValues(int ventIdStart, int ventIdEnd)
        {
            if (_ventDictionary == null)
            {
                Debug.Log($"Vent Dictionary: {_ventDictionary}");
                return new int[0];
            }

            return _ventDictionary[ventIdStart].PathDictionary[ventIdEnd].PathOrderArray;
        }

        private void ResetVentCount()
        {
            _ventCount = 0;
        }

        private void DestroySelectedVentObject(GameObject ventObject)
        {
            DestroyImmediate(ventObject);
            ClearVentObjectList();
            CleanUpVentDictionary();
        }

        private void SelectVentObject(GameObject selectVent)
        {
            if (selectVent != null)
                UnityEditor.Selection.activeObject = selectVent;
        }

        #endregion

        #endregion
    }

    [Serializable]
    public struct VentStruct
    {
        public GameObject VentObject;
        public int VentID;
        public PathOrderDictionary PathDictionary;
    }
    
    [Serializable]
    public struct PathOrder
    {
        public int[] PathOrderArray;
    }

    [Serializable] public class VentDictionary : SerializableDictionary<int, VentStruct> { }
    [Serializable] public class PathOrderDictionary : SerializableDictionary<int, PathOrder> { }
}