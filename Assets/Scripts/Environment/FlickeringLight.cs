using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FlickeringLight : MonoBehaviour
{
    private Light2D _light;
    public float minIntensity = 0.6f;
    public float maxIntensity = 1.2f;
    [Range(0.01f, 0.2f)] public float flickerSpeed = 0.05f;

    void Start() => _light = GetComponent<Light2D>();

    void Update()
    {
        float targetIntensity = Random.Range(minIntensity, maxIntensity);
        _light.intensity = Mathf.MoveTowards(_light.intensity, targetIntensity, flickerSpeed);
    }
}