using UnityEngine;
using System.Collections;
using Cinemachine;

[RequireComponent(typeof(Collider2D))]
public class CameraPanTrigger : MonoBehaviour
{
    [Header("Камера (Cinemachine)")]
    public CinemachineVirtualCamera virtualCamera;
    [Tooltip("На сколько сместить камеру? (0 = вернуть по центру, -5 = влево)")]
    public float targetOffsetX = -5f;
    public float cameraPanDuration = 2f; 
    
    [Header("Какой это триггер?")]
    [Tooltip("Поставь галочку, если это НИЖНИЙ триггер (Включает сложную секвенцию с задержками)")]
    public bool isOpeningTrigger = false;
    [Tooltip("Поставь галочку, если это ВЕРХНИЙ триггер (Просто плавно гасит всё одновременно с камерой)")]
    public bool isClosingTrigger = false;

    [Header("UI Элементы")]
    public CanvasGroup mainLogo;
    public CanvasGroup subtitle;
    
    [Header("Тайминги появления (Только для Opening)")]
    public float waitBeforeLogo = 2f;     
    public float logoFadeDuration = 1.5f; 
    public float waitBeforeSubtitle = 1f; 
    public float subtitleFadeDuration = 1f;

    private bool _hasTriggered = false;
    private CinemachineFramingTransposer _transposer;

    void Awake()
    {
        if (virtualCamera != null)
        {
            _transposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_hasTriggered && collision.CompareTag("Player"))
        {
            _hasTriggered = true;
            
            if (_transposer != null)
                StartCoroutine(PanCamera());

            if (isOpeningTrigger && mainLogo != null && subtitle != null)
            {
                StartCoroutine(PlayOpeningSequence());
            }

            if (isClosingTrigger && mainLogo != null && subtitle != null)
            {
                StartCoroutine(FadeCanvasGroup(mainLogo, mainLogo.alpha, 0f, cameraPanDuration));
                StartCoroutine(FadeCanvasGroup(subtitle, subtitle.alpha, 0f, cameraPanDuration));
            }
        }
    }


    private IEnumerator PanCamera()
    {
        float startX = _transposer.m_TrackedObjectOffset.x;
        float elapsed = 0f;

        while (elapsed < cameraPanDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / cameraPanDuration;
            t = t * t * (3f - 2f * t); 

            _transposer.m_TrackedObjectOffset.x = Mathf.Lerp(startX, targetOffsetX, t);
            yield return null;
        }
        _transposer.m_TrackedObjectOffset.x = targetOffsetX; 
    }

    private IEnumerator PlayOpeningSequence()
    {
        yield return new WaitForSeconds(cameraPanDuration + waitBeforeLogo);

        yield return StartCoroutine(FadeCanvasGroup(mainLogo, 0f, 1f, logoFadeDuration));

        yield return new WaitForSeconds(waitBeforeSubtitle);

        yield return StartCoroutine(FadeCanvasGroup(subtitle, 0f, 1f, subtitleFadeDuration));
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float startAlpha, float targetAlpha, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t); 

            cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }
        cg.alpha = targetAlpha;
    }
}