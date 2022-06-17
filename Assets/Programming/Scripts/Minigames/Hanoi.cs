using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace CoreCraft.Minigames
{
    public class Hanoi : MinigameManager
    {
        [SerializeField]private List<GameObject> _topSticks;
        [SerializeField]private GameObject _bottomStick;
        [SerializeField]private List<GameObject> _movingPieces;
        [SerializeField] private float _blockY;
        public List<GameObject> _correctSolution;
        [SerializeField] private float _stickY = 450;
        private int middleStick;
        [SerializeField] private MinigameManager _manager;
        [SerializeField] private float _solX;
        [SerializeField] private float _solY;
        [SerializeField] private GameObject _canvas;
        [SerializeField] private float _stickYD;
        private bool _puzzleComp = false;

        public void Awake()
        {
            middleStick = 1;
            
           
            foreach(GameObject block in _movingPieces)
            {
                int i = Random.Range(0, _topSticks.Count + 1);
                if(i == _topSticks.Count)
                {
                    block.transform.position = new Vector3(_bottomStick.transform.position.x,(_bottomStick.transform.position.y - _stickYD) + (_bottomStick.GetComponent<StickScript>().BlockList.Count * _blockY), 0);
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
                    if (obj.transform.position.x > _bottomStick.transform.position.x)
                    {
                        obj.transform.position = new Vector3(obj.transform.position.x - 700, obj.transform.position.y, 0);
                        if (obj.GetComponent<StickScript>().BlockList.Count > 0)
                        {
                            foreach (GameObject part in obj.GetComponent<StickScript>().BlockList)
                            {
                                part.transform.position = new Vector3(part.transform.position.x - 700, part.transform.position.y, 0);
                            }
                        }
                    }
                    else
                    {
                        obj.transform.position = new Vector3( obj.transform.position.x + 350, obj.transform.position.y, 0);
                        if (obj.GetComponent<StickScript>().BlockList.Count > 0)
                        {
                            foreach (GameObject part in obj.GetComponent<StickScript>().BlockList)
                            {
                                part.transform.position = new Vector3(part.transform.position.x + 350, part.transform.position.y, 0);
                            }
                        }
                    }

                }
                DetermineMiddleStick(_manager._inputValue);
                _manager._inputValue = 0;
                

            }

            if (_manager._inputValue < 0)
            {               
                foreach (GameObject obj in _topSticks)
                {
                    if (obj.transform.position.x < _bottomStick.transform.position.x)
                    {
                        obj.transform.position = new Vector3(obj.transform.position.x + 700, obj.transform.position.y, 0);
                        if (obj.GetComponent<StickScript>().BlockList.Count > 0)
                        {
                            foreach (GameObject part in obj.GetComponent<StickScript>().BlockList)
                            {
                                part.transform.position = new Vector3(part.transform.position.x + 700, part.transform.position.y, 0);
                            }
                        }
                    }
                    else
                    {
                        obj.transform.position = new Vector3(obj.transform.position.x - 350, obj.transform.position.y, 0);
                        if (obj.GetComponent<StickScript>().BlockList.Count > 0)
                        {
                            foreach (GameObject part in obj.GetComponent<StickScript>().BlockList)
                            {
                                part.transform.position = new Vector3(part.transform.position.x - 350, part.transform.position.y, 0);
                            }
                        }
                    }

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
                    _bottomStick.GetComponent<StickScript>().BlockList.Add(_topSticks[middleStick].GetComponent<StickScript>().BlockList[0]);

                    _topSticks[middleStick].GetComponent<StickScript>().BlockList[0].transform.position = new Vector3(_bottomStick.transform.position.x, (_bottomStick.transform.position.y - _stickYD) + (_blockY * _bottomStick.GetComponent<StickScript>().BlockList.Count), 0);
                    _topSticks[middleStick].GetComponent<StickScript>().BlockList.RemoveAt(0);
                    foreach (GameObject obj in _topSticks[middleStick].GetComponent<StickScript>().BlockList)
                    {
                        obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y - _blockY, 0);
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
                    _topSticks[middleStick].GetComponent<StickScript>().BlockList.Add(_bottomStick.GetComponent<StickScript>().BlockList[_bottomStick.GetComponent<StickScript>().BlockList.Count - 1]);
                    GameObject temp = _topSticks[middleStick].GetComponent<StickScript>().BlockList[_topSticks[middleStick].GetComponent<StickScript>().BlockList.Count - 1];
                    for(int i = _topSticks[middleStick].GetComponent<StickScript>().BlockList.Count - 1; i >= 1; i--)
                    {
                        _topSticks[middleStick].GetComponent<StickScript>().BlockList[i] = _topSticks[middleStick].GetComponent<StickScript>().BlockList[i - 1];
                    }
                    _topSticks[middleStick].GetComponent<StickScript>().BlockList[0] = temp;
                    for (int i = 0; i <= _topSticks[middleStick].GetComponent<StickScript>().BlockList.Count - 1; i++)
                    {
                        _topSticks[middleStick].GetComponent<StickScript>().BlockList[i].transform.position = new Vector3(_topSticks[middleStick].transform.position.x, (_topSticks[middleStick].transform.position.y - _stickYD) + _blockY * i, 0);
                    }
                    _bottomStick.GetComponent<StickScript>().BlockList.Remove(temp);
                    

                }
                _manager._inputValue2 = 0;
                
            }
        }

        public void AddBlock(GameObject Stick, GameObject Block)
        {

            StickScript stickscript = Stick.GetComponent<StickScript>();
            stickscript.BlockList.Add(Block);
            if (stickscript.BlockList.Count == 0)
                Block.transform.position = new Vector3(Stick.transform.position.x, Stick.transform.position.y - _stickYD, Stick.transform.position.z);
            else
                Block.transform.position = new Vector3(Stick.transform.position.x, Stick.transform.position.y - _stickYD + _blockY * stickscript.BlockList.Count, Block.transform.position.z);

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

            Debug.Log(middleStick);
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
                GameObject x = Instantiate(_correctSolution[j], _canvas.transform);
                x.transform.position = new Vector3(_solX, _solY + (_blockY * j), 0);
            }

        }

    }
}
