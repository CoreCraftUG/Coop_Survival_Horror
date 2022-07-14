using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace CoreCraft
{
    public class Elevator : MovingObjectBase
    {
        protected float _startHeight;
        [SerializeField] protected float _targetHeight;

        protected bool change = false;
        public Vector3 Position;
        public bool MoveUpBool { get; protected set; }


        protected override void Start()
        {
            base.Start();
            _startHeight = transform.position.y;
            Position = transform.position;
        }

        protected override void Update()
        {
            base.Update();

            if (MoveUpBool)
            {
                if (change)
                {
                    Debug.Log($"Object Is moving Up");
                    change = (transform.position.y < _targetHeight);
                    MoveUpBool = change;

                    if (transform.position.y >= _targetHeight)
                    {
                        _rigidbody.velocity = Vector3.zero;
                        transform.position = new Vector3(transform.position.x, _targetHeight, transform.position.z);
                        Position = transform.position;
                        MoveUpBool = false;
                        MoveDownBool = false;
                        change = false;
                        return;
                    }

                    _rigidbody.velocity = Vector3.up * Speed * Time.deltaTime;
                    Position = transform.position;
                }
            }
            else if (MoveDownBool)
            {
                if (change)
                {
                    Debug.Log($"Object Is moving Down");
                    change = (transform.position.y > _startHeight);
                    MoveDownBool = change;

                    if (transform.position.y <= _startHeight)
                    {
                        _rigidbody.velocity = Vector3.zero;
                        transform.position = new Vector3(transform.position.x, _startHeight, transform.position.z);
                        Position = transform.position;
                        MoveUpBool = false;
                        MoveDownBool = false;
                        change = false;
                        return;
                    }

                    _rigidbody.velocity = Vector3.down * Speed * Time.deltaTime;
                    Position = transform.position;
                }
            }
            else
            {
                _rigidbody.velocity = Vector3.zero;
                transform.position = Position;
            }
        }

        public void MoveUp(ulong clientId)
        {
            Debug.Log($"Object Move Up");
            MoveUpServerRpc();
        }

        public void MoveDown(ulong clientId)
        {
            Debug.Log($"Object Move Down");
            MoveDownServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        public void MoveUpServerRpc()
        {
            Debug.Log($"Object Move Up Server");
            MoveUpBool = true;
            MoveDownBool = false;
            change = true;
        }

        [ServerRpc(RequireOwnership = false)]
        public void MoveDownServerRpc()
        {
            Debug.Log($"Object Move Down Server");
            MoveUpBool = false;
            MoveDownBool = true;
            change = true;
        }
    }
}
