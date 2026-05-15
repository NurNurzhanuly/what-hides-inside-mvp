using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Collider2D))]
public class SmartLightTrigger : MonoBehaviour
{
    [Header("Настройки")]
    public Volume globalVolume;
    public float darkExposure = -3f;
    public float normalExposure = 0f;
    
    [Header("Эффект ослепления")]
    [Tooltip("Насколько сильно ослепляет при выходе из темноты (например, 3)")]
    public float exitExposure = 3f;
    [Tooltip("Скорость вспышки (чем больше, тем быстрее ударит по глазам)")]
    public float flashSpeed = 8f;
    [Tooltip("Скорость возврата к нормальному свету после вспышки")]
    public float recoverySpeed = 1.5f;

    [Header("Скорость обычного перехода")]
    [Tooltip("Скорость затухания, когда заходишь в пещеру")]
    public float transitionSpeed = 2f;

    [Header("Ориентация пещеры")]
    [Tooltip("Поставь галочку, если темнота начинается СПРАВА от этого триггера. Сними, если темнота СЛЕВА.")]
    public bool isCaveToTheRight = true;

    private ColorAdjustments _colorAdjustments;
    private static Coroutine _activeTransition; // Общая корутина для всех триггеров
    private static MonoBehaviour _coroutineOwner;

    private void Start()
    {
        if (globalVolume != null)
            globalVolume.profile.TryGet(out _colorAdjustments);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && _colorAdjustments != null)
        {
            // Определяем, куда шагнул игрок
            bool playerIsRight = collision.transform.position.x > transform.position.x;
            bool isInsideCave = isCaveToTheRight ? playerIsRight : !playerIsRight;

            // Если уже идет какая-то смена света - обрываем её
            if (_activeTransition != null && _coroutineOwner != null)
            {
                _coroutineOwner.StopCoroutine(_activeTransition);
            }

            _coroutineOwner = this;

            if (isInsideCave)
            {
                // Зашел в пещеру - просто плавно темнеет
                Debug.Log("Шаг в ТЕМНОТУ");
                _activeTransition = StartCoroutine(FadeTo(darkExposure, transitionSpeed));
            }
            else
            {
                // Вышел из пещеры - ОСЛЕПЛЕНИЕ!
                Debug.Log("Шаг НА СВЕТ (Ослепление)");
                // Проверяем: если игрок уже был на свету (или почти на свету), не надо его слепить снова.
                // Слепим, только если он реально вылез из темноты.
                if (_colorAdjustments.postExposure.value < normalExposure - 1f)
                {
                    _activeTransition = StartCoroutine(FlashAndRecover());
                }
                else
                {
                    // Если он просто дернулся на выходе туда-сюда, просто плавно возвращаем свет
                    _activeTransition = StartCoroutine(FadeTo(normalExposure, transitionSpeed));
                }
            }
        }
    }

    // Обычный плавный переход (в темноту)
    private IEnumerator FadeTo(float target, float speed)
    {
        float currentExposure = _colorAdjustments.postExposure.value;

        while (Mathf.Abs(currentExposure - target) > 0.05f)
        {
            currentExposure = Mathf.Lerp(currentExposure, target, speed * Time.deltaTime);
            _colorAdjustments.postExposure.value = currentExposure;
            yield return null;
        }

        _colorAdjustments.postExposure.value = target;
        _activeTransition = null;
    }

    // Спец-эффект: Ослепление и восстановление
    private IEnumerator FlashAndRecover()
    {
        float currentExposure = _colorAdjustments.postExposure.value;

        // Фаза 1: Резкая вспышка ослепления
        while (Mathf.Abs(currentExposure - exitExposure) > 0.1f)
        {
            currentExposure = Mathf.Lerp(currentExposure, exitExposure, flashSpeed * Time.deltaTime);
            _colorAdjustments.postExposure.value = currentExposure;
            yield return null;
        }

        // Фаза 2: Плавное привыкание глаз (возврат к норме)
        while (Mathf.Abs(currentExposure - normalExposure) > 0.05f)
        {
            currentExposure = Mathf.Lerp(currentExposure, normalExposure, recoverySpeed * Time.deltaTime);
            _colorAdjustments.postExposure.value = currentExposure;
            yield return null;
        }

        _colorAdjustments.postExposure.value = normalExposure;
        _activeTransition = null;
    }
}