using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreCraft
{
    public class LightChain : MonoBehaviour
    {

        [SerializeField] private List<GameObject> _lights = new List<GameObject>();
        private bool _isRunning = false;
        [SerializeField] private float _lightDuration;
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            if (!_isRunning)
                StartCoroutine(LightChainActive());
        }

        private IEnumerator LightChainActive()
        {
            _isRunning = true;
            for(int i = 0; i < _lights.Count; i++)
            {
                _lights[i].SetActive(true);
                yield return new WaitForSeconds(_lightDuration);
                _lights[i].SetActive(false);
            }
            yield return new WaitForEndOfFrame();
            _isRunning = false;
        }
    }
}
