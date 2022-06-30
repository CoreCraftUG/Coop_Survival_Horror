using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace CoreCraft.Character
{
    public class PlayerCamera : NetworkBehaviour
    {
        [SerializeField] private bool _lookConstrained = true;
        [SerializeField] private GameObject _cameraObject;


        private NetworkVariable<float> _xRotation = new NetworkVariable<float>();
        private NetworkVariable<float> _yRotation = new NetworkVariable<float>();

        void Start()
        {
            _yRotation.Value = transform.rotation.y;
            _xRotation.Value = transform.rotation.x;
            if (!IsOwner)
            {
                gameObject.GetComponentInChildren<Camera>().enabled = false;
                gameObject.GetComponentInChildren<AudioListener>().enabled = false;
            }
        }

        void Update()
        {
            if (!IsServer)
                return;
            
            transform.rotation = Quaternion.Euler(_xRotation.Value, _yRotation.Value,0.0f);
        }

        public void RequestLookUpRotation(Vector2 rotationInput)
        {
            RotateRequestServerRpc(rotationInput);
        }

        [ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Unreliable)]
        private void RotateRequestServerRpc(Vector2 rotation)
        {
            _yRotation.Value += rotation.x;
            _xRotation.Value -= rotation.y;

            if (_lookConstrained)
                _xRotation.Value = Mathf.Clamp(_xRotation.Value, -90.0f, 90.0f);
        }

        public void LockCursor(bool lockState)
        {
            Cursor.lockState = lockState switch
            {
                true => CursorLockMode.Locked,
                false => CursorLockMode.None
            };

            Cursor.visible = !lockState;
        }
    }
}
