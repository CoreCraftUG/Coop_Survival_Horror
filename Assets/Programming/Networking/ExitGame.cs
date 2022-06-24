using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoreCraft
{
    public class ExitGame : MonoBehaviour
    {
        [SerializeField] private Button _exitButton;

        void Start()
        {
            _exitButton.onClick.AddListener((() =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }));
        }
    }
}