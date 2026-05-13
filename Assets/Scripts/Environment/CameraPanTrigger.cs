using UnityEngine;
using System.Collections;
using Cinemachine;

[RequireComponent(typeof(Collider2D))]
public class CameraPanTrigger : MonoBehaviour
{
    [Header("Камера (Cinemachine)")]
    public CinemachineVirtualCamera virtualCamera;
    
    [Header("Настройки смещения")]
    [Tooltip("На сколько сместить камеру? (0 = вернуть по центру, -5 = влево, 5 = вправо)")]
    public float targetOffsetX = -5f;
    public float panDuration = 2f;

    // Защита от случайных повторных срабатываний
    private bool _hasTriggered = false;
    private CinemachineFramingTransposer _transposer;

    void Awake()
    {
        // Ищем компонент смещения внутри виртуальной камеры
        if (virtualCamera != null)
        {
            _transposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        }
        else
        {
            Debug.LogError("В скрипт не добавлена Virtual Camera!");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_hasTriggered && collision.CompareTag("Player"))
        {
            _hasTriggered = true;
            if (_transposer != null)
            {
                // Запускаем корутину плавного смещения камеры
                StartCoroutine(PanCamera());
            }
        }
    }

    private IEnumerator PanCamera()
    {
        float startX = _transposer.m_TrackedObjectOffset.x;
        float elapsed = 0f;

        while (elapsed < panDuration)
        {
            elapsed += Time.deltaTime;
            
            // Плавное сглаживание движения (SmoothStep)
            float t = elapsed / panDuration;
            t = t * t * (3f - 2f * t); 

            _transposer.m_TrackedObjectOffset.x = Mathf.Lerp(startX, targetOffsetX, t);
            yield return null;
        }
        
        // Гарантируем точное конечное значение
        _transposer.m_TrackedObjectOffset.x = targetOffsetX; 
    }
}