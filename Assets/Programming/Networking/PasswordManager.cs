using System.Collections;
using System.Collections.Generic;
using CoreCraft.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoreCraft.Networking
{
    public class PasswordManager : Singleton<PasswordManager>
    {
        [SerializeField] private TMP_InputField _passwordInputField;
        [SerializeField] private Toggle _showPasswordToggle;

        private string _password;

        void Start()
        {
            _showPasswordToggle.onValueChanged.AddListener(toggleState =>
            {
                _passwordInputField.contentType = toggleState ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;

                _passwordInputField.ForceLabelUpdate();
            });
            _passwordInputField.onValueChanged.AddListener(enterPassword =>
            {
                _password = _passwordInputField.text;
            });
        }

        public string GetPassword()
        {
            return _password;
        }
    }
}
