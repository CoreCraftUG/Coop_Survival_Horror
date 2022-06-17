using System;
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

    [RequireComponent(typeof(Rigidbody))]
    public class PhysicsCharacter : NetworkBehaviour
    {
        [Header("Events")]
        public UnityEvent<Vector3> WalkingEvent = new UnityEvent<Vector3>();
        public UnityEvent<Vector2> LookEvent = new UnityEvent<Vector2>();
        public UnityEvent<ECharacterState> CharacterStateEvent = new UnityEvent<ECharacterState>();
        public UnityEvent<ECharacterSoundType> CharacterSoundEvent = new UnityEvent<ECharacterSoundType>();

        [Space, Header("Components")]
        [SerializeField, ReadOnly] private Rigidbody _rigidbody;

        [SerializeField] private Transform _orientationTransform;

        [SerializeField] private Transform _playerObjectAttachmentTransform;

        [Space, Header("Variables")]
        [SerializeField] private float _walkSpeed;
        [SerializeField] private float _crouchSpeed;
        [SerializeField] private float _runSpeed;


        [SerializeField] private float _gravityMultiplier;

        [HideInInspector]
        public NetworkVariable<bool> FreeVision = new NetworkVariable<bool>(true);
        public Transform PlayerObjectAttachmentTransform { get; private set; }

        public NetworkVariable<ECharacterState> CharacterState = new NetworkVariable<ECharacterState>();

        private NetworkVariable<Vector3> _networkDirection = new NetworkVariable<Vector3>();
        private NetworkVariable<float> _networkYRotation = new NetworkVariable<float>();

        private NetworkVariable<bool> _movementRequested = new NetworkVariable<bool>(false);

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.freezeRotation = true;
            StateChangeRequestServerRpc(ECharacterState.Walk);
            PlayerObjectAttachmentTransform = _playerObjectAttachmentTransform;
        }

        private void Start()
        {

        }
        
        private void Update()
        {
            Debug.DrawLine(transform.position, transform.position + (transform.forward * 2),Color.green);
            if (IsServer)
            {
                float speed = CharacterState.Value switch
                {
                    ECharacterState.Walk => _walkSpeed,
                    ECharacterState.Run => _runSpeed,
                    ECharacterState.Crouch => _crouchSpeed,
                    _ => throw new ArgumentOutOfRangeException()
                };

                Vector3 orientation = transform.forward * _networkDirection.Value.z + transform.right * _networkDirection.Value.x;
                _rigidbody.AddForce(orientation.normalized * speed + new Vector3(0.0f, Physics.gravity.y * _gravityMultiplier, 0.0f), ForceMode.Force);
                Debug.Log($"{orientation.normalized * speed + new Vector3(0.0f, Physics.gravity.y * _gravityMultiplier, 0.0f)}");

                if (_movementRequested.Value)
                {
                    if (FreeVision.Value)
                        transform.rotation = Quaternion.Euler(0.0f, _networkYRotation.Value, 0.0f);

                    _movementRequested.Value = false;
                }
                _rigidbody.velocity = Vector3.zero;
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
            CharacterStateEvent.Invoke(CharacterState.Value);
            Logger.Instance.Log($"{CharacterState.Value}",ELogType.Debug);
        }
    }
}