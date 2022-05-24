using UnityEngine;
using UnityEngine.UI;

namespace GameSettings
{
    public class DisableDropdownOption : MonoBehaviour
    {
        public string[] OptionsToBeDisabled;
        private Toggle _toggle;
        private string[] _itemName;

        // Start is called before the first frame update
        void Start()
        {
            _toggle = GetComponent<Toggle>();
            _itemName = _toggle.name.Split(':');
            foreach (string item in _itemName)
            {
                for (int i = 0; i < OptionsToBeDisabled.Length; i++)
                {
                    if (item.Trim() == OptionsToBeDisabled[i])
                        _toggle.interactable = false;
                }
            }
        }
    }
}