using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

namespace CoreCraft.Minigames
{
    public class Turnables : MinigameManager
    {

        [SerializeField] private List<Image> _turnables;
        private float[] angles = { 0, 45, 90, 135, 190, 235, 270, 315 };
        [SerializeField] private float[] _correctAngles = { 0, 180, 90, 90 };
        [SerializeField] private float _range = 5;
        [SerializeField] private List<Button> _buttons;
        private int _activeButton;
        
        [SerializeField] private GameObject _canvas;
        [SerializeField] private bool _canvasActive = true;

        public void Awake()
        {
            foreach(Image img in _turnables)
            {
                img.transform.eulerAngles =  new Vector3(0, 0, Random.Range(0, 360));
            }
           
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
            _inputBool = false;
        }

        public void FixedUpdate()
        {          
            if(_turnables[_activeButton].color != Color.green)
                _turnables[_activeButton].transform.Rotate(0, 0, Time.deltaTime * 40 * _inputValue);
            if (_inputValue == 0 && _inputBool)
                CheckIfCorrect();
        }

        public void HideMinigame()
        {
            _canvasActive = !_canvasActive;
            _canvas.SetActive(_canvasActive);
        }

        public void RotationAction(InputAction.CallbackContext context)
        {
            if(context.phase == InputActionPhase.Performed)
            {
        
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
