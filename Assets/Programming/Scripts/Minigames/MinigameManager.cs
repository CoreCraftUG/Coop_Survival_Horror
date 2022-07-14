using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CoreCraft.Minigames
{
    public class MinigameManager : NetworkBehaviour
    {
        public NetworkVariable<float>_inputValue = new NetworkVariable<float>();
        public NetworkVariable<bool> _inputBool = new NetworkVariable<bool>();
        public NetworkVariable<float> _inputValue2 = new NetworkVariable<float>();
        public NetworkVariable<bool> _inputBool2 = new NetworkVariable<bool>();
        [SerializeField] private GameObject _hanoi;
        [SerializeField] private GameObject _turnShapes;
        [SerializeField] private GameObject _circleGame;
        protected bool _hanoiActive = false;
        protected bool _turnShapesActive = false;
        protected bool _circleGameActive = true;

        protected virtual void Awake()
        {
            this.transform.GetComponent<NetworkObject>().Spawn();
        }
        public virtual void MinigameInput(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                Debug.Log(context.ReadValue<float>());
                SetInputValue1ServerRpc(context.ReadValue<float>());
            }
            if (context.phase == InputActionPhase.Canceled)
            {
                 SetInputValue1ServerRpc(0);
            }
        }

        public virtual void MinigameInput2(InputAction.CallbackContext context)
        {
            if(context.phase == InputActionPhase.Performed)
            {
                SetInputValue2ServerRpc(context.ReadValue<float>());
            }
            else
            {
                 SetInputValue2ServerRpc(0);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        protected void SetInputValue1ServerRpc(float input)
        {
            _inputValue.Value = input;

            if (input == 0)  
                _inputBool.Value = true;
        }

        [ServerRpc(RequireOwnership = false)]
        protected void SetInputValue2ServerRpc(float input)
        {
            _inputValue2.Value = input;

            if (input == 0)
                _inputBool2.Value = true;
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
