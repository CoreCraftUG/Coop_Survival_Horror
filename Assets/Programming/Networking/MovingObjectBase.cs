using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;

namespace CoreCraft.Networking
{
    public class MovingObjectBase : NetworkBehaviour
    {
        [SerializeField] private float _targetHeight;
        [SerializeField] private float _time;

        private Rigidbody _rigidbody => GetComponent<Rigidbody>();

        private bool change = false;
        private bool canMove = true;

        private float _timer;

        public Vector3 Velocity;

        private float _startHeight;
        [SerializeField] private float _speed;

        public Vector3 CurrentPosition { get; private set; }
        public Vector3 LastPosition { get; private set; }

        public float MovingYDistance { get; private set; }

        private List<GameObject> collidingObjects = new List<GameObject>();
        private bool _willMove;

        protected virtual void Start()
        {
#if UNITY_EDITOR
            UnityEditor.Selection.activeObject = gameObject;
#endif
            // EventManager.Instance.GameStarted.AddListener((started => { GetComponent<NetworkObject>().Spawn(); Debug.Log($"Game started: {started}"); }));
            // GetComponent<NetworkObject>().Spawn();
            Debug.Log($"Object spawned: {GetComponent<NetworkObject>().IsSpawned}");
            tag = "MovingObject";
            _startHeight = transform.position.y;
            CurrentPosition = transform.position;
            LastPosition = transform.position;
            // _speed = Mathf.Abs(_startHeight - _targetHeight) / _time;
        }
        
        protected virtual void FixedUpdate()
        {
            if (!IsServer)
                return;

            if (transform.position.y < _targetHeight && !change)
            {
                change = false;
                _rigidbody.AddForce(Vector3.up * _speed * Time.deltaTime, ForceMode.Force);
            }
            else
            {
                change = !(transform.position.y <= _startHeight);
                _rigidbody.AddForce(Vector3.down * _speed * Time.deltaTime, ForceMode.Force);
            }

            Velocity = _rigidbody.velocity;
            Debug.Log($"Position: {transform.position}");
            /*if (IsServer && _willMove)
            {
                if (/*IsServer &&/ canMove)
                {
                    canMove = false;
                    if (change)
                    {
                        transform.DOMoveY(_targetHeight, _time).OnComplete(() => {
                            change = !change;
                            canMove = true;
                            _willMove = false;
                        }).OnUpdate((() =>
                        {
                            LastPosition = CurrentPosition;
                            CurrentPosition = transform.position;
                            MovingYDistance = LastPosition.y - CurrentPosition.y;
                        }));
                    }
                    else
                    {
                        transform.DOMoveY(_startHeight, _time).OnComplete(() => {
                            change = !change;
                            canMove = true;
                            _willMove = false;
                        }).OnUpdate((() =>
                        {
                            LastPosition = CurrentPosition;
                            CurrentPosition = transform.position;
                            MovingYDistance = LastPosition.y - CurrentPosition.y;
                        }));
                    }
                }
                if (transform.position.y < _targetHeight && !change)
                {
                    change = false;
                    _rigidbody.AddForce(transform.up * _speed * Time.deltaTime);
                }
                else if (transform.position.y > _startHeight && change)
                {
                    // change = !(transform.position.y <= _startHeight);
                    _rigidbody.AddForce(-transform.up * _speed * Time.deltaTime);
                }
                else if(transform.position.y > _targetHeight && !change)
                {
                    _willMove = false;
                    change = true;
                }
                else if(transform.position.y < _startHeight && change)
                {
                    _willMove = false;
                    change = false;
                }
            }
            else
            {
                if (_timer > 2)
                {
                    _willMove = true;
                    _timer = 0;
                }
                else
                    _timer += NetworkManager.Singleton.ServerTime.FixedDeltaTime;
            }*/
        }

        protected virtual void OnTriggerEnter(Collider collider)
        {
            if (collider.transform.tag == "Player")
            {
                collidingObjects.Add(collider.transform.gameObject);
            }
        }

        protected virtual void OnTriggerExit(Collider collider)
        {
            if (collider.transform.tag == "Player" && collidingObjects.Contains(collider.transform.gameObject))
            {
                collidingObjects.Remove(collider.transform.gameObject);
            }
        }

        protected virtual void OnTriggerStay(Collider collider)
        {
        }
    }
}
