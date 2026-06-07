using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CaveLightController : MonoBehaviour
{
    public static CaveLightController Instance { get; private set; }

    [Header("Volume")]
    public Volume globalVolume;

    [Header("Уровни экспозиции")]
    public float normalExposure = 0f;
    public float darkExposure = -3f;
    public float exitExposure = 3f;

    [Header("Длительности переходов (сек)")]
    public float fadeToDarkDuration = 1.5f;
    public float fadeToNormalDuration = 1.0f;
    public float flashDuration = 0.15f;
    public float recoverDuration = 1.5f;

    private Coroutine _routine;

    public bool IsInDark { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        if (globalVolume == null)
        {
            Debug.LogError("[CaveLight] globalVolume НЕ назначен!");
            return;
        }
        // profile (геттер) возвращает рантайм-инстанс, к которому привязан рендер
        if (globalVolume.profile.TryGet(out ColorAdjustments ca))
            Debug.Log("[CaveLight] ColorAdjustments найден, всё ок.");
        else
            Debug.LogError("[CaveLight] В профиле Volume нет Color Adjustments! Добавь override и включи Post Exposure.");
    }

    // Берём ColorAdjustments из актуального профиля КАЖДЫЙ раз
    private ColorAdjustments GetCA()
    {
        if (globalVolume != null && globalVolume.profile.TryGet(out ColorAdjustments ca))
            return ca;
        return null;
    }

    private float Current
    {
        get { var ca = GetCA(); return ca != null ? ca.postExposure.value : 0f; }
    }

    private void SetExposure(float v)
    {
        var ca = GetCA();
        if (ca != null) ca.postExposure.value = v;
    }

    public void EnterDark()
    {
        if (IsInDark) return;
        IsInDark = true;
        StartRoutine(FadeRoutine(darkExposure, fadeToDarkDuration));
    }

    public void ExitToLight()
    {
        if (!IsInDark) return;
        IsInDark = false;
        StartRoutine(FlashRoutine());
    }

    public void ApplyStateInstant(bool inDark)
    {
        IsInDark = inDark;
        StopActive();
        SetExposure(inDark ? darkExposure : normalExposure);
    }

    private void StartRoutine(IEnumerator r)
    {
        StopActive();
        _routine = StartCoroutine(r);
    }

    private void StopActive()
    {
        if (_routine != null) { StopCoroutine(_routine); _routine = null; }
    }

    private IEnumerator FadeRoutine(float target, float duration)
    {
        float start = Current;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            SetExposure(Mathf.Lerp(start, target, t / duration));
            yield return null;
        }
        SetExposure(target);
        _routine = null;
    }

    private IEnumerator FlashRoutine()
    {
        float start = Current;
        float t = 0f;
        while (t < flashDuration)
        {
            t += Time.deltaTime;
            SetExposure(Mathf.Lerp(start, exitExposure, t / flashDuration));
            yield return null;
        }
        SetExposure(exitExposure);

        t = 0f;
        while (t < recoverDuration)
        {
            t += Time.deltaTime;
            SetExposure(Mathf.Lerp(exitExposure, normalExposure, t / recoverDuration));
            yield return null;
        }
        SetExposure(normalExposure);
        _routine = null;
    }
}