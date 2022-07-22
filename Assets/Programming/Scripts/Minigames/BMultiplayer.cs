using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;



namespace CoreCraft.Minigames
{
    public class BMultiplayer : MinigameManager
    {
        protected InputActionMap _map;
        protected InputAction _action1;
        protected InputAction _action2;
        [SerializeField] protected InputActionAsset _inputsActionAsset;
        public bool _complete;
        [SerializeField] protected GameObject _puzzlePiece;
        [SerializeField] protected Transform _pieceStartTransform;
        [SerializeField] private Material _correctMat;

        protected override void Awake()
        {
            base.Awake();
            _map = _inputsActionAsset.FindActionMap("MinigameMap");
            _map.FindAction("MiniGame2").performed += MinigameInput2;
            _action1 = _map.FindAction("MiniGame2");
            AwakeServerRpc();

        }
            //transform.GetComponent<NetworkObject>().OwnerClientId;
            // Update is called once per frame
         void Update()
        {
        
        }
        private void FixedUpdate()
        {
            if (IsOwner && _action1.ReadValue<float>() != 0 && !_complete)
            {

                float j;
                Debug.Log(_action1.ReadValue<float>());
                j = 5 * Mathf.Abs(_action1.ReadValue<float>()) * Time.deltaTime;
                _puzzlePiece.transform.Translate(Vector3.up * j);
                //_rigidbody.MovePosition(_puzzlePiece.transform.position );
            }
            else if (IsOwner && _action1.ReadValue<float>() == 0 && _puzzlePiece.transform.position.y > _pieceStartTransform.position.y && !_complete)
            {
                _puzzlePiece.transform.Translate(Vector3.up * -1 * Time.deltaTime * 5);
            }
        }
        public override void MinigameInput2(InputAction.CallbackContext context)
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
