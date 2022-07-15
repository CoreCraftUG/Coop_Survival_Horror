using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using Unity.VisualScripting;

namespace CoreCraft
{
    public class MovingObjectBase : NetworkBehaviour
    {
        [SerializeField] protected float _time;
        [SerializeField] public float Speed;

        public bool MoveDownBool { get; protected set; }

        protected Rigidbody _rigidbody => GetComponent<Rigidbody>();


        protected virtual void Start()
        {
#if UNITY_EDITOR
            // UnityEditor.Selection.activeObject = gameObject;
#endif
            tag = "MovingObject";
        }

        protected virtual void FixedUpdate()
        {
            if (!IsServer)
                return;
        }

        protected virtual void OnTriggerEnter(Collider collider)
        {
        }

        protected virtual void OnTriggerExit(Collider collider)
        {
        }

        protected virtual void OnTriggerStay(Collider collider)
        {
        }
    }
}
