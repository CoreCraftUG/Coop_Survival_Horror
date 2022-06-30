using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace CoreCraft.Minigames
{
    public class Hanoi : BaseMinigame
    {
        [SerializeField] private List<GameObject> _topSticks;
        [SerializeField] private GameObject _bottomStick;
        [SerializeField] private List<GameObject> _movingPieces;
        [SerializeField] private GameObject _solutionStick;
        public List<GameObject> _correctSolution;       
        private int middleStick;
        [SerializeField] private MinigameManager _manager;
        private bool _puzzleComp = false;
        private Vector3 pos1;
        private Vector3 pos2;
        private Vector3 pos3;

        public void Awake()
        {
            middleStick = 1;
            pos1 = _topSticks[0].transform.position;
            pos2 = _topSticks[1].transform.position;
            pos3 = _topSticks[2].transform.position;

            foreach (GameObject block in _movingPieces)
            {
                int i = Random.Range(0, _topSticks.Count + 1);
                if(i == _topSticks.Count)
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

        public void Update()
        {
            if (_puzzleComp)
                return;
            if(_manager._inputValue > 0)
            {
                foreach (GameObject obj in _topSticks)
                {
                    if (obj.transform.position == pos1)
                    {
                        obj.transform.position = pos2;
                    }
                    else if (obj.transform.position == pos2)
                    {
                        obj.transform.position = pos3;
                    }
                    else if (obj.transform.position == pos3)
                    {
                        obj.transform.position = pos1;
                    }
                }
                DetermineMiddleStick(_manager._inputValue);
                _manager._inputValue = 0;                
            }

            if (_manager._inputValue < 0)
            {               
                foreach (GameObject obj in _topSticks)
                {
                    if (obj.transform.position == pos1)
                    {
                        obj.transform.position = pos3;
                    }
                    else if (obj.transform.position == pos2)
                    {
                        obj.transform.position = pos1;
                    }
                    else if (obj.transform.position == pos3)
                    {
                        obj.transform.position = pos2;
                    }
                    //if (obj.transform.position.x < _bottomStick.transform.position.x)
                    //{
                    //    obj.transform.position = new Vector3(obj.transform.position.x + 700, obj.transform.position.y, 0);
                    //    if (obj.GetComponent<StickScript>().BlockList.Count > 0)
                    //    {
                    //        foreach (GameObject part in obj.GetComponent<StickScript>().BlockList)
                    //        {
                    //            part.transform.position = new Vector3(part.transform.position.x + 700, part.transform.position.y, 0);
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    obj.transform.position = new Vector3(obj.transform.position.x - 350, obj.transform.position.y, 0);
                    //    if (obj.GetComponent<StickScript>().BlockList.Count > 0)
                    //    {
                    //        foreach (GameObject part in obj.GetComponent<StickScript>().BlockList)
                    //        {
                    //            part.transform.position = new Vector3(part.transform.position.x - 350, part.transform.position.y, 0);
                    //        }
                    //    }
                    //}

                }
                DetermineMiddleStick(_manager._inputValue);
                _manager._inputValue = 0;               
            }

            if (_manager._inputValue2 < 0)
            {
                if (_topSticks[middleStick].GetComponent<StickScript>().BlockList.Count <= 0)
                    return;
                else
                {
                    //_bottomStick.GetComponent<StickScript>().BlockList.Add(_topSticks[middleStick].GetComponent<StickScript>().BlockList[0]);

                    //_topSticks[middleStick].GetComponent<StickScript>().BlockList[0].transform.position = new Vector3(_bottomStick.transform.position.x, (_bottomStick.transform.position.y - _stickYD) + (_blockY * _bottomStick.GetComponent<StickScript>().BlockList.Count), 0);
                    //_topSticks[middleStick].GetComponent<StickScript>().BlockList.RemoveAt(0);
                    //foreach (GameObject obj in _topSticks[middleStick].GetComponent<StickScript>().BlockList)
                    //{
                    //    obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y - _blockY, 0);
                    //}
                    _topSticks[middleStick].GetComponent<StickScript>().BlockList[0].transform.position = _bottomStick.GetComponent<StickScript>().Slots[_bottomStick.GetComponent<StickScript>().BlockList.Count].transform.position;
                    _bottomStick.GetComponent<StickScript>().BlockList.Add(_topSticks[middleStick].GetComponent<StickScript>().BlockList[0]);
                    _topSticks[middleStick].GetComponent<StickScript>().BlockList.RemoveAt(0);
                    for(int i = 0; i < _topSticks[middleStick].GetComponent<StickScript>().BlockList.Count; i++)
                    {
                        _topSticks[middleStick].GetComponent<StickScript>().BlockList[i].transform.position = _topSticks[middleStick].GetComponent<StickScript>().Slots[i].transform.position;
                    }

                }
                CheckTotal();
                _manager._inputValue2 = 0;              
            }

            if (_manager._inputValue2 > 0)
            {
                if (_bottomStick.GetComponent<StickScript>().BlockList.Count <= 0)
                    return;

                else
                {
                    if(_topSticks[middleStick].GetComponent<StickScript>().BlockList.Count > 0)
                    {
                        _topSticks[middleStick].GetComponent<StickScript>().BlockList.Add(null);
                        for(int i = _topSticks[middleStick].GetComponent<StickScript>().BlockList.Count - 1; i > 0; i--)
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
                    for(int i = 0; i < _topSticks[middleStick].GetComponent<StickScript>().BlockList.Count; i++)
                    {
                        _topSticks[middleStick].GetComponent<StickScript>().BlockList[i].transform.position = _topSticks[middleStick].GetComponent<StickScript>().Slots[i].transform.position;
                    }
                    //_topSticks[middleStick].GetComponent<StickScript>().BlockList.Add(_bottomStick.GetComponent<StickScript>().BlockList[_bottomStick.GetComponent<StickScript>().BlockList.Count - 1]);
                    //GameObject temp = _topSticks[middleStick].GetComponent<StickScript>().BlockList[_topSticks[middleStick].GetComponent<StickScript>().BlockList.Count - 1];
                    //for(int i = _topSticks[middleStick].GetComponent<StickScript>().BlockList.Count - 1; i >= 1; i--)
                    //{
                    //    _topSticks[middleStick].GetComponent<StickScript>().BlockList[i] = _topSticks[middleStick].GetComponent<StickScript>().BlockList[i - 1];
                    //}
                    //_topSticks[middleStick].GetComponent<StickScript>().BlockList[0] = temp;
                    //for (int i = 0; i <= _topSticks[middleStick].GetComponent<StickScript>().BlockList.Count - 1; i++)
                    //{
                    //    _topSticks[middleStick].GetComponent<StickScript>().BlockList[i].transform.position = new Vector3(_topSticks[middleStick].transform.position.x, (_topSticks[middleStick].transform.position.y - _stickYD) + _blockY * i, 0);
                    //}
                    //_bottomStick.GetComponent<StickScript>().BlockList.Remove(temp);


                }
                _manager._inputValue2 = 0;               
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
                    obj.GetComponent<Image>().color = Color.green;
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
                GameObject x = Instantiate(_correctSolution[j]);
                x.transform.position = _solutionStick.GetComponent<StickScript>().Slots[j].transform.position;
            }

        }

    }
}
