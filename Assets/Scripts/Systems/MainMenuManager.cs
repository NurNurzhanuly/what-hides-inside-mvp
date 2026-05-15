using UnityEngine;
using System.Collections;
using Cinemachine;

public class MainMenuManager : MonoBehaviour
{
    [Header("== DEBUG SETTINGS ==")]
    [Tooltip("ЕСЛИ ГАЛОЧКА СТОИТ: Меню не появится, игрок сразу стоит на ногах и может бегать (для быстрого теста уровней).")]
    public bool skipMenuForTesting = false;

    [Space(10)]
    [Header("Элементы UI")]
    public CanvasGroup menuCanvasGroup;       // Весь холст меню
    public GameObject menuFrameImage;         // Отдельная фоновая рамка кнопок
    public GameObject settingsPanel;          // Панель настроек
    public float fadeDuration = 2f;

    [Header("Настройки Камер")]
    public CinemachineVirtualCamera menuCam;

    [Header("Настройки Игрока")]
    public PlayerMovement playerMovement;
    public float lyingDownAngle = -90f;
    public float standUpDuration = 1.5f;

    private Rigidbody2D _playerRb;

    void Start()
    {
        // ==========================================
        // РЕЖИМ РАЗРАБОТЧИКА (БЫСТРЫЙ ТЕСТ)
        // ==========================================
        if (skipMenuForTesting)
        {
            Debug.Log("<color=yellow>ВНИМАНИЕ: Включен режим разработчика (Skip Menu). Меню пропущено.</color>");
            
            // Выключаем UI
            if (menuCanvasGroup != null) menuCanvasGroup.gameObject.SetActive(false);
            
            // Сбрасываем приоритет камеры меню, чтобы сразу работала игровая
            if (menuCam != null) menuCam.Priority = 0;
            
            // Убеждаемся, что игрок стоит прямо и им можно управлять
            if (playerMovement != null)
            {
                playerMovement.transform.rotation = Quaternion.Euler(0, 0, 0);
                playerMovement.enabled = true;
            }
            
            return; // ОБРЫВАЕМ функцию Start! Дальше код меню не запустится.
        }

        // ==========================================
        // НОРМАЛЬНЫЙ РЕЖИМ ИГРЫ (МЕНЮ)
        // ==========================================
        if (menuFrameImage != null) menuFrameImage.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        if (menuCam != null) menuCam.Priority = 20;

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
            _playerRb = playerMovement.GetComponent<Rigidbody2D>();
            
            if (_playerRb != null)
            {
                _playerRb.freezeRotation = true;
                playerMovement.transform.rotation = Quaternion.Euler(0, 0, lyingDownAngle);
            }
        }
    }

    // ==========================================
    // КНОПКА: NEW GAME
    // ==========================================
    public void OnPlayClicked()
    {
        StartCoroutine(StartGameRoutine());
    }

    private IEnumerator StartGameRoutine()
    {
        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.interactable = false;
            menuCanvasGroup.blocksRaycasts = false;
        }

        if (menuCam != null) menuCam.Priority = 0;

        StartCoroutine(StandUpPlayer());

        if (menuCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                menuCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                yield return null;
            }
            menuCanvasGroup.alpha = 0f;
            menuCanvasGroup.gameObject.SetActive(false);
        }
    }

    private IEnumerator StandUpPlayer()
    {
        if (playerMovement == null) yield break;

        Transform playerTransform = playerMovement.transform;
        float startAngle = playerTransform.eulerAngles.z;
        if (startAngle > 180) startAngle -= 360;

        float elapsed = 0f;
        while (elapsed < standUpDuration)
        {
            elapsed += Time.deltaTime;
            float newAngle = Mathf.Lerp(startAngle, 0f, elapsed / standUpDuration);
            playerTransform.rotation = Quaternion.Euler(0, 0, newAngle);
            yield return null;
        }

        playerTransform.rotation = Quaternion.Euler(0, 0, 0);
        playerMovement.enabled = true;
    }

    // ==========================================
    // КНОПКИ НАСТРОЕК (SETTINGS / BACK)
    // ==========================================
    public void OnSettingsClicked()
    {
        if (menuFrameImage != null) menuFrameImage.SetActive(false); 
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void OnBackClicked()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (menuFrameImage != null) menuFrameImage.SetActive(true); 
    }

    // ==========================================
    // КНОПКА: EXIT
    // ==========================================
    public void OnExitClicked()
    {
        Debug.Log("Игрок нажал EXIT. Выход из игры...");
        Application.Quit();
    }
}