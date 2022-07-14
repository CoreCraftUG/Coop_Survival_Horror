using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace CoreCraft
{
    public class MovingPlatform : MovingObjectBase
    {
        protected Transform _startTransform;
        [SerializeField] protected Transform _targetTransform;

        protected bool change = false;
        public Vector3 Position;
        public bool MoveForwardBool { get; protected set; }
        public bool MoveBackBool { get; protected set; }


        protected override void Start()
        {
            base.Start();
            _startTransform = transform;
            Position = transform.position;
        }
        
        protected override void Update()
        {
            base.Update();

            if (MoveForwardBool)
            {
                if (change)
                {
                    Debug.Log($"Object Is moving Up");
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

                    _rigidbody.velocity = (_targetTransform.position - _startTransform.position).normalized * Speed * Time.deltaTime;
                    Position = transform.position;
                }
            }
            else if (MoveBackBool)
            {
                if (change)
                {
                    Debug.Log($"Object Is moving Down");
                    change = (Vector3.Distance(transform.position, _startTransform.position) > 0.2f);
                    MoveDownBool = change;

                    if (Vector3.Distance(transform.position, _startTransform.position) > 0.2f)
                    {
                        _rigidbody.velocity = Vector3.zero;
                        transform.position = _startTransform.position;
                        Position = transform.position;
                        MoveForwardBool = false;
                        MoveBackBool = false;
                        change = false;
                        return;
                    }

                    _rigidbody.velocity = (_startTransform.position - _targetTransform.position).normalized * Speed * Time.deltaTime;
                    Position = transform.position;
                }
            }
            else
            {
                _rigidbody.velocity = Vector3.zero;
                transform.position = Position;
            }
        }

        public void MoveForward(ulong clientId)
        {
            Debug.Log($"Object Move Up");
            MoveForwardServerRpc();
        }

        public void MoveBack(ulong clientId)
        {
            Debug.Log($"Object Move Down");
            MoveBackServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        public void MoveForwardServerRpc()
        {
            Debug.Log($"Object Move Up Server");
            MoveForwardBool = true;
            MoveBackBool = false;
            change = true;
        }

        [ServerRpc(RequireOwnership = false)]
        public void MoveBackServerRpc()
        {
            Debug.Log($"Object Move Down Server");
            MoveForwardBool = false;
            MoveBackBool = true;
            change = true;
        }
    }
}
