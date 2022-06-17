using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace CoreCraft.Character
{
    public class PlayerCamera : NetworkBehaviour
    {
        [SerializeField] private bool _lookConstrained = true;
        [SerializeField] private Transform _cameraPositionTransform;

        private bool _hasPlayer = true;

        private NetworkVariable<float> _xRotation = new NetworkVariable<float>();
        private NetworkVariable<float> _yRotation = new NetworkVariable<float>();
        void Start()
        {
            _yRotation.Value = transform.rotation.y;
            _xRotation.Value = transform.rotation.x;
        }

        void Update()
        {
            Debug.DrawLine(transform.position, transform.position + (transform.forward * 2), Color.red);
            if (_hasPlayer)
                transform.position = _cameraPositionTransform.position;

            if (IsServer)
            {
                transform.rotation = Quaternion.Euler(_xRotation.Value, _yRotation.Value,0.0f);
            }
        }

        public void RequestLookUpRotation(Vector2 rotationInput)
        {
            RotateRequestServerRpc(rotationInput);
        }

        [ServerRpc]
        private void RotateRequestServerRpc(Vector2 rotation)
        {
            _yRotation.Value += rotation.x;
            _xRotation.Value -= rotation.y;

            if (_lookConstrained)
                _xRotation.Value = Mathf.Clamp(_xRotation.Value, -90.0f, 90.0f);
        }

        public void SetHasPlayer(bool state) => _hasPlayer = state;

        public void SetCameraPositionTransform(Transform position) => _cameraPositionTransform = position;

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