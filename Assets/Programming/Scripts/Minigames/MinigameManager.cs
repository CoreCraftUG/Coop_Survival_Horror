using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CoreCraft.Minigames
{
    public class MinigameManager : MonoBehaviour
    {
        public float _inputValue;
        public bool _inputBool;
        public float _inputValue2;
        public bool _inputBool2;
        [SerializeField] private GameObject _hanoi;
        [SerializeField] private GameObject _turnShapes;
        [SerializeField] private GameObject _circleGame;
        private bool _hanoiActive = false;
        private bool _turnShapesActive = false;
        private bool _circleGameActive = true;
        // Start is called before the first frame update
        public void MinigameInput(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                _inputValue = context.ReadValue<float>();
                _inputBool = true;
            }
            else
            {
                _inputValue = 0;
            }
        }

        public void MinigameInput2(InputAction.CallbackContext context)
        {
            if(context.phase == InputActionPhase.Performed)
            {
                _inputValue2 = context.ReadValue<float>();
                _inputBool2 = true;
            }
            else
            {
                _inputValue2 = 0;
            }
        }

        public void HanoiSwitch()
        {
            switch (_hanoiActive)
            {
                case true:
                    _hanoi.SetActive(false);
                    break;
                case false:
                    _hanoi.SetActive(true);
                    break;
            }

            _hanoiActive = !_hanoiActive;
            _turnShapesActive = false;
            _turnShapes.SetActive(false);
            _circleGameActive = false;
            _circleGame.SetActive(false);
        }

        public void TurnShapesSwitch()
        {
            switch (_turnShapesActive)
            {
                case true:
                    _turnShapes.SetActive(false);
                    break;
                case false:
                    _turnShapes.SetActive(true);
                    break;
            }

            _turnShapesActive = !_turnShapesActive;
            _circleGameActive = false;
            _circleGame.SetActive(false);
            _hanoiActive = false;
            _hanoi.SetActive(false);
        }

        public void CircleGameSwitch()
        {
            switch (_circleGameActive)
            {
                case true:
                    _circleGame.SetActive(false);
                    break;
                case false:
                    _circleGame.SetActive(true);
                    break;
            }

            _circleGameActive = !_circleGameActive;
            _turnShapesActive = false;
            _turnShapes.SetActive(false);
            _hanoiActive = false;
            _hanoi.SetActive(false);
        }
    }
}
