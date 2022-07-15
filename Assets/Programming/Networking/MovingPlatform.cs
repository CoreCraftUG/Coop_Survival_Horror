using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace CoreCraft
{
    public class MovingPlatform : MovingObjectBase
    {
        protected Vector3 _startPosition;
        [SerializeField] protected Transform _targetTransform;

        protected bool change = false;
        public Vector3 Position;
        public bool MoveForwardBool { get; protected set; }
        public bool MoveBackBool { get; protected set; }

        public UnityEvent<bool> _moving = new UnityEvent<bool>();


        protected override void Start()
        {
            base.Start();
            _startPosition = transform.position;
            Position = transform.position;
        }
        
        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (MoveForwardBool)
            {
                _moving.Invoke(true);

                if (change)
                {
                    change = (Vector3.Distance(transform.position,_targetTransform.position) > 0.2f);
                    MoveForwardBool = change;

                    if (Vector3.Distance(transform.position, _targetTransform.position) <= 0.2f)
                    {
                        _rigidbody.velocity = Vector3.zero;
                        transform.position = _targetTransform.position;
                        Position = transform.position;
                        MoveForwardBool = false;
                        MoveBackBool = false;
                        change = false;
                        return;
                    }

                    _rigidbody.velocity = (_targetTransform.position - _startPosition).normalized * Speed * Time.deltaTime;
                    // _rigidbody.MovePosition(transform.position + (_targetTransform.position - _startPosition).normalized * Speed * Time.fixedDeltaTime);
                    Position = transform.position;
                }
            }
            else if (MoveBackBool)
            {
                _moving.Invoke(true);

                if (change)
                {
                    change = (Vector3.Distance(transform.position, _startPosition) > 0.2f);
                    MoveDownBool = change;

                    if (Vector3.Distance(transform.position, _startPosition) <= 0.2f)
                    {
                        _rigidbody.velocity = Vector3.zero;
                        transform.position = _startPosition;
                        Position = transform.position;
                        MoveForwardBool = false;
                        MoveBackBool = false;
                        change = false;
                        return;
                    }

                    _rigidbody.velocity = (_startPosition - _targetTransform.position).normalized * Speed * Time.deltaTime;
                    // _rigidbody.MovePosition(transform.position + (_startPosition - _targetTransform.position).normalized * Speed * Time.fixedDeltaTime);
                    Position = transform.position;
                }
            }
            else
            {
                _moving.Invoke(false);
                _rigidbody.velocity = Vector3.zero;
                transform.position = Position;
            }
        }

        public void MoveForward(ulong clientId)
        {
            MoveForwardServerRpc();
        }

        public void MoveBack(ulong clientId)
        {
            MoveBackServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        public void MoveForwardServerRpc()
        {
            MoveForwardBool = true;
            MoveBackBool = false;
            change = true;
        }

        [ServerRpc(RequireOwnership = false)]
        public void MoveBackServerRpc()
        {
            MoveForwardBool = false;
            MoveBackBool = true;
            change = true;
        }
    }
}
