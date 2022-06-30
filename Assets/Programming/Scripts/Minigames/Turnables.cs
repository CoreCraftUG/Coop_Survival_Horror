using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

namespace CoreCraft.Minigames
{
    public class Turnables : BaseMinigame
    {

        [SerializeField] private List<Image> _turnables;
        private float[] angles = { 0, 45, 90, 135, 190, 235, 270, 315 };
        [SerializeField] private float[] _correctAngles;
        [SerializeField] private float _range = 5;
        [SerializeField] private List<Button> _buttons;
        private int _activeButton;
        [SerializeField] private MinigameManager _minigameManager;


        public void Awake()
        {
            foreach(Image img in _turnables)
            {
                img.transform.eulerAngles =  new Vector3(0, 0, Random.Range(0, 360));
            }
            _inputBool.Value = true;
        }

        public void CheckIfCorrect()
        {
            float currentAngle = 0;
            for(int i = 0; i < _turnables.Count; i++)
            {
                currentAngle = _turnables[i].transform.eulerAngles.z;
                if(currentAngle > _correctAngles[i] - _range && currentAngle < _correctAngles[i] + _range)
                {
                    _turnables[i].transform.eulerAngles = new Vector3(0, 0, _correctAngles[i]);
                    _turnables[i].color = Color.green;
                    _buttons[i].enabled = false;
                }
            }
            _inputBool.Value = false;
        }

        public void FixedUpdate()
        {
            Debug.Log(_minigameManager._inputValue);
            if(_minigameManager._inputValue.Value != 0)
                Debug.Log(_minigameManager._inputValue);
            if(_turnables[_activeButton].color != Color.green && _minigameManager._inputValue.Value != 0)
                _turnables[_activeButton].transform.Rotate(0, 0, Time.deltaTime * 40 * _minigameManager._inputValue.Value);
            if (_minigameManager._inputValue.Value == 0)
                CheckIfCorrect();
            else
            {
                _turnables[_activeButton].transform.Rotate(0, 0, 0);
            }
            
        }



        public void Rotate1()
        {
            _activeButton = 0;
           
        }
        public void Rotate2()
        {
            _activeButton = 1;
        }
        public void Rotate3()
        {
            _activeButton = 2;
        }
        public void Rotate4()
        {
            _activeButton = 3;
        }
        // Start is called before the first frame update

    }
}
