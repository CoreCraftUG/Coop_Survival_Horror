using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreCraft.Minigames
{
    public class MultiplayerPuzzleTest : BaseMinigame
    {
        [SerializeField] protected AMultiplayerTest _test1;
        [SerializeField] protected BMultiplayer _test2;
        protected bool _complete;
        // Start is called before the first frame update
        protected override void Awake()
        {
            base.Awake();
            _complete = false;
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        protected virtual void CheckComplete()
        {
            if (_test1._complete && _test2._complete)
                _complete = true;
        }
    }
}
