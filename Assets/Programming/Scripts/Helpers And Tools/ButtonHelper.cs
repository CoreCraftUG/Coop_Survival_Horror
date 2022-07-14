using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoreCraft
{
    [RequireComponent(typeof(Button))]
    public class ButtonHelper : MonoBehaviour
    {
        [SerializeField] private Button _button => GetComponent<Button>();

        [SerializeField] private List<GameObject> _toggleActiveObjects;

        void Start()
        {
            _button.onClick.AddListener((() =>
            {
                foreach (GameObject o in _toggleActiveObjects)
                    o.SetActive(!o.active);
            }));
        }
    }
}