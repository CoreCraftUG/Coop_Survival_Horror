using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;



namespace CoreCraft.Minigames
{
    public class BMultiplayer : MultiplayerPuzzleTest
    {
        private InputActionAsset _inputsActionAsset = new InputActionAsset();
        private InputActionMap _map;
        // Start is called before the first frame update
        void Start()
        {
        
        }

        protected override void Awake()
        {
            base.Awake();
            _map = _inputsActionAsset.FindActionMap("MinigameMap");

            _map.FindAction("MiniGame1").started += MinigameInput;
            _map.FindAction("MiniGame2").performed += MinigameInput2;
            _complete = false;
        }
            //transform.GetComponent<NetworkObject>().OwnerClientId;
            // Update is called once per frame
         void Update()
        {
        
        }
        public override void MinigameInput2(InputAction.CallbackContext context)
        {
            base.MinigameInput2(context);
        }
    }
}
