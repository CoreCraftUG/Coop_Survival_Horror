using System;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityLayerMask;
using CoreCraft.Networking;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using Logger = CoreCraft.Core.Logger;

namespace CoreCraft.Character
{
    public enum ECharacterState
    {
        Walk,
        Run,
        Crouch
    }

    public enum ECharacterSoundType
    {

    }

    struct GroundedData
    {
        public bool IsGround;
        public bool IsSlope;
        public bool IsMoving;
        public bool StepAhead;
        public Vector3 GroundNormal;
        public Vector3 GroundPosition;
        public GameObject GroundObject;
    }

    [RequireComponent(typeof(Rigidbody))]
    public class PhysicsCharacter : NetworkBehaviour
    {
        [Header("Events")]
        public UnityEvent<Vector3> WalkingEvent = new UnityEvent<Vector3>();
        public UnityEvent<Vector2> LookEvent = new UnityEvent<Vector2>();
        public UnityEvent<ECharacterState> CharacterStateEvent = new UnityEvent<ECharacterState>();
        public UnityEvent<ECharacterSoundType> CharacterSoundEvent = new UnityEvent<ECharacterSoundType>();

        [Space, Header("Components")]
        [SerializeField] private Transform _orientationTransform;
        [SerializeField] private Transform _playerObjectAttachmentTransform;
        [SerializeField] private CapsuleCollider _characterCollider;

        private Rigidbody _rigidbody => GetComponent<Rigidbody>();

        [Space, Header("Move")]
        [SerializeField] private float _walkSpeed;
        [SerializeField] private float _crouchSpeed;
        [SerializeField] private float _runSpeed;
        [SerializeField] private float _airControl;

        [Space, Header("Crouch")]
        [SerializeField] private float _crouchHeight;
        [SerializeField] private float _standardWalkHeight;

        [Space, Header("Ground Check")]
        [SerializeField] private float _playerHeight;
        [SerializeField] private float _groundDrag;
        [SerializeField] private LayerMask _groundLayer;

        private bool _grounded;

        [Space, Header("Steps")]
        [SerializeField] private float _stepOffset;
        [SerializeField] private float _playerRadius;

        [Space,Header("Physics")]
        [SerializeField] private float _gravityMultiplier;
        [SerializeField] private float _maxSlopeAngle;

        [HideInInspector]
        public NetworkVariable<bool> FreeVision = new NetworkVariable<bool>(true);
        [HideInInspector]
        public NetworkVariable<ECharacterState> CharacterState = new NetworkVariable<ECharacterState>();

        public Transform PlayerObjectAttachmentTransform { get; private set; }

        private NetworkVariable<Vector3> _networkDirection = new NetworkVariable<Vector3>();
        private NetworkVariable<float> _networkYRotation = new NetworkVariable<float>();

        private NetworkVariable<bool> _movementRequested = new NetworkVariable<bool>(false);

        private float _currentSpeed;
        private GroundedData _lastGroundedData = new GroundedData();
        private GroundedData _groundedData = new GroundedData();

        private void Awake()
        {
            _rigidbody.freezeRotation = true;
            StateChangeRequestServerRpc(ECharacterState.Walk);
            PlayerObjectAttachmentTransform = _playerObjectAttachmentTransform;
            _rigidbody.useGravity = false;
        }

        private void Start()
        {

        }
        
        private void Update()
        {
            if (!IsServer)
                return;

            _currentSpeed = CharacterState.Value switch
            {
                ECharacterState.Walk => _walkSpeed,
                ECharacterState.Run => _runSpeed,
                ECharacterState.Crouch => _crouchSpeed,
                _ => throw new ArgumentOutOfRangeException()
            };

            Vector3 orientation = transform.forward * _networkDirection.Value.z + transform.right * _networkDirection.Value.x;

            switch (_groundedData.IsGround)
            {
                case true when CanTraverseGround():

                    /*
                    StepCheck();
                    if (_groundedData.StepAhead && !_groundedData.IsSlope)
                        _rigidbody.AddForce(transform.up * _stepOffset * _currentSpeed, ForceMode.Force);
                    */

                    orientation = Vector3.ProjectOnPlane(orientation, _groundedData.GroundNormal).normalized * _currentSpeed;
                    Debug.DrawRay(transform.position - (orientation / 2), orientation, Color.green);

                    if (_groundedData.IsMoving)
                    {
                        MovingObjectBase moveObj = _groundedData.GroundObject.GetComponent<MovingObjectBase>();
                        orientation += new Vector3(/* X */_groundedData.GroundObject.GetComponent<Rigidbody>().velocity.x,
                            /* Y */moveObj.MoveDownBool? _groundedData.GroundObject.GetComponent<Rigidbody>().velocity.y - 0.1f : _groundedData.GroundObject.GetComponent<Rigidbody>().velocity.y - orientation.y,
                            /* Z */ _groundedData.GroundObject.GetComponent<Rigidbody>().velocity.z);
                    }

                    _rigidbody.AddForce(orientation, ForceMode.VelocityChange);
                    Debug.Log($"{_rigidbody.velocity}");

                    // if (_groundedData.IsMoving)
                    // {
                    //     MovingObjectBase moveObj = _groundedData.GroundObject.GetComponent<MovingObjectBase>();
                    //     _rigidbody.velocity = new Vector3(/* X */ _rigidbody.velocity.x + _groundedData.GroundObject.GetComponent<Rigidbody>().velocity.x,
                    //         /* Y */moveObj.MoveDownBool ? _rigidbody.velocity.y +_groundedData.GroundObject.GetComponent<Rigidbody>().velocity.y - 0.1f : _rigidbody.velocity.y + _groundedData.GroundObject.GetComponent<Rigidbody>().velocity.y,
                    //             /* Z */ _rigidbody.velocity.z + _groundedData.GroundObject.GetComponent<Rigidbody>().velocity.z);
                    // }

                    _rigidbody.drag = _groundDrag;
                    SpeedControl();

                    break;

                case false:

                    _rigidbody.AddForce(orientation.normalized * _currentSpeed * _airControl, ForceMode.Force);
                    _rigidbody.drag = 0;

                    break;
            }

            // if (!_groundedData.IsMoving)
            // {
            //     _rigidbody.AddForce(-transform.up * _gravityMultiplier, ForceMode.Force);
            // }
            _rigidbody.AddForce(-transform.up * _gravityMultiplier, ForceMode.Force);

            if (_movementRequested.Value)
            {
                if (FreeVision.Value) transform.rotation = Quaternion.Euler(0.0f, _networkYRotation.Value, 0.0f);

                _movementRequested.Value = false;
            }

            _lastGroundedData = _groundedData;
        }

        private bool CanTraverseGround()
        {
            float angle = Vector3.Angle(transform.forward, _groundedData.GroundNormal);
            if (angle < _maxSlopeAngle && _groundedData.GroundPosition.y > _lastGroundedData.GroundPosition.y)
            {
                return false;
            }
            else if (angle is > 85 and < 95)
            {
                _groundedData.IsSlope = false;
                return true;
            }
            else
            {
                _groundedData.IsSlope = true;
                return true;
            }
        }

        private void StepCheck()
        {
            Vector3 origin = new Vector3(transform.position.x, (transform.position.y - _playerHeight / 2) + _stepOffset , transform.position.z) + (transform.forward * _playerRadius);
            _groundedData.StepAhead = Physics.Raycast(origin,transform.up * -1, _stepOffset , _groundLayer);
            Debug.DrawRay(origin, transform.up * -_stepOffset, Color.red);
        }

        private void FixedUpdate()
        {
            if (IsServer)
                GroundCheck();
        }

        private void GroundCheck()
        {
            Debug.DrawRay(transform.position, Vector3.down * _playerHeight * 0.65f, Color.yellow);
            if (Physics.Raycast(transform.position, Vector3.down,out RaycastHit hit, _playerHeight * 0.65f, _groundLayer))
            {
                _groundedData.IsGround = true;
                _groundedData.GroundNormal = hit.normal;
                _groundedData.GroundPosition = hit.point;
                _groundedData.IsMoving = hit.transform.gameObject.layer == 7;
                _groundedData.GroundObject = hit.transform.gameObject;
            }
            else
            {
                _groundedData.IsGround = false;
            }
        }

        private void SpeedControl()
        {
            if (_groundedData.IsSlope)
            {
                if (_rigidbody.velocity.magnitude > _currentSpeed)
                {
                    Vector3 limitedVelocity = _rigidbody.velocity.normalized * _currentSpeed;
                    _rigidbody.velocity = new Vector3(limitedVelocity.x, _rigidbody.velocity.y, limitedVelocity.z);
                }
            }
            else
            {
                Vector3 flatVelocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
                if (flatVelocity.magnitude > _currentSpeed)
                {
                    Vector3 limitedVelocity = flatVelocity.normalized * _currentSpeed;
                    _rigidbody.velocity = new Vector3(limitedVelocity.x, _rigidbody.velocity.y, limitedVelocity.z);
                }
            }
        }

        public void RequestStateChange(ECharacterState state)
        {
            StateChangeRequestServerRpc(state);
        }

        public void RequestMove(Vector3 directionInput)
        {
            MoveRequestServerRpc(directionInput);
        }

        public void RequestRotation(Vector2 rotationInput)
        {
            RotateRequestServerRpc(rotationInput);
        }

        [ServerRpc(RequireOwnership = false)]
        private void MoveRequestServerRpc(Vector3 direction)
        {
            _networkDirection.Value = direction;
            WalkingEvent.Invoke(direction);
        }

        [ServerRpc(RequireOwnership = false)]
        private void RotateRequestServerRpc(Vector2 rotation)
        {
            _movementRequested.Value = true;
            LookEvent.Invoke(rotation);
            _networkYRotation.Value += rotation.x;
        }

        [ServerRpc(RequireOwnership = false)]
        private void StateChangeRequestServerRpc(ECharacterState state)
        {
            CharacterState.Value = state;
            if (CharacterState.Value == ECharacterState.Crouch)
            {
                _characterCollider.height = _crouchHeight;
                _rigidbody.AddForce(Vector3.down * 5.0f, ForceMode.Impulse);
            }
            else
                _characterCollider.height = _standardWalkHeight;

            CharacterStateEvent.Invoke(CharacterState.Value);
            Logger.Instance.Log($"{CharacterState.Value}", ELogType.Debug);
        }
    }
}