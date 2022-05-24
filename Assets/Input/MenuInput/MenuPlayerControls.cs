using CoreCraft.Programming.Menu;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuPlayerControls : MonoBehaviour
{
    [Header("Menus")]
    [SerializeField] private SettingsMenuManager _settingsMenuManager;
    [SerializeField] private PauseMenuManager _pauseMenuManager;

    public void SettingsTabLeft(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            _settingsMenuManager.PreviousMenuTab();
        }
    }

    public void SettingsTabRight(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            _settingsMenuManager.NextMenuTab();
        }
    }

    public void Escape(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            _pauseMenuManager.OpenMenu();
        }
    }
}
