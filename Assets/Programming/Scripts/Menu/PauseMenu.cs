using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CoreCraft.Programming.Menu
{
    public class PauseMenu : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _mainMenuButton;

        [Header("PopUp")]
        [SerializeField] private GameObject _popUp;
        [SerializeField] private TextMeshProUGUI _popUpText;
        [SerializeField] private Button _acceptButton;
        [SerializeField] private Button _declineButton;

        [Header("Menus")]
        [SerializeField] private GameObject _settingsMenu;
        [SerializeField] private PauseMenuManager _pauseMenuManager;

        // Handle exit game pop up
        private void HandleExitGame(string text)
        {
            _popUp.SetActive(true);
            _popUpText.text = text;
            _acceptButton.onClick.RemoveAllListeners();
            _declineButton.onClick.RemoveAllListeners();

            _acceptButton.onClick.AddListener(delegate { Application.Quit(); });
            _declineButton.onClick.AddListener(delegate { _popUp.SetActive(false); });
        }

        // Handle all pop ups
        //private void PopUp(string action)
        //{
        //    switch (action)
        //    {
        //        case "Exit":
        //            HandleExitGame();
        //    }
        //}

        // Resume Game
        private void Resume()
        {
            _pauseMenuManager.OpenMenu();
        }

        // Open Settings

        private void Settings()
        {
            _settingsMenu.SetActive(true);
        }

        // Main Menu
        private void MainMenu()
        {
            Time.timeScale = 1f;
            gameObject.SetActive(false);
            SceneManager.LoadScene("S_Menu_Testing");
        }

        // Assign Button Events
        private void AssignButtonEvents()
        {
            _resumeButton.onClick.AddListener(delegate { Resume(); });
            _settingsButton.onClick.AddListener(delegate { Settings(); });
            _mainMenuButton.onClick.AddListener(delegate { MainMenu(); });
        }

        private void Start()
        {
            AssignButtonEvents();
        }
    }
}