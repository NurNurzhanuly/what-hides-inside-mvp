using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FlickeringLight : MonoBehaviour
{
    private Light2D _light;
    public float minIntensity = 0.4f;
    public float maxIntensity = 1.1f;
    public float flickerChance = 0.08f;

    void Awake() => _light = GetComponent<Light2D>();

    void Update()
    {
        if (Random.value < flickerChance)
        {
            _light.intensity = Random.Range(minIntensity, maxIntensity);
        }
    }
}