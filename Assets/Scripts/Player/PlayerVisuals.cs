using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    private Rigidbody2D _rb;
    private Vector3 _initialScale;

    void Awake()
    {
        _rb = GetComponentInParent<Rigidbody2D>();
        _initialScale = transform.localScale;
    }

    void Update()
    {
        if (_rb == null) return;
        float speed = Mathf.Abs(_rb.linearVelocity.x);
        if (speed > 0.1f)
        {
            float dir = Mathf.Sign(_rb.linearVelocity.x);
            transform.localScale = new Vector3(dir * _initialScale.x, _initialScale.y, _initialScale.z);
        }
    }
}