using UnityEngine;
using System.Collections;
using Cinemachine;

public class MainMenuManager : MonoBehaviour
{
    [Header("Элементы UI")]
    public CanvasGroup menuCanvasGroup;       // Весь холст меню (для плавного растворения при старте)
    
    [Tooltip("Перетащи сюда FrameImage (родительский объект для рамки и кнопок)")]
    public GameObject menuFrameImage;         // Главный блок меню
    
    [Tooltip("Перетащи сюда SettingsPanel")]
    public GameObject settingsPanel;          // Окно настроек
    
    public float fadeDuration = 2f;           // Скорость растворения меню при старте

    [Header("Настройки Камер")]
    public CinemachineVirtualCamera menuCam;

    [Header("Настройки Игрока")]
    public PlayerMovement playerMovement;
    public float lyingDownAngle = -90f;
    public float standUpDuration = 1.5f;

    private Rigidbody2D _playerRb;

    void Start()
    {
        // 1. Включаем Главное Меню, прячем Настройки
        if (menuFrameImage != null) menuFrameImage.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        // 2. Активируем камеру меню
        if (menuCam != null) menuCam.Priority = 20;

        // 3. Укладываем игрока на землю и отбираем управление
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

    // КНОПКА: NEW GAME
    public void OnPlayClicked()
    {
        StartCoroutine(StartGameRoutine());
    }

    private IEnumerator StartGameRoutine()
    {
        // Блокируем клики, чтобы игрок не нажал ничего в процессе
        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.interactable = false;
            menuCanvasGroup.blocksRaycasts = false;
        }

        // Камера плавно летит к игроку
        if (menuCam != null) menuCam.Priority = 0;

        // Игрок начинает вставать
        StartCoroutine(StandUpPlayer());

        // Меню плавно растворяется
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
            menuCanvasGroup.gameObject.SetActive(false); // Выключаем Canvas для оптимизации
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
        playerMovement.enabled = true; // Возвращаем управление!
    }

    // КНОПКИ НАСТРОЕК (SETTINGS / BACK)
    public void OnSettingsClicked()
    {
        // Прячем главный блок (Рамку вместе с кнопками внутри)
        if (menuFrameImage != null) menuFrameImage.SetActive(false); 
        
        // Показываем панель настроек
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void OnBackClicked()
    {
        // Прячем панель настроек
        if (settingsPanel != null) settingsPanel.SetActive(false);
        
        // Возвращаем главный блок (Рамку с кнопками)
        if (menuFrameImage != null) menuFrameImage.SetActive(true); 
    }

    // КНОПКА: EXIT
    public void OnExitClicked()
    {
        Debug.Log("Игрок нажал EXIT. Выход из игры...");
        Application.Quit();
    }
}