using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float jumpForce = 12f;
    public LayerMask groundLayer;
    
    private Rigidbody2D _rb;
    private BoxCollider2D _coll;
    private IInputProvider _input;
    private bool _isDragging = false;
    private float _moveX;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _coll = GetComponent<BoxCollider2D>();
        _input = GetComponent<IInputProvider>();
        
        _rb.gravityScale = 4f; 
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void Update()
    {
        if (_input == null) return;

        _moveX = _input.GetHorizontalInput();

        if (_input.IsJumpPressed() && IsGrounded() && !_isDragging)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
        }
    }

    void FixedUpdate()
    {
        float speed = _isDragging ? moveSpeed * 0.5f : moveSpeed;
        _rb.linearVelocity = new Vector2(_moveX * speed, _rb.linearVelocity.y);
    }

    public void SetDragging(bool state)
    {
        _isDragging = state;
    }

    private bool IsGrounded()
    {
        return Physics2D.BoxCast(_coll.bounds.center, _coll.bounds.size, 0f, Vector2.down, 0.1f, groundLayer);
    }
}