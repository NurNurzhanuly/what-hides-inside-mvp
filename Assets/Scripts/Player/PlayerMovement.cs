using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 8f;
    public float jumpForce = 14f;
    public LayerMask groundLayer;

    private Rigidbody2D _rb;
    private BoxCollider2D _coll;
    private IInputProvider _input; // Возвращаем интерфейс для диплома
    private float _moveX;
    private float _baseSpeed;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _coll = GetComponent<BoxCollider2D>();
        _input = GetComponent<IInputProvider>(); // Ищем читалку клавиш
        
        _rb.gravityScale = 4f; 
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _baseSpeed = moveSpeed;
    }

    void Update()
    {
        // Используем интерфейс, как прописано в твоей архитектуре
        if (_input != null)
        {
            _moveX = _input.GetMoveDirection().x;
            if (_input.IsJumpPressed() && IsGrounded())
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
            }
        }
    }

    void FixedUpdate()
    {
        _rb.linearVelocity = new Vector2(_moveX * moveSpeed, _rb.linearVelocity.y);
    }

    private bool IsGrounded()
    {
        return Physics2D.BoxCast(_coll.bounds.center, _coll.bounds.size, 0f, Vector2.down, 0.1f, groundLayer);
    }

    public void SetSpeedModifier(float percent)
    {
        moveSpeed = _baseSpeed * percent;
    }
}