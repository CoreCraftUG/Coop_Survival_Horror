using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreCraft.Character
{
    public class CharacterCamera : MonoBehaviour
    {
        [SerializeField] public bool LookConstrained;
        [SerializeField] private bool _lockCursor;

        [SerializeField] private GameObject _flashLightObject;

        private float _xRotation;
        private float _yRotation;

        [SerializeField] private bool _test = true;
        private float _lightIntensity;
        private System.Random _random;

        void Start()
        { 
            LockCursor(_lockCursor);
            _lightIntensity = _flashLightObject.GetComponent<Light>().intensity;
            _random = new System.Random();
        }

        void Update()
        {
            transform.rotation = Quaternion.Euler(_xRotation, _yRotation, 0.0f);
        }

        public void RequestLookUpRotation(Vector2 rotationInput)
        {
            _yRotation += rotationInput.x;
            _xRotation -= rotationInput.y;

            if (LookConstrained)
                _xRotation = Mathf.Clamp(_xRotation, -90.0f, 90.0f);
        }

        public void LockCursor(bool lockState)
        {
            Cursor.lockState = lockState switch
            {
                true => CursorLockMode.Locked,
                false => CursorLockMode.None
            };

            Cursor.visible = !lockState;
        }

        public void FlashLight()
        {
            _flashLightObject.SetActive(!_flashLightObject.active);
            if (!_test)
                return;

            if (_random.Next(0, 100) < 15)
            {
                _flashLightObject.GetComponent<Light>().intensity = 10000;
            }
            else
            {
                _flashLightObject.GetComponent<Light>().intensity = _lightIntensity;
            }
        }
    }
}
