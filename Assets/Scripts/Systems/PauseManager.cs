using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("Элементы UI")]
    public GameObject pauseCanvas;      // Холст паузы
    public GameObject pauseButtons;     // Контейнер с кнопками (Resume, Settings, MainMenu)

    private bool _isPaused = false;
    private IInputProvider _input;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _input = player.GetComponent<IInputProvider>();
        }

        if (pauseCanvas != null) pauseCanvas.SetActive(false);
    }

    void Update()
    {
        // Нажатие Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Если меню настроек открыто, Esc не должен снимать паузу!
            // Он должен закрыть настройки. Этим займется скрипт SettingsMenu.
            // Поэтому тут мы просто ставим на паузу или снимаем её, если мы в главном окне паузы.
            
            if (_isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        _isPaused = true;
        Time.timeScale = 0f; 
        
        if (pauseCanvas != null) pauseCanvas.SetActive(true);
        if (pauseButtons != null) pauseButtons.SetActive(true);
    }

    public void ResumeGame()
    {
        _isPaused = false;
        Time.timeScale = 1f; 
        
        if (pauseCanvas != null) pauseCanvas.SetActive(false);
    }

    // Прячет кнопки паузы, когда мы открываем Настройки
    public void OnSettingsClicked()
    {
        if (pauseButtons != null) pauseButtons.SetActive(false);
    }

    // Эту функцию (возврат кнопок паузы) теперь должен вызывать скрипт SettingsMenu!
    public void ShowPauseButtons()
    {
        if (pauseButtons != null) pauseButtons.SetActive(true);
    }

    public void OnMainMenuClicked()
    {
        Time.timeScale = 1f; 
        if (SaveManager.Instance != null) SaveManager.Instance.isRespawning = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}