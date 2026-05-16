using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    public Animator animator;
    public Rigidbody2D rb;
    public PlayerMovement movement;
    private Vector3 _initialScale;

    void Awake()
    {
        // Ищем всё прямо на самом игроке
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (movement == null) movement = GetComponent<PlayerMovement>();
        if (animator == null) animator = GetComponent<Animator>();
        
        _initialScale = transform.localScale;
    }

    void Update()
    {
        if (rb == null || animator == null) return;

        // 1. Считаем скорость и отдаем Аниматору
        float currentSpeed = Mathf.Abs(rb.linearVelocity.x);
        animator.SetFloat("Speed", currentSpeed);

        if (movement != null)
        {
            animator.SetBool("IsGrounded", movement.IsGrounded());
        }

        // 2. Поворачиваем персонажа туда, куда он бежит
        bool isDragging = GetComponent<FixedJoint2D>() != null;
        if (currentSpeed > 0.1f && !isDragging)
        {
            float dir = Mathf.Sign(rb.linearVelocity.x);
            transform.localScale = new Vector3(dir * Mathf.Abs(_initialScale.x), _initialScale.y, _initialScale.z);
        }
    }
}