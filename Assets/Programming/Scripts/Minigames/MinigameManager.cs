using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CoreCraft.Minigames
{
    public class MinigameManager : MonoBehaviour
    {
        public float _inputValue;
        public bool _inputBool;
        public float _inputValue2;
        public bool _inputBool2;
        
        // Start is called before the first frame update
        public void MinigameInput(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                _inputValue = context.ReadValue<float>();
                _inputBool = true;
            }
        }

        public void MinigameInput2(InputAction.CallbackContext context)
        {
            if(context.phase == InputActionPhase.Performed)
            {
                _inputValue2 = context.ReadValue<float>();
                _inputBool2 = true;
            }
        }
    }
}
