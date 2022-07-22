using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

namespace CoreCraft.Minigames
{
    public class AMultiplayerTest : MinigameManager
    {
        // Start is called before the first frame update
        private Rigidbody _rigidbody;
        protected InputActionMap _map;
        protected InputAction _action1;
        protected InputAction _action2;
        [SerializeField] protected InputActionAsset _inputsActionAsset;
        public bool _complete;
        [SerializeField] protected GameObject _puzzlePiece;
        [SerializeField] protected Transform _pieceStartTransform;
        [SerializeField] private MultiplayerPuzzleTest _testParent;
        [SerializeField] private Material _correctMat;
        protected override void Awake()
        {
            base.Awake();
            _map = _inputsActionAsset.FindActionMap("MinigameMap");
            _map.FindAction("MiniGame1").started += MinigameInput;
            _map.FindAction("MiniGame2").performed += MinigameInput2;
            _action1 = _map.FindAction("MiniGame1");
            AwakeServerRpc();
        }


        // Update is called once per frame
        private void FixedUpdate()
        {
            
            if (IsOwner && _action1.ReadValue<float>() != 0 && !_complete) 
           {             
                float j;
                Debug.Log(_action1.ReadValue<float>());
                j = 5 * Mathf.Abs(_action1.ReadValue<float>()) * Time.deltaTime;
                _puzzlePiece.transform.Translate(Vector3.right * j);
            }
            else if(IsOwner && _action1.ReadValue<float>() == 0 && _puzzlePiece.transform.position.x > _pieceStartTransform.position.x && !_complete)
            {
                _puzzlePiece.transform.Translate(Vector3.right * -1 * Time.deltaTime * 5);
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

        [ServerRpc(RequireOwnership = false)]
        private void AwakeServerRpc()
        {
            _pieceStartTransform = this.transform;
            _complete = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.transform.tag == "MinigameCheck")
            {
                this._complete = true;
                //other.transform.GetComponent<BMultiplayer>()._complete = true;
                _puzzlePiece.GetComponent<Renderer>().material = _correctMat;

            }
        }

    }
}
