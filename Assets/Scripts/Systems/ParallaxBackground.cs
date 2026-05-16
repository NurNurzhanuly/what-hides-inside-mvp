using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Header("Настройки")]
    public Transform cameraTransform;
    
    [Range(0f, 1f)]
    public float parallaxEffectX = 0.5f;
    [Range(0f, 1f)]
    public float parallaxEffectY = 0.1f;

    private Vector3 _lastCameraPosition;
    private Material _material;

    // В URP 2D Unlit шейдере текстура обычно привязана к свойству _MainTex
    // даже если в инспекторе оно называется Diffuse.
    private static readonly int MainTexProperty = Shader.PropertyToID("_MainTex");

    void Start()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }

        _lastCameraPosition = cameraTransform.position;
        
        // Получаем материал с нашего Quad
        _material = GetComponent<MeshRenderer>().material;
    }

    void LateUpdate()
    {
        Vector3 deltaMovement = cameraTransform.position - _lastCameraPosition;

        float offsetX = (deltaMovement.x * parallaxEffectX) / transform.localScale.x;
        float offsetY = (deltaMovement.y * parallaxEffectY) / transform.localScale.y;

        // Двигаем текстуру через прямое обращение к свойству
        Vector2 currentOffset = _material.GetTextureOffset(MainTexProperty);
        _material.SetTextureOffset(MainTexProperty, currentOffset + new Vector2(offsetX, offsetY));

        // Держим квад перед камерой
        transform.position = new Vector3(cameraTransform.position.x, cameraTransform.position.y, transform.position.z);

        _lastCameraPosition = cameraTransform.position;
    }
}
