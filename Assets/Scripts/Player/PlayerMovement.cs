using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float climbSpeed = 5f;
    public float jumpForce = 12f;
    public LayerMask groundLayer;
    
    private Rigidbody2D _rb;
    private BoxCollider2D _coll;
    private IInputProvider _input;
    private bool _isDragging = false;
    private bool _isOnLadder = false;
    private float _moveX;
    private float _defaultGravity;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _coll = GetComponent<BoxCollider2D>();
        _input = GetComponent<IInputProvider>();
        
        _defaultGravity = 4f;
        _rb.gravityScale = _defaultGravity; 
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void Update()
    {
        if (_input == null) return;

        _moveX = _input.GetHorizontalInput();

        if (_input.IsJumpPressed() && !_isDragging)
        {
            if (IsGrounded() || _isOnLadder)
            {
                if (_isOnLadder) SetOnLadder(false);
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
            }
        }
    }

    void FixedUpdate()
    {
        if (_input == null) return;

        if (_isOnLadder)
        {
            float v = _input.GetVerticalInput();
            _rb.linearVelocity = new Vector2(_moveX * moveSpeed * 0.5f, v * climbSpeed);
        }
        else
        {
            float speed = _isDragging ? moveSpeed * 0.5f : moveSpeed;
            _rb.linearVelocity = new Vector2(_moveX * speed, _rb.linearVelocity.y);
        }
    }

    public void SetOnLadder(bool state)
    {
        _isOnLadder = state;
        _rb.gravityScale = state ? 0 : _defaultGravity;
        if (state) _rb.linearVelocity = Vector2.zero;
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