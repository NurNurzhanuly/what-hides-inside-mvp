using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

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

    [Header("Настройки Камер и Света")]
    public CinemachineVirtualCamera menuCam;
    public Volume globalVolume;

    [Header("Настройки Игрока")]
    public PlayerMovement playerMovement;
    public float lyingDownAngle = -90f;
    public float standUpDuration = 1.5f;

    private Rigidbody2D _playerRb;

    void Start()
    {
        bool isRespawningNow = SaveManager.Instance != null && SaveManager.Instance.isRespawning;

        if (skipMenuForTesting || isRespawningNow)
        {
            if (isRespawningNow) SaveManager.Instance.isRespawning = false;

            if (menuCanvasGroup != null) menuCanvasGroup.gameObject.SetActive(false);
            if (menuFrameImage != null) menuFrameImage.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);

            if (menuCam != null) menuCam.Priority = 0;

            if (playerMovement != null)
            {
                if (isRespawningNow && SaveManager.Instance.HasSavedGame())
                {
                    playerMovement.transform.position = SaveManager.Instance.LoadCheckpointPosition(playerMovement.transform.position);
                    StartCoroutine(RestoreSavedLightRoutine());
                }

                playerMovement.transform.rotation = Quaternion.Euler(0, 0, 0);
                playerMovement.enabled = true;
            }

            if (isRespawningNow)
            {
                StartCoroutine(FadeInFromBlack());
            }
            return;
        }

        // НОРМАЛЬНЫЙ СТАРТ
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

    public void OnPlayClicked()
    {
        if (SaveManager.Instance != null) SaveManager.Instance.ClearSave();
        StartCoroutine(StartGameRoutine(false));
    }

    public void OnLoadClicked()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.HasSavedGame())
        {
            StartCoroutine(StartGameRoutine(true));
        }
    }

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
            playerMovement.transform.position = SaveManager.Instance.LoadCheckpointPosition(playerMovement.transform.position);
            playerMovement.transform.rotation = Quaternion.Euler(0, 0, 0);
            StartCoroutine(RestoreSavedLightRoutine());
        }
        else
        {
            if (CaveLightController.Instance != null)
                CaveLightController.Instance.ApplyStateInstant(false);
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

    private IEnumerator RestoreSavedLightRoutine()
    {
        yield return null; // ждём кадр, чтобы URP/контроллер прогрузились

        // ждём, пока контроллер появится (на случай порядка инициализации)
        float timeout = 0f;
        while (CaveLightController.Instance == null && timeout < 1f)
        {
            timeout += Time.deltaTime;
            yield return null;
        }

        if (CaveLightController.Instance != null)
        {
            bool inDark = SaveManager.Instance.LoadCheckpointInDark();
            Debug.Log($"[Respawn] загружаю inDark={inDark}, контроллер есть=True");
            CaveLightController.Instance.ApplyStateInstant(inDark);
        }
        else
        {
            Debug.LogError("[Respawn] CaveLightController.Instance так и не появился!");
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

    public void OnSettingsClicked()
    {
        if (menuFrameImage != null) menuFrameImage.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void ShowMenuButtons()
    {
        if (menuFrameImage != null) menuFrameImage.SetActive(true);
    }

    public void OnExitClicked()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}