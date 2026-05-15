using UnityEngine;
using System.Collections;
using Cinemachine;

public class MainMenuManager : MonoBehaviour
{
    [Header("Элементы UI")]
    public CanvasGroup menuCanvasGroup;       // Весь холст меню (для затухания)
    public GameObject mainButtonsPanel;       // Контейнер с кнопками (MenuContainer)
    public GameObject menuFrameImage;         // Отдельная фоновая рамка кнопок (FrameImage)
    public GameObject settingsPanel;          // Панель настроек (SettingsPanel)
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
        // 1. Убеждаемся, что включено именно Главное Меню и его рамка, а настройки скрыты
        if (mainButtonsPanel != null) mainButtonsPanel.SetActive(true);
        if (menuFrameImage != null) menuFrameImage.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        // 2. Включаем камеру меню
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
        menuCanvasGroup.interactable = false;
        menuCanvasGroup.blocksRaycasts = false;

        // Камера летит к игроку
        if (menuCam != null) menuCam.Priority = 0;

        // Игрок встает
        StartCoroutine(StandUpPlayer());

        // Меню растворяется
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            menuCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }
        menuCanvasGroup.alpha = 0f;

        // Выключаем UI для оптимизации
        menuCanvasGroup.gameObject.SetActive(false);
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

    // КНОПКИ НАСТРОЕК (SETTINGS / BACK)
    public void OnSettingsClicked()
    {
        if (mainButtonsPanel != null) mainButtonsPanel.SetActive(false);
        if (menuFrameImage != null) menuFrameImage.SetActive(false); // Прячем рамку
        
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void OnBackClicked()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        
        if (mainButtonsPanel != null) mainButtonsPanel.SetActive(true);
        if (menuFrameImage != null) menuFrameImage.SetActive(true); // Возвращаем рамку
    }

    // КНОПКА: EXIT
    public void OnExitClicked()
    {
        Debug.Log("Игрок нажал EXIT. Выход из игры...");
        Application.Quit();
    }
}