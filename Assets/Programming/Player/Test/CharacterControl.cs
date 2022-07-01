using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CoreCraft.Character
{
    public class CharacterControl : MonoBehaviour
    {
        [SerializeField] private CharacterPhysics _characterPhysics;
        [SerializeField] private CharacterCamera _characterCamera;

        [SerializeField] public float MouseXSensitivity;
        [SerializeField] public float MouseYSensitivity;
        [SerializeField] public bool RunToggle;
        [SerializeField] public bool CrouchToggle;

        private Vector3 _cameraPosition;


        void Start()
        {
        }
        
        void Update()
        {
            _cameraPosition = _characterPhysics.CameraHolderTransform.position;

            _characterCamera.transform.gameObject.transform.position = _cameraPosition;
            transform.position = _cameraPosition;
        }

        public void WalkInput(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();
            Vector3 vector3Input = new Vector3(input.y, 0, input.x);

            _characterPhysics.RequestMove(vector3Input);
        }

        public void LookInput(InputAction.CallbackContext context)
        {
                Vector2 input = context.ReadValue<Vector2>();

                input.y = input.y * MouseXSensitivity * Time.deltaTime;
                input.x = input.x * MouseYSensitivity * Time.deltaTime;

                _characterPhysics.RequestRotate(input);
                _characterCamera.RequestLookUpRotation(input);
        }

        public void CrouchInput(InputAction.CallbackContext context)
        {
                switch (CrouchToggle)
                {
                    case true when context.phase == InputActionPhase.Started:
                        switch (_characterPhysics.CharacterState)
                        {
                            case ECharacterState.Crouch:
                                _characterPhysics.RequestStateChange(ECharacterState.Walk);
                                break;
                            default:
                                _characterPhysics.RequestStateChange(ECharacterState.Crouch);
                                break;
                        }

                        break;
                    case false:
                        switch (context.phase)
                        {
                            case InputActionPhase.Started:
                                _characterPhysics.RequestStateChange(ECharacterState.Crouch);
                                break;
                            case InputActionPhase.Canceled:
                                _characterPhysics.RequestStateChange(ECharacterState.Walk);
                                break;
                        }

                        break;
                }
        }

        public void RunInput(InputAction.CallbackContext context)
        {
                switch (RunToggle)
                {
                    case true when context.phase == InputActionPhase.Started:
                        switch (_characterPhysics.CharacterState)
                        {
                            case ECharacterState.Run:
                                _characterPhysics.RequestStateChange(ECharacterState.Walk);
                                break;
                            default:
                                _characterPhysics.RequestStateChange(ECharacterState.Run);
                                break;
                        }

                        break;
                    case false:
                        switch (context.phase)
                        {
                            case InputActionPhase.Started:
                                _characterPhysics.RequestStateChange(ECharacterState.Run);
                                break;
                            case InputActionPhase.Canceled:
                                _characterPhysics.RequestStateChange(ECharacterState.Walk);
                                break;
                        }

                        break;
                }
        }

        public void FlashLightInput(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                _characterCamera.FlashLight();
            }
        }
    }
}