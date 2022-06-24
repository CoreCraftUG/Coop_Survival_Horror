using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Logger = CoreCraft.Core.Logger;

namespace CoreCraft.Character
{
    public class PlayerController : NetworkBehaviour
    {
        private PhysicsCharacter _physicsCharacter;
        public PlayerCamera PlayerCamera { get; private set; }

        [SerializeField] public float MouseXSensitivity;
        [SerializeField] public float MouseYSensitivity;
        [SerializeField] public bool RunToggle;
        [SerializeField] public bool CrouchToggle;

        private PlayerInput _playerInput;
        private NetworkObject _networkObject;
        private NetworkVariable<Vector3> _networkPosition = new NetworkVariable<Vector3>();

        private bool _hasPlayer = true;

        private void Awake()
        {
            _networkObject = GetComponent<NetworkObject>();
            _playerInput = GetComponent<PlayerInput>();

            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            SetPositionServerRpc(_physicsCharacter.PlayerObjectAttachmentTransform.position);

            if (!IsServer) return;

            // if (_hasPlayer)
            PlayerCamera.transform.gameObject.transform.position = _networkPosition.Value;

            transform.position = _networkPosition.Value;
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetCameraServerRpc(ulong cameraNetworkId)
        {
            GameObject obj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[cameraNetworkId].transform.gameObject;

            PlayerCamera = obj.GetComponent<PlayerCamera>();
            SetCameraClientRpc(cameraNetworkId);
        }

        [ClientRpc]
        public void SetCameraClientRpc(ulong cameraNetworkId)
        {
            GameObject obj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[cameraNetworkId].transform.gameObject;

            PlayerCamera = obj.GetComponent<PlayerCamera>();
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetPhysicsCharacterServerRpc(ulong characterNetworkId)
        {
            GameObject obj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[characterNetworkId]
                .transform.gameObject;

            _physicsCharacter = obj.GetComponent<PhysicsCharacter>();
            SetPhysicsCharacterClientRpc(characterNetworkId);
        }

        [ClientRpc]
        public void SetPhysicsCharacterClientRpc(ulong characterNetworkId)
        {
            GameObject obj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[characterNetworkId]
                .transform.gameObject;

            _physicsCharacter = obj.GetComponent<PhysicsCharacter>();
        }

        public void SetHasPlayer(bool state) => _hasPlayer = state;

        public void WalkInput(InputAction.CallbackContext callback)
        {
            if (IsOwner)
            {
                Vector2 input = callback.ReadValue<Vector2>();
                Vector3 vector3Input = new Vector3(input.x, 0, input.y);

                _physicsCharacter.RequestMove(vector3Input);
            }
        }

        public void LookInput(InputAction.CallbackContext callback)
        {
            if (IsOwner && callback.started)
            {
                Vector2 input = callback.ReadValue<Vector2>();

                input.y = input.y * MouseXSensitivity * Time.deltaTime;
                input.x = input.x * MouseYSensitivity * Time.deltaTime;

                _physicsCharacter.RequestRotation(input);
                PlayerCamera.RequestLookUpRotation(input);
            }
        }

        public void CrouchInput(InputAction.CallbackContext callback)
        {
            if (IsOwner)
            {
                switch (CrouchToggle)
                {
                    case true when callback.phase == InputActionPhase.Started:
                        switch (_physicsCharacter.CharacterState.Value)
                        {
                            case ECharacterState.Crouch:
                                _physicsCharacter.RequestStateChange(ECharacterState.Walk);
                                break;
                            default:
                                _physicsCharacter.RequestStateChange(ECharacterState.Crouch);
                                break;
                        }

                        break;
                    case false:
                        switch (callback.phase)
                        {
                            case InputActionPhase.Started:
                                _physicsCharacter.RequestStateChange(ECharacterState.Crouch);
                                break;
                            case InputActionPhase.Canceled:
                                _physicsCharacter.RequestStateChange(ECharacterState.Walk);
                                break;
                        }

                        break;
                }
            }
        }

        public void RunInput(InputAction.CallbackContext callback)
        {
            if (IsOwner)
            {
                switch (RunToggle)
                {
                    case true when callback.phase == InputActionPhase.Started:
                        switch (_physicsCharacter.CharacterState.Value)
                        {
                            case ECharacterState.Run:
                                _physicsCharacter.RequestStateChange(ECharacterState.Walk);
                                break;
                            default:
                                _physicsCharacter.RequestStateChange(ECharacterState.Run);
                                break;
                        }

                        break;
                    case false:
                        switch (callback.phase)
                        {
                            case InputActionPhase.Started:
                                _physicsCharacter.RequestStateChange(ECharacterState.Run);
                                break;
                            case InputActionPhase.Canceled:
                                _physicsCharacter.RequestStateChange(ECharacterState.Walk);
                                break;
                        }

                        break;
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetPositionServerRpc(Vector3 position)
        {
            _networkPosition.Value = position;
        }
    }
}