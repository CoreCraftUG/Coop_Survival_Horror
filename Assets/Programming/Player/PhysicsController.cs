using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using Logger = CoreCraft.Core.Logger;

namespace CoreCraft.Character
{
    public class PhysicsController : NetworkBehaviour
    {
        [Header("Events")]
        public UnityEvent<Vector3> WalkingEvent = new UnityEvent<Vector3>();
        public UnityEvent<Vector2> LookEvent = new UnityEvent<Vector2>();
        public UnityEvent<ECharacterState> CharacterStateEvent = new UnityEvent<ECharacterState>();
        public UnityEvent<ECharacterSoundType> CharacterSoundEvent = new UnityEvent<ECharacterSoundType>();

        [Space, Header("Components")]
        [SerializeField] private CharacterController _cController;
        [SerializeField] private Transform _orientationTransform;
        [SerializeField] private Transform _playerObjectAttachmentTransform;
        [SerializeField] private CapsuleCollider _characterCollider;

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
        [SerializeField] private float _airDrag;
        [SerializeField] private LayerMask _groundLayer;

        private bool _grounded;

        [Space, Header("Physics")]
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
            _cController = GetComponent<CharacterController>();
            StateChangeRequestServerRpc(ECharacterState.Walk);
            PlayerObjectAttachmentTransform = _playerObjectAttachmentTransform;
        }

        private void Start()
        {

        }

        private void Update()
        {
            if (!IsServer) return;

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
                    orientation = Vector3.ProjectOnPlane(orientation, _groundedData.GroundNormal).normalized;
                    _cController.SimpleMove(orientation * _currentSpeed);

                    transform.parent = _groundedData.IsMoving ? _groundedData.GroundObject.transform : null;
                    break;
                case false:
                    _cController.SimpleMove(orientation.normalized * _currentSpeed / _airDrag * _airControl);
                    break;
            }

            if (!_groundedData.IsMoving)
            {
                // _cController.Move(Vector3.down * _gravityMultiplier);
            }

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

        private void FixedUpdate()
        {
            if (IsServer)
                GroundCheck();
        }

        private void GroundCheck()
        {
            Debug.DrawRay(transform.position, Vector3.down * _playerHeight * 0.6f, Color.yellow);
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, _playerHeight * 0.6f, _groundLayer))
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

        [ServerRpc]
        private void MoveRequestServerRpc(Vector3 direction)
        {
            _networkDirection.Value = direction;
            WalkingEvent.Invoke(direction);
        }

        [ServerRpc]
        private void RotateRequestServerRpc(Vector2 rotation)
        {
            _movementRequested.Value = true;
            LookEvent.Invoke(rotation);
            _networkYRotation.Value += rotation.x;
        }

        [ServerRpc]
        private void StateChangeRequestServerRpc(ECharacterState state)
        {
            CharacterState.Value = state;
            if (CharacterState.Value == ECharacterState.Crouch)
            {
                _characterCollider.height = _crouchHeight;
                _cController.height = _crouchHeight;
            }
            else
            {
                _characterCollider.height = _standardWalkHeight;
                _cController.height = _standardWalkHeight;
            }

            CharacterStateEvent.Invoke(CharacterState.Value);
            Logger.Instance.Log($"{CharacterState.Value}", ELogType.Debug);
        }
    }
}
