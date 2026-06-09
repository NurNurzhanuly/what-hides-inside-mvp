using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("Элементы UI")]
    public GameObject pauseCanvas;      
    public GameObject pauseButtons;   

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

        if (Input.GetKeyDown(KeyCode.Escape))
        {
           
            
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


    public void OnSettingsClicked()
    {
        if (pauseButtons != null) pauseButtons.SetActive(false);
    }


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