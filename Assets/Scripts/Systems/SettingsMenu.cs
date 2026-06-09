using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
    public GameObject settingsCanvas; 

    [Header("Ссылки на менеджеры")]
    public MainMenuManager mainMenuManager;
    public PauseManager pauseManager;

    public void OpenSettings()
    {
        if (settingsCanvas != null) settingsCanvas.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsCanvas != null) settingsCanvas.SetActive(false);


        if (mainMenuManager != null && mainMenuManager.gameObject.activeInHierarchy)
        {
            mainMenuManager.ShowMenuButtons();
        }


        if (pauseManager != null && pauseManager.pauseCanvas.activeInHierarchy)
        {
            pauseManager.ShowPauseButtons();
        }
    }
}