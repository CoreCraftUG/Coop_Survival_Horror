using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using UnityEngine.InputSystem;

namespace CoreCraft.Minigames
{
    public class Turnables : BaseMinigame
    {

        [SerializeField] private List<GameObject> _turnables;       
         private List<float> _correctAngles = new List<float>();
        [SerializeField] private float _range = 1;
        private int _amountOfParts;
       
        private int _activeButton;
        private int _correctCounter;
        private List<bool> _isCorrect = new List<bool>();
        [SerializeField] private Material _completeMaterial;
        [SerializeField] private Material _selectedMaterial;
        [SerializeField] private Material _baseMaterial;
        [SerializeField] private InputActionAsset _inputsActionAsset;
        private InputActionMap _map;
        private InputAction _rotate;
        private NetworkVariable<bool> _canMove = new NetworkVariable<bool>(true);
        protected override void Awake()
        {
           
            _map = _inputsActionAsset.FindActionMap("MinigameMap");

            _map.FindAction("MiniGame1").started += MinigameInput;
            _map.FindAction("MiniGame2").performed += MinigameInput2;
            _rotate = _map.FindAction("MiniGame1");

            base.Awake();
            AwakeServerRpc();

        }

        [ServerRpc(RequireOwnership = false)]
        private void AwakeServerRpc()
        {
            _activeButton = 0;
            foreach (GameObject img in _turnables)
            {
                bool temp = false;
                _correctAngles.Add(img.transform.eulerAngles.x);
                img.transform.eulerAngles = new Vector3(Random.Range(0, 360), 0, 0);
                img.GetComponent<MeshRenderer>().material = _baseMaterial;
                _isCorrect.Add(temp);
            }
            _inputBool.Value = true;
            _correctCounter = 0;
            _amountOfParts = _turnables.Count;
            _turnables[0].GetComponent<MeshRenderer>().material = _selectedMaterial;
            Debug.Log(_correctAngles);
        }

        public void CheckIfCorrect()
        {
            float currentAngle = 0;
                currentAngle = _turnables[_activeButton].transform.eulerAngles.x;
                if(currentAngle > _correctAngles[_activeButton] - _range && currentAngle < _correctAngles[_activeButton] + _range)
                {
                    _turnables[_activeButton].transform.eulerAngles = new Vector3(_correctAngles[_activeButton], 0, 0);
                    _turnables[_activeButton].GetComponent<MeshRenderer>().material = _completeMaterial;
                    _isCorrect[_activeButton] = true;
                }
            _canMove.Value = true;
            _inputBool.Value = false;
        }

        private void OnDestroy()
        {
            _map.FindAction("MiniGame1").performed -= MinigameInput;
            _map.FindAction("MiniGame2").performed -= MinigameInput2;
        }
        public void FixedUpdate()
        {           
            if (IsOwner && !_isCorrect[_activeButton])
            {
                if (_rotate.ReadValue<float>() != 0)
                {
                    _turnables[_activeButton].transform.Rotate(Time.deltaTime * 50 * _rotate.ReadValue<float>(), 0, 0);
                    _canMove.Value = false;
                }
                else
                {
                    CheckIfCorrect();
                                    }              
            }
            
        }

        
        public override void MinigameInput(InputAction.CallbackContext context)
        {
            if (IsOwner)
            {
                //base.MinigameInput(context);
                if (context.phase == InputActionPhase.Started)
                {
                    _inputValue.Value = context.ReadValue<float>();
                    Debug.Log(_inputValue.Value);
                }
                else
                    _inputValue.Value = 0;
            }

        }

        public override void MinigameInput2(InputAction.CallbackContext context)
        {
            if (IsOwner && _canMove.Value)
            {
                base.MinigameInput2(context);
                TurnablesInput2ServerRPC();
            }
        }

        [ServerRpc]

        protected void TurnablesInput2ServerRPC()
        {
            if (!_canMove.Value)
                return;
            _canMove.Value = false;
            if (!_isCorrect[_activeButton])
                _turnables[_activeButton].GetComponent<MeshRenderer>().material = _baseMaterial;
            if (_inputValue2.Value > 0)
            {
                if (_activeButton < _amountOfParts - 1)
                    _activeButton++;
                else
                    _activeButton = 0;
            }
            if(_inputValue2.Value < 0)
            {
                if (_activeButton > 0)
                    _activeButton--;
                else
                    _activeButton = _amountOfParts - 1;
            }
            _inputValue2.Value = 0;
            _canMove.Value = true;
            if (!_isCorrect[_activeButton])
                _turnables[_activeButton].GetComponent<MeshRenderer>().material = _selectedMaterial;
            else
                TurnablesInput2ServerRPC();
        }

        // Start is called before the first frame update

    }
}
