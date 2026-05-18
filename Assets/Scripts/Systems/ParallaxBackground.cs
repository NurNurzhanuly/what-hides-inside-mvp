using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ParallaxBackground : MonoBehaviour
{
    [Header("Настройки")]
    [Tooltip("Ссылка на главную камеру (Main Camera)")]
    public Transform cameraTransform;
    
    [Tooltip("Скорость смещения. 0 - стоит на месте, 1 - двигается как фон")]
    [Range(0f, 1f)]
    public float parallaxEffectX = 0.5f;
    [Range(0f, 1f)]
    public float parallaxEffectY = 0.1f;

    private Vector3 _lastCameraPosition;
    private Material _materialInstance;

    void Start()
    {
        if (cameraTransform == null) 
        {
            cameraTransform = Camera.main.transform;
        }

        _lastCameraPosition = cameraTransform.position;
        
        // Создаем ИНСТАНС материала, чтобы двигать текстуру только на этом объекте
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        _materialInstance = sr.material; 
    }

    void LateUpdate()
    {
        if (cameraTransform == null) return;

        // Вычисляем, насколько сдвинулась камера
        Vector3 deltaMovement = cameraTransform.position - _lastCameraPosition;

        // Вычисляем смещение текстуры (с учетом масштаба самого объекта)
        float offsetX = (deltaMovement.x * parallaxEffectX) / transform.localScale.x;
        float offsetY = (deltaMovement.y * parallaxEffectY) / transform.localScale.y;

        // Сдвигаем текстуру внутри спрайта
        _materialInstance.mainTextureOffset += new Vector2(offsetX, offsetY);

        // Двигаем сам объект фона вслед за камерой, чтобы он всегда был перед глазами
        transform.position = new Vector3(cameraTransform.position.x, cameraTransform.position.y, transform.position.z);

        _lastCameraPosition = cameraTransform.position;
    }
}