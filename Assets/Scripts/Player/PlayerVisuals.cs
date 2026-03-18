using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    private SpriteRenderer _sr;

    void Awake() => _sr = GetComponent<SpriteRenderer>();

    public void UpdateAppearance(float healthPercent)
    {
        _sr.color = Color.Lerp(Color.red, Color.black, healthPercent);
    }
}