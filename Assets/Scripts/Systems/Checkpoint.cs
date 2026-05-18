using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    private bool _isActivated = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_isActivated && collision.CompareTag("Player"))
        {
            _isActivated = true;
            
            float currentExposure = 0f; // По умолчанию свет нормальный (0)

            // Пытаемся найти Global Volume на сцене, чтобы узнать, насколько сейчас темно
            Volume globalVolume = Object.FindFirstObjectByType<Volume>();
            if (globalVolume != null && globalVolume.profile.TryGet(out ColorAdjustments colorAdjustments))
            {
                currentExposure = colorAdjustments.postExposure.value; // Читаем текущую темноту экрана!
            }

            // Передаем в Менеджер и позицию, и СВЕТ!
            SaveManager.Instance.SaveCheckpoint(transform.position, currentExposure);
        }
    }
}