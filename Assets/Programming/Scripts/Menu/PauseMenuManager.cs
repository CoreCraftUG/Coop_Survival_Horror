using System;
using System.Collections.Generic;
using CoreCraft.Programming.GameSettings;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace CoreCraft.Programming.Menu
{
    public class PauseMenuManager : MonoBehaviour
    {
        [Header("Scriptable Objects")]
        [SerializeField] private SO_Settings _sOSettings;

        [Header("Menus")]
        [SerializeField] private GameObject _pauseMenu;
        [SerializeField] private GameObject _settingsMenu;
        [SerializeField] private List<PopUpButton> _popUps;

        [Header("Input Action Maps")] [SerializeField]
        private PlayerInput _playerInput;

        private void Awake()
        {
            //_playerInput = GetComponent<PlayerInput>();
        }

        private void CloseMenu()
        {
            bool popUp = false;

            foreach (PopUpButton item in _popUps)
            {
                if (!item.PopUp.activeSelf)
                    return;

                item.DeclineButton.onClick.Invoke();
                popUp = true;
            }

            if (!popUp)
            {
                if (_settingsMenu.activeSelf)
                {
                    // When the user exits the settings menu, save.
                    _sOSettings.SaveDataToDisk();
                    _settingsMenu.SetActive(false);
                }
                else
                    _pauseMenu.SetActive(false);
            }
        }

        public void OpenMenu()
        {
            if (_pauseMenu.activeSelf)
            {
                _playerInput.SwitchCurrentActionMap("PlayerInputActionMap");
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                _pauseMenu.SetActive(false);
                Time.timeScale = 1f;
            }
            else
            {
                _playerInput.SwitchCurrentActionMap("MenuInputAction");
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                _pauseMenu.SetActive(true);
                Time.timeScale = 0f;
            }
        }
    }

    [Serializable]
    public class PopUpButton
    {
        public GameObject PopUp;
        public Button DeclineButton;
    }
}