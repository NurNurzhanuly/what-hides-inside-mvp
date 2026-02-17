using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    private SpriteRenderer _sr;

    void Awake() => _sr = GetComponent<SpriteRenderer>();

    public void UpdateAppearance(float healthPercent)
    {
        _sr.color = Color.Lerp(Color.red, Color.black, healthPercent);
        
        float s = Mathf.Lerp(0.5f, 1.9f, healthPercent);
        transform.localScale = new Vector3(s, s, 1f);
    }
}