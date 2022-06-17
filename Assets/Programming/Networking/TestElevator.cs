using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using DG.Tweening;

namespace CoreCraft.Networking
{
    public class TestElevator : NetworkBehaviour
    {
        [SerializeField] private float _targetHeight;
        [SerializeField] private float _time;

        private bool change = false;
        private bool canMove = true;

        private float _startHeight;

        void Start()
        {
            _startHeight = transform.position.y;
        }
        
        void Update()
        {
            if (IsServer && canMove)
            {
                canMove = false;
                if (change)
                    transform.DOMoveY(_targetHeight, _time).OnComplete(() => { change = !change;
                        canMove = true;
                    });
                else
                    transform.DOMoveY(_startHeight, _time).OnComplete(() => { change = !change;
                        canMove = true;
                    });
            }
        }
    }
}
