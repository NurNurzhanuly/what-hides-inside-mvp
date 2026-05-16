using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
    public GameObject settingsCanvas; // Сюда перетащи сам SettingsCanvas

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

        // Возвращаем кнопки Главного Меню (если оно существует и активно)
        if (mainMenuManager != null && mainMenuManager.gameObject.activeInHierarchy)
        {
            mainMenuManager.ShowMenuButtons();
        }

        // Возвращаем кнопки Паузы (если пауза активна)
        if (pauseManager != null && pauseManager.pauseCanvas.activeInHierarchy)
        {
            pauseManager.ShowPauseButtons();
        }
    }
}