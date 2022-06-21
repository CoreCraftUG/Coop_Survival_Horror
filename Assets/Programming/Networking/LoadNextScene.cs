using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoreCraft
{
    public class LoadNextScene : MonoBehaviour
    {
        [SerializeField] private string _nextSceneName;

        private void Start()
        {
            StartCoroutine(LoadScene());
        }

        private IEnumerator LoadScene()
        {
            yield return null;
            SceneManager.LoadScene(_nextSceneName);
        }
    }
}
