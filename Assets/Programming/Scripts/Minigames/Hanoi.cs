using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


namespace CoreCraft.Minigames
{
    public class Hanoi : BaseMinigame
    {
        [SerializeField] private List<GameObject> _topSticks;
        [SerializeField] private GameObject _bottomStick;
        [SerializeField] private List<GameObject> _movingPieces;
        [SerializeField] private GameObject _solutionStick;
        [SerializeField] private InputActionAsset _inputsActionAsset;
        [SerializeField] private Material _completeMaterial;

        public List<GameObject> _correctSolution;       
        private int middleStick;
        private bool _puzzleComp = false;
        private InputActionMap _map;

        private NetworkVariable<Vector3> pos1 = new NetworkVariable<Vector3>();
        private NetworkVariable<Vector3> pos2 = new NetworkVariable<Vector3>();
        private NetworkVariable<Vector3> pos3 = new NetworkVariable<Vector3>();
        private NetworkVariable<bool> _canMove = new NetworkVariable<bool>(true);

        public void Awake()
        {
            _map = _inputsActionAsset.FindActionMap("MinigameMap");

            _map.FindAction("MiniGame1").performed += MinigameInput;
            _map.FindAction("MiniGame2").performed += MinigameInput2;


            this.transform.GetComponent<NetworkObject>().Spawn();
            AwakeServerRpc();
        }

        private void Start()
        {
        }

        [ServerRpc(RequireOwnership = false)]
        private void AwakeServerRpc()
        {

            middleStick = 1;

            pos1.Value = _topSticks[0].transform.position;
            pos2.Value = _topSticks[1].transform.position;
            pos3.Value = _topSticks[2].transform.position;

            foreach (GameObject block in _movingPieces)
            {
                int i = Random.Range(0, _topSticks.Count + 1);
                if (i == _topSticks.Count)
                {
                    block.transform.position = _bottomStick.GetComponent<StickScript>().Slots[_bottomStick.GetComponent<StickScript>().BlockList.Count].transform.position;
                    _bottomStick.GetComponent<StickScript>().BlockList.Add(block);
                }
                else
                {
                    AddBlock(_topSticks[i], block);
                }
            }
            makeSolution();
        }

        private void OnDestroy()
        {
            _map.FindAction("MiniGame1").performed -= MinigameInput;
            _map.FindAction("MiniGame2").performed -= MinigameInput2;
        }

        public override void MinigameInput(InputAction.CallbackContext context)
        {
            if (IsOwner && !_puzzleComp)
            {
                base.MinigameInput(context);

                HanoiInput1ServerRpc();
            }
        }

        [ServerRpc]
        protected void HanoiInput1ServerRpc()
        {
            if (!_canMove.Value)
                return;

            if (_inputValue.Value > 0)
            {
                _canMove.Value = false;
                foreach (GameObject obj in _topSticks)
                {
                    if (obj.transform.position == pos1.Value)
                    {
                        obj.transform.DOMove(pos2.Value,0.25f).OnComplete((() =>
                        {
                            _canMove.Value = true;
                        }));
                        foreach (GameObject childObj in obj.GetComponent<StickScript>().BlockList)
                        {
                            childObj.transform.DOMove(new Vector3(pos2.Value.x, childObj.transform.position.y, pos2.Value.z), 0.25f);
                        }
                    }
                    else if (obj.transform.position == pos2.Value)
                    {
                        obj.transform.DOMove(pos3.Value, 0.25f).OnComplete((() =>
                        {
                            _canMove.Value = true;
                        }));
                        foreach (GameObject childObj in obj.GetComponent<StickScript>().BlockList)
                        {
                            childObj.transform.DOMove(new Vector3(pos3.Value.x, childObj.transform.position.y, pos3.Value.z), 0.25f);
                        }
                    }
                    else if (obj.transform.position == pos3.Value)
                    {
                        obj.transform.DOMove(pos1.Value, 0.25f).OnComplete((() =>
                        {
                            _canMove.Value = true;
                        }));
                        foreach (GameObject childObj in obj.GetComponent<StickScript>().BlockList)
                        {
                            childObj.transform.DOMove(new Vector3(pos1.Value.x, childObj.transform.position.y, pos1.Value.z), 0.25f);
                        }
                    }
                }
                DetermineMiddleStick(_inputValue.Value);
                _inputValue.Value = 0;
            }

            if (_inputValue.Value < 0)
            {
                _canMove.Value = false;
                foreach (GameObject obj in _topSticks)
                {
                    if (obj.transform.position == pos1.Value)
                    {
                        obj.transform.DOMove(pos3.Value, 0.25f).OnComplete((() =>
                        {
                            _canMove.Value = true;
                        }));
                        foreach (GameObject childObj in obj.GetComponent<StickScript>().BlockList)
                        {
                            childObj.transform.DOMove(new Vector3(pos3.Value.x, childObj.transform.position.y, pos3.Value.z), 0.25f);
                        }
                    }
                    else if (obj.transform.position == pos2.Value)
                    {
                        obj.transform.DOMove(pos1.Value, 0.25f).OnComplete((() =>
                        {
                            _canMove.Value = true;
                        }));
                        foreach (GameObject childObj in obj.GetComponent<StickScript>().BlockList)
                        {
                            childObj.transform.DOMove(new Vector3(pos1.Value.x, childObj.transform.position.y, pos1.Value.z), 0.25f);
                        }
                    }
                    else if (obj.transform.position == pos3.Value)
                    {
                        obj.transform.DOMove(pos2.Value, 0.25f).OnComplete((() =>
                        {
                            _canMove.Value = true;
                        }));
                        foreach (GameObject childObj in obj.GetComponent<StickScript>().BlockList)
                        {
                            childObj.transform.DOMove(new Vector3(pos2.Value.x, childObj.transform.position.y, pos2.Value.z), 0.25f);
                        }
                    }
                }
                DetermineMiddleStick(_inputValue.Value);
                _inputValue.Value = 0;
            }
        }


        public override void MinigameInput2(InputAction.CallbackContext context)
        {
            if (IsOwner && !_puzzleComp)
            {
                base.MinigameInput2(context);

                HanoiInput2ServerRpc();
            }
        }

        [ServerRpc]
        private void HanoiInput2ServerRpc()
        {
            if (!_canMove.Value)
                return;

            if (_inputValue2.Value < 0)
            {
                if (_topSticks[middleStick].GetComponent<StickScript>().BlockList.Count <= 0)
                    return;
                else
                {
                    _topSticks[middleStick].GetComponent<StickScript>().BlockList[0].transform.position = _bottomStick.GetComponent<StickScript>().Slots[_bottomStick.GetComponent<StickScript>().BlockList.Count].transform.position;
                    _bottomStick.GetComponent<StickScript>().BlockList.Add(_topSticks[middleStick].GetComponent<StickScript>().BlockList[0]);
                    _topSticks[middleStick].GetComponent<StickScript>().BlockList.RemoveAt(0);
                    for (int i = 0; i < _topSticks[middleStick].GetComponent<StickScript>().BlockList.Count; i++)
                    {
                        _topSticks[middleStick].GetComponent<StickScript>().BlockList[i].transform.position = _topSticks[middleStick].GetComponent<StickScript>().Slots[i].transform.position;
                    }

                }
                CheckTotal();
                _inputValue2.Value = 0;
            }

            if (_inputValue2.Value > 0)
            {
                if (_bottomStick.GetComponent<StickScript>().BlockList.Count <= 0)
                    return;

                else
                {
                    if (_topSticks[middleStick].GetComponent<StickScript>().BlockList.Count > 0)
                    {
                        _topSticks[middleStick].GetComponent<StickScript>().BlockList.Add(null);
                        for (int i = _topSticks[middleStick].GetComponent<StickScript>().BlockList.Count - 1; i > 0; i--)
                        {
                            _topSticks[middleStick].GetComponent<StickScript>().BlockList[i] = _topSticks[middleStick].GetComponent<StickScript>().BlockList[i - 1];
                        }
                        _topSticks[middleStick].GetComponent<StickScript>().BlockList[0] = _bottomStick.GetComponent<StickScript>().BlockList[_bottomStick.GetComponent<StickScript>().BlockList.Count - 1];
                    }
                    else
                    {
                        _topSticks[middleStick].GetComponent<StickScript>().BlockList.Add(_bottomStick.GetComponent<StickScript>().BlockList[_bottomStick.GetComponent<StickScript>().BlockList.Count - 1]);
                    }

                    _bottomStick.GetComponent<StickScript>().BlockList.RemoveAt(_bottomStick.GetComponent<StickScript>().BlockList.Count - 1);
                    for (int i = 0; i < _topSticks[middleStick].GetComponent<StickScript>().BlockList.Count; i++)
                    {
                        _topSticks[middleStick].GetComponent<StickScript>().BlockList[i].transform.position = _topSticks[middleStick].GetComponent<StickScript>().Slots[i].transform.position;
                    }


                }
                _inputValue2.Value = 0;
            }
        }

        public void AddBlock(GameObject Stick, GameObject Block)
        {
            StickScript stickscript = Stick.GetComponent<StickScript>();
            Block.transform.position = stickscript.Slots[stickscript.BlockList.Count].transform.position;
            stickscript.BlockList.Add(Block);
        }

        public void CheckTotal()
        {
            if (_correctSolution.Count == _bottomStick.GetComponent<StickScript>().BlockList.Count)
            {
                for (int i = 0; i < _correctSolution.Count; i++)
                {
                    if (_correctSolution[i] != _bottomStick.GetComponent<StickScript>().BlockList[i])
                        return;
                }
                foreach (GameObject obj in _bottomStick.GetComponent<StickScript>().BlockList)
                {
                    obj.GetComponent<MeshRenderer>().material = _completeMaterial;
                }
                _puzzleComp = true;
            }
        }

        public void DetermineMiddleStick(float i)
        {
            if (i > 0)
            {
                switch (middleStick)
                {
                    case 0:
                        middleStick = 2;
                        break;
                    case 1:
                        middleStick = 0;
                        break;
                    case 2:
                        middleStick = 1;
                        break;
                }
            }
            else if(i < 0)
            {

                switch (middleStick)
                {
                    case 0:
                        middleStick = 1;
                        break;
                    case 1:
                        middleStick = 2;
                        break;
                    case 2:
                        middleStick = 0;
                        break;
                }
            }
        }
        public void makeSolution()
        {
            List<GameObject> tempSolution = new List<GameObject>();
            for (int k = 0; k < _movingPieces.Count; k++)
            {
                tempSolution.Add(_movingPieces[k]);
            }
            _correctSolution.Clear();
            int i = 0;
            while (tempSolution.Count > 0)
            {
                i = Random.Range(0, tempSolution.Count);
                _correctSolution.Add(tempSolution[i]);
                tempSolution.RemoveAt(i);
            }

            for (int j = 0; j < _correctSolution.Count; j++)
            {
                GameObject x = Instantiate(_correctSolution[j],transform);
                x.transform.position = _solutionStick.GetComponent<StickScript>().Slots[j].transform.position;
            }

        }

    }
}
