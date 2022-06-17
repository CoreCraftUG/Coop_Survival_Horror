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
        [SerializeField] private PhysicsCharacter _physicsCharacter;
        [SerializeField] private PlayerCamera _playerCamera;

        [SerializeField] public float MouseXSensitivity;
        [SerializeField] public float MouseYSensitivity;
        [SerializeField] public bool RunToggle;
        [SerializeField] public bool CrouchToggle;

        private NetworkVariable<Vector3> _networkPosition = new NetworkVariable<Vector3>();

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            SetPositionServerRpc(_physicsCharacter.PlayerObjectAttachmentTransform.position);

            if (IsServer)
            {
                transform.position = _networkPosition.Value;
            }
        }

        public void WalkInput(InputAction.CallbackContext callback)
        {
            Vector2 input = callback.ReadValue<Vector2>();
            Vector3 vector3Input = new Vector3(input.x, 0, input.y);

            _physicsCharacter.RequestMove(vector3Input);
        }
        public void LookInput(InputAction.CallbackContext callback)
        {
            if (callback.started)
            {
                Vector2 input = callback.ReadValue<Vector2>();

                input.y = input.y * MouseXSensitivity * Time.deltaTime;
                input.x = input.x * MouseYSensitivity * Time.deltaTime;

                _physicsCharacter.RequestRotation(input);
                _playerCamera.RequestLookUpRotation(input);
            }
        }

        public void CrouchInput(InputAction.CallbackContext callback)
        {
            if (CrouchToggle && callback.phase == InputActionPhase.Started)
            {
                switch (_physicsCharacter.CharacterState.Value)
                {
                    case ECharacterState.Crouch:
                        _physicsCharacter.RequestStateChange(ECharacterState.Walk);
                        break;
                    default:
                        _physicsCharacter.RequestStateChange(ECharacterState.Crouch);
                        break;
                }

            }
            else if (!CrouchToggle)
            {
                switch (callback.phase)
                {
                    case InputActionPhase.Started:
                        _physicsCharacter.RequestStateChange(ECharacterState.Crouch);
                        break;
                    case InputActionPhase.Canceled:
                        _physicsCharacter.RequestStateChange(ECharacterState.Walk);
                        break;
                }
            }
        }

        public void RunInput(InputAction.CallbackContext callback)
        {
            if (RunToggle && callback.phase == InputActionPhase.Started)
            {
                switch (_physicsCharacter.CharacterState.Value)
                {
                    case ECharacterState.Run:
                        _physicsCharacter.RequestStateChange(ECharacterState.Walk);
                        break;
                    default:
                        _physicsCharacter.RequestStateChange(ECharacterState.Run);
                        break;
                }
                
            }
            else if(!RunToggle)
            {
                switch (callback.phase)
                {
                    case InputActionPhase.Started:
                        _physicsCharacter.RequestStateChange(ECharacterState.Run);
                        break;
                    case InputActionPhase.Canceled:
                        _physicsCharacter.RequestStateChange(ECharacterState.Walk);
                        break;
                }
            }
        }

        [ServerRpc]
        private void SetPositionServerRpc(Vector3 position)
        {
            _networkPosition.Value = position;
        }
    }
}
