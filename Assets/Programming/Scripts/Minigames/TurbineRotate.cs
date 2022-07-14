using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreCraft
{
    public class TurbineRotate : MonoBehaviour
    {
        [SerializeField] private float _rotationSpeed;
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            this.transform.Rotate(0, Time.deltaTime * _rotationSpeed, 0);
        }
    }
}
