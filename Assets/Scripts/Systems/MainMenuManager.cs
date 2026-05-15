using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Cinemachine;

public class MainMenuManager : MonoBehaviour
{
    [Header("== DEBUG SETTINGS ==")]
    public bool skipMenuForTesting = false;

    [Space(10)]
    [Header("Элементы UI")]
    public CanvasGroup menuCanvasGroup;       
    public GameObject menuFrameImage;         
    public GameObject settingsPanel;          
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
        // Проверяем: игрок возрождается после смерти?
        bool isRespawningNow = SaveManager.Instance != null && SaveManager.Instance.isRespawning;

        // Если это быстрый тест ИЛИ мы возрождаемся -> МЕНЮ НЕ ПОКАЗЫВАЕМ!
        if (skipMenuForTesting || isRespawningNow)
        {
            Debug.Log($"[MainMenu] Быстрый старт. isRespawning: {isRespawningNow}");
            
            // 1. Сбрасываем флаг, чтобы он не мешал при следующем заходе в игру
            if (isRespawningNow) SaveManager.Instance.isRespawning = false;

            // 2. ЖЕСТКО отключаем ВСЁ меню, чтобы оно не висело на экране
            if (menuCanvasGroup != null) menuCanvasGroup.gameObject.SetActive(false);
            if (menuFrameImage != null) menuFrameImage.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            
            // 3. Камера смотрит на игрока
            if (menuCam != null) menuCam.Priority = 0;

            // 4. Настраиваем игрока
            if (playerMovement != null)
            {
                // Если возрождаемся - ставим на чекпоинт
                if (isRespawningNow && SaveManager.Instance.HasSavedGame())
                {
                    playerMovement.transform.position = SaveManager.Instance.LoadCheckpoint(playerMovement.transform.position);
                }
                
                playerMovement.transform.rotation = Quaternion.Euler(0, 0, 0);
                playerMovement.enabled = true; // Разрешаем бегать
            }

            // 5. Высветляем черный экран после смерти
            if (isRespawningNow)
            {
                StartCoroutine(FadeInFromBlack());
            }

            return; // ОБРЫВАЕМ функцию Start! Код ниже не выполнится.
        }

        // НОРМАЛЬНЫЙ СТАРТ (Игрок только запустил игру)
        Debug.Log("[MainMenu] Нормальный старт меню.");
        
        // Включаем главное меню, рамку, выключаем настройки
        if (menuCanvasGroup != null) menuCanvasGroup.gameObject.SetActive(true);
        if (menuFrameImage != null) menuFrameImage.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        
        // Включаем камеру меню
        if (menuCam != null) menuCam.Priority = 20;

        // Укладываем игрока
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
        if (SaveManager.Instance != null) SaveManager.Instance.ClearSave();
        StartCoroutine(StartGameRoutine(false));
    }

    // КНОПКА: LOAD GAME
    public void OnLoadClicked()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.HasSavedGame())
        {
            StartCoroutine(StartGameRoutine(true));
        }
        else
        {
            Debug.LogWarning("Сохранений нет!");
        }
    }

    // ОБЩАЯ КОРУТИНА СТАРТА ИЗ МЕНЮ
    private IEnumerator StartGameRoutine(bool isLoadingSave)
    {
        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.interactable = false;
            menuCanvasGroup.blocksRaycasts = false;
        }

        if (menuCam != null) menuCam.Priority = 0;

        if (isLoadingSave && playerMovement != null)
        {
            Vector2 savedPos = SaveManager.Instance.LoadCheckpoint(playerMovement.transform.position);
            playerMovement.transform.position = savedPos;
            playerMovement.transform.rotation = Quaternion.Euler(0, 0, 0); 
        }
        else
        {
            yield return StartCoroutine(StandUpPlayer());
        }

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

        if (playerMovement != null) playerMovement.enabled = true;
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
    }

    private IEnumerator FadeInFromBlack()
    {
        GameObject canvasObj = new GameObject("FadeInCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        GameObject imageObj = new GameObject("BlackScreen");
        imageObj.transform.SetParent(canvasObj.transform, false);
        Image fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 1f); 
        
        RectTransform rt = fadeImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        float timer = 0f;
        float duration = 1.5f; 

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timer / duration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        Destroy(canvasObj);
    }

    // КНОПКИ НАСТРОЕК (SETTINGS / BACK)
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

    public void OnExitClicked()
    {
        Debug.Log("Игрок нажал EXIT. Выход из игры...");
        Application.Quit();
    }
}