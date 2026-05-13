using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    private Rigidbody2D _rb;
    private Vector3 _initialScale;
    private FixedJoint2D _joint; 

    void Awake()
    {
        _rb = GetComponentInParent<Rigidbody2D>();
        _initialScale = transform.localScale;
    }

    void Update()
    {
        if (_rb == null) return;
        
        // Проверяем, держим ли мы сейчас ящик
        _joint = GetComponentInParent<FixedJoint2D>();

        float speed = Mathf.Abs(_rb.linearVelocity.x);
        
        // ПОВОРАЧИВАЕМСЯ ТОЛЬКО ЕСЛИ НИЧЕГО НЕ ДЕРЖИМ (_joint == null)
        if (speed > 0.1f && _joint == null)
        {
            float dir = Mathf.Sign(_rb.linearVelocity.x);
            transform.localScale = new Vector3(dir * _initialScale.x, _initialScale.y, _initialScale.z);
        }
    }
}