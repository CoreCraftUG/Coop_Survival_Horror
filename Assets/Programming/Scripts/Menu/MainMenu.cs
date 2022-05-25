using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CoreCraft.Programming.Menu
{
    public class MainMenu : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _startGameButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _quitButton;

        [Header("Menus")]
        [SerializeField] private GameObject _settingsMenu;

        // Start is called before the first frame update
        void Start()
        {
            AssignButtonEvents();
        }

        private void Settings()
        {
            _settingsMenu.SetActive(true);
        }

        // Resume Game
        private void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
        }

        // Main Menu
        private void StartGame()
        {
            gameObject.SetActive(false);
            SceneManager.LoadScene("test_level");
        }

        // Assign Button Events
        private void AssignButtonEvents()
        {
            _startGameButton.onClick.AddListener(StartGame);
            _settingsButton.onClick.AddListener(Settings);
            _quitButton.onClick.AddListener(Quit);
        }
    }
}