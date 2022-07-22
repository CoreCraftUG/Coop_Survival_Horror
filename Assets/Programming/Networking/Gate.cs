using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreCraft
{
    [RequireComponent(typeof(Animator))]
    public class Gate : MonoBehaviour
    {
        private Animator _animator => GetComponent<Animator>();

        public void SetCloseBool(bool close)
        {
            _animator.SetBool("Close", close);
        }
    }
}
