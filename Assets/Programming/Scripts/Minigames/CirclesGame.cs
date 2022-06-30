using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoreCraft.Minigames
{
    public class CirclesGame : BaseMinigame
    {
        [SerializeField] private List<GameObject> _circle1;
        [SerializeField] private List<GameObject> _circle2;
        [SerializeField] private List<GameObject> _circle3;
        [SerializeField] private List<GameObject> _circle4;
        [SerializeField] private MinigameManager _minigameManager;
        [SerializeField] private List<int> _correctAngles;
        [SerializeField] private List<GameObject> _activeCircle;
        [SerializeField] private int _gapCount;
        [SerializeField] private int _randomMax;
        [SerializeField] private int _randomMin;
        [SerializeField] private int _rotationSpeed;

        private int _sliceCount;
        
        private int temp;


        private void Awake()
        {
            _activeCircle = _circle1;
            _sliceCount = _activeCircle.Count;
            _correctAngles.Clear();
            while(_correctAngles.Count < _gapCount)
            {
                temp = Random.Range(0, _sliceCount);
                if(_correctAngles.Count > 0)
                {
                    List<int> tempexc = new List<int>();
                    foreach(int x in _correctAngles)
                    {
                        switch (x)
                        {
                            case 15:
                                tempexc.Add(x);
                                tempexc.Add(0);
                                tempexc.Add(x-1);
                                break;
                            case 0:
                                tempexc.Add(x);
                                tempexc.Add(_sliceCount-1);
                                tempexc.Add(x+1);
                                break;
                            default:
                                tempexc.Add(x);
                                tempexc.Add(x + 1);
                                tempexc.Add(x - 1);
                                break;
                        }
                    }
                    if (!tempexc.Contains(temp))
                        _correctAngles.Add(temp);
                }
                else
                 _correctAngles.Add(temp);
            }

            foreach(int obj in _correctAngles)
            {
                _circle1[obj].SetActive(false);
                _circle2[obj].SetActive(false);
                _circle3[obj].SetActive(false);
                _circle4[obj].SetActive(false);
            }

            int i = Random.Range(_randomMin, _randomMax);
            int j = Random.Range(_randomMin, _randomMax);

            foreach(GameObject obj in _circle1)
            {
                obj.transform.Rotate(0, 0, i);
            }
            foreach (GameObject obj in _circle3)
            {
                obj.transform.Rotate(0, 0, j);
            }
        }

        public void FixedUpdate()
        {
            if ( _minigameManager._inputValue != 0 && _activeCircle[0].GetComponent<Image>().color != Color.green)
            {
                foreach(GameObject obj in _activeCircle)
                {
                    obj.transform.Rotate(0, 0, Time.deltaTime * _rotationSpeed * _minigameManager._inputValue);
                }
            }
            if(_minigameManager._inputValue2 != 0)
            {
                if (_activeCircle == _circle1 && _circle3[0].GetComponent<Image>().color != Color.green)
                {
                    _activeCircle = _circle3;
                    foreach (GameObject obj in _activeCircle)
                    {
                        obj.GetComponent<Image>().color = Color.yellow;
                    }
                    foreach (GameObject obj in _circle1)
                    {
                        if (_circle1[0].GetComponent<Image>().color != Color.green)
                            obj.GetComponent<Image>().color = Color.white;
                    }
                }
                else if(_circle1[0].GetComponent<Image>().color != Color.green)
                {
                    _activeCircle = _circle1;
                    foreach (GameObject obj in _activeCircle)
                    {
                        obj.GetComponent<Image>().color = Color.yellow;
                    }
                    foreach (GameObject obj in _circle3)
                    {
                        if(_circle3[0].GetComponent<Image>().color != Color.green)
                            obj.GetComponent<Image>().color = Color.white;
                    }
                    
                }
                _minigameManager._inputValue2 = 0;
            }
            if(_minigameManager._inputValue == 0 && _minigameManager._inputValue2 == 0)
                CheckCorrect();
              

        }

        public void CheckCorrect()
        {
            int counter = 0;
            for(int i = 0; i < _correctAngles.Count; i++)
            {
                if(_activeCircle[i].transform.eulerAngles.z > _circle2[i].transform.eulerAngles.z - 5 && _activeCircle[i].transform.eulerAngles.z < _circle2[i].transform.eulerAngles.z + 5)
                {
                    counter++;
                }
            }

            if(counter >= 3)
            {
                foreach(GameObject obj in _activeCircle)
                {
                    obj.GetComponent<Image>().color = Color.green;
                }
            }
        }
    }
}
