using System.Collections.Generic;
using CoreCraft.Programming.GameSettings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoreCraft.Programming.Menu
{
    public class SettingsMenuManager : MonoBehaviour
    {
        [Header("Scriptable Objects")]
        [SerializeField] private SO_Settings _sOSettings;

        [Header("Settings Buttons")]
        [SerializeField] private Button _gameButton;
        [SerializeField] private Button _videoButton;
        [SerializeField] private Button _interfaceButton;
        [SerializeField] private Button _audioButton;
        [SerializeField] private Button _closeButton;

        [Header("Menu Game Objects")]
        [SerializeField] private GameObject _menuGame;
        [SerializeField] private GameObject _menuVideo;
        [SerializeField] private GameObject _menuInterface;
        [SerializeField] private GameObject _menuAudio;
        [SerializeField] private List<GameObject> _menuList;
        [SerializeField] private TextMeshProUGUI _settingsTitle;

        [Header("Menus to set active again after closing")] 
        [SerializeField] private List<GameObject> _menusToEnable;

        private GameObject _currentMenu;
        [SerializeField] private int _currentMenuIndex = 0;

        //private void OpenMenu(GameObject menu)
        //{
        //    _currentMenu.SetActive(false);
        //    menu.SetActive(true);
        //    _currentMenu = menu;
        //}

        private void OpenMenu(int index)
        {
            _currentMenu.SetActive(false);
            _menuList[index].SetActive(true);
            _currentMenu = _menuList[index];
            _currentMenuIndex = index;
            _settingsTitle.text = _menuList[index].GetComponent<MenuTitleDescription>().Title;
        }

        private void CloseMenu()
        {
            if (_menusToEnable.Count > 0)
            {
                foreach (GameObject item in _menusToEnable)
                {
                    item.SetActive(true);
                }
            }

            _sOSettings.SaveDataToDisk();
            gameObject.SetActive(false);
        }

        public void NextMenuTab()
        {
            if (_currentMenuIndex == 3)
            {
                Debug.Log($"The current index: {_currentMenuIndex} is the maximum of the list.");
            }
            else
            {
                _currentMenuIndex++;
                OpenMenu(_currentMenuIndex);
                Debug.Log($"Current index: {_currentMenuIndex} = {_menuList[_currentMenuIndex].gameObject.name}.");
            }
        }

        public void PreviousMenuTab()
        {
            if (_currentMenuIndex != 0)
            {
                _currentMenuIndex--;
                OpenMenu(_currentMenuIndex);
                Debug.Log($"Current index: {_currentMenuIndex} = {_menuList[_currentMenuIndex].gameObject.name}.");
            }
            else
                Debug.Log($"The current index: {_currentMenuIndex} is the minimum of the list.");
        }

        private void AssingButtonEvents()
        {
            _gameButton.onClick.AddListener(delegate { OpenMenu(0); });
            _videoButton.onClick.AddListener(delegate { OpenMenu(1); });
            _interfaceButton.onClick.AddListener(delegate { OpenMenu(2); });
            _audioButton.onClick.AddListener(delegate { OpenMenu(3); });
            _closeButton.onClick.AddListener(delegate { CloseMenu(); });
        }

        private void Start()
        {
            _currentMenu = _menuGame.activeSelf ? _menuGame :
                _menuVideo.activeSelf ? _menuVideo :
                _menuInterface.activeSelf ? _menuInterface :
                _menuAudio.activeSelf ? _menuAudio : null;

            AssingButtonEvents();

            if (_currentMenu == _menuGame)
                _gameButton.onClick.Invoke();
        }
    }
}