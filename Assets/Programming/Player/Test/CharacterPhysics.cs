using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityCharacterController;
using CoreCraft.Networking;
using UnityEngine;

namespace CoreCraft.Character
{
    [RequireComponent(typeof(CharacterController))]
    public class CharacterPhysics : MonoBehaviour
    {
        [SerializeField] private float _groundCheckDepth;
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private float _crouchHeight;
        [SerializeField] private float _standardWalkHeight;
        [SerializeField] private float _gravityMultiply;
        [SerializeField] private float _airDrag;
        [SerializeField] private float _groundDrag;
        [SerializeField] public Transform CameraHolderTransform;

        [Space, Header("Move")]
        [SerializeField] private float _walkSpeed;
        [SerializeField] private float _crouchSpeed;
        [SerializeField] private float _runSpeed;
        [SerializeField] private float _airControl;

        //Move Data
        private float _yRotation;
        private Vector3 _direction;
        private bool _moveRequest;

        [HideInInspector]
        public ECharacterState CharacterState;

        private CapsuleCollider _characterCollider => transform.GetComponentInChildren<CapsuleCollider>();
        private CharacterController _characterController => transform.GetComponent<CharacterController>();
        private float _yGravity => Physics.gravity.y;

        private GroundedData _groundedData = new GroundedData();
        private float _currentSpeed;
        private Vector3 _lastDirection;
        private float _lastSpeed;

        void Start()
        {
        
        }
        
        void Update()
        {
            //Ground Check
            GroundCheck();
            transform.SetParent(_groundedData.IsMoving ? _groundedData.GroundObject.transform : null);

            //Move
            switch (_moveRequest)
            {
                case true when _groundedData.IsGround: //Grounded & Move requested
                {
                    _currentSpeed = CharacterState switch
                    {
                        ECharacterState.Walk => _walkSpeed,
                        ECharacterState.Run => _runSpeed,
                        ECharacterState.Crouch => _crouchSpeed,
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    Vector3 moveDirection = transform.forward * _direction.x + transform.right * _direction.z;
                    Vector3 gravityDirection = Vector3.zero; 


                    moveDirection = Vector3.ProjectOnPlane(moveDirection, _groundedData.GroundNormal).normalized;
                    Debug.DrawRay(transform.position + transform.up - (moveDirection * 2), moveDirection * 4, Color.green);

                    if (!_groundedData.IsMoving)
                    {
                        gravityDirection += transform.up * (_yGravity * _gravityMultiply);
                    }
                    else
                    {
                        // gravityDirection += transform.up * _groundedData.GroundObject.GetComponent<MovingObjectBase>().MovingYDistance;
                    }

                    _lastDirection = moveDirection;
                    _lastSpeed = _currentSpeed;

                    _characterController.Move(((moveDirection * _currentSpeed) + gravityDirection) * Time.deltaTime);
                    break;
                }
                case true: //not Grounded & Move requested
                {
                    _currentSpeed = _walkSpeed;
                    CharacterState = ECharacterState.Walk;

                    if (_lastSpeed < 0.1)
                    {
                        _lastSpeed = 0;
                        _lastDirection = Vector3.zero;
                    }

                    Vector3 airMoveDirection = Vector3.zero;
                    Vector3 gravityDirection = Vector3.zero;

                    if (_lastSpeed > 0)
                    {
                        airMoveDirection = transform.forward * _direction.x + transform.right * _direction.z;
                        _lastDirection += airMoveDirection;
                        _lastDirection = _lastDirection.normalized * _lastSpeed;

                        if (!_groundedData.IsMoving)
                        {
                            gravityDirection += transform.up * (_yGravity * _gravityMultiply);
                        }
                        else
                        {
                            // gravityDirection += transform.up * _groundedData.GroundObject.GetComponent<MovingObjectBase>().MovingYDistance;
                        }

                        _characterController.Move(((_lastDirection * _airControl * _lastSpeed) + gravityDirection) * Time.deltaTime);
                        _lastSpeed -= _airDrag * Time.deltaTime;
                    }
                    else
                    {
                        if (!_groundedData.IsMoving)
                        {
                            _lastDirection += transform.up * (_yGravity * _gravityMultiply);
                        }
                        else
                        {
                            // gravityDirection += transform.up * _groundedData.GroundObject.GetComponent<MovingObjectBase>().MovingYDistance;
                        }
                        _characterController.Move(_lastDirection * Time.deltaTime);
                        _lastSpeed -= _airDrag * Time.deltaTime;
                    }
                }
                    break;
                case false when !_groundedData.IsGround: //not Grounded & no Move requested
                {
                    _currentSpeed = _walkSpeed;
                    CharacterState = ECharacterState.Walk;
                    Vector3 gravityDirection = Vector3.zero;

                    if (_lastSpeed < 0.1)
                    {
                        _lastSpeed = 0;
                        _lastDirection = Vector3.zero;
                    }

                    if (_lastSpeed > 0)
                    {
                        if (!_groundedData.IsMoving)
                        {
                            gravityDirection += transform.up * (_yGravity * _gravityMultiply);
                        }
                        else
                        {
                            // gravityDirection += transform.up * _groundedData.GroundObject.GetComponent<MovingObjectBase>().MovingYDistance;
                        }

                        _characterController.Move(((_lastDirection * _lastSpeed) + gravityDirection) * Time.deltaTime);
                        _lastSpeed -= _airDrag * Time.deltaTime;
                    }
                    else
                    {
                        _lastDirection += transform.up * (_yGravity * _gravityMultiply);
                        _characterController.Move(_lastDirection * Time.deltaTime);
                        _lastSpeed -= _airDrag * Time.deltaTime;
                    }
                }
                    break;
                default: //Grounded & no Move requested
                {
                    Vector3 gravityDirection = Vector3.zero;

                    if (_lastSpeed < 0.1)
                    {
                        _lastSpeed = 0;
                        _lastDirection = Vector3.zero;
                    }

                    if (!_groundedData.IsMoving)
                    {
                        gravityDirection += transform.up * (_yGravity * _gravityMultiply);
                    }
                    else
                    {
                        // gravityDirection += transform.up * _groundedData.GroundObject.GetComponent<MovingObjectBase>().MovingYDistance;
                    }

                    if (_lastSpeed > 0)
                    {
                        _characterController.Move(((_lastDirection * _lastSpeed) + gravityDirection) * Time.deltaTime);
                        _lastSpeed -= _groundDrag * Time.deltaTime;
                    }
                    else
                    {
                        _characterController.Move(_lastDirection * Time.deltaTime);
                        _lastSpeed -= _groundDrag * Time.deltaTime;
                    }
                }
                    break;
            }

            //Rotation
            transform.rotation = Quaternion.Euler(0.0f, _yRotation, 0.0f);

            //
            // _moveRequest = false;
        }

        public void RequestMove(bool started,Vector3 moveVector)
        {
            _direction = moveVector;
            _moveRequest = started;
        }

        public void RequestRotate(Vector2 rotation)
        {
            _yRotation += rotation.x;
        }

        public void RequestStateChange(ECharacterState state)
        {
            CharacterState = state;
            if (CharacterState == ECharacterState.Crouch)
            {
                _characterCollider.height = _crouchHeight;
                _characterController.height = _crouchHeight;
            }
            else
            {
                _characterCollider.height = _standardWalkHeight;
                _characterController.height = _standardWalkHeight;
            }
        }

        private void GroundCheck()
        {
            Debug.DrawRay(transform.position, Vector3.down * _groundCheckDepth, Color.yellow);
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, _groundCheckDepth, _groundLayer))
            {
                _groundedData.IsGround = true;
                _groundedData.GroundNormal = hit.normal;
                _groundedData.GroundPosition = hit.point;
                _groundedData.IsMoving = hit.transform.gameObject.layer == 7;
                _groundedData.GroundObject = hit.transform.gameObject;

                float angle = Vector3.Angle(transform.forward, hit.normal);
                _groundedData.IsSlope = angle is <= 88 or >= 92;
            }
            else
            {
                _groundedData.IsGround = false;
                _groundedData.IsMoving = false;
            }
        }
    }
}
