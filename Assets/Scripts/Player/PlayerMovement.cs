using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 7f;
    public float jumpForce = 12f;
    public LayerMask groundLayer;

    private Rigidbody2D _rb;
    private BoxCollider2D _coll;
    private IInputProvider _input;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _coll = GetComponent<BoxCollider2D>();
        _input = GetComponent<IInputProvider>();
        
        _rb.gravityScale = 3.5f; 
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Update()
    {
        if (_input != null && _input.IsJumpPressed() && IsGrounded())
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
        }
    }

    void FixedUpdate()
    {
        if (_input == null) return;

        float moveX = _input.GetHorizontalInput();
        float targetSpeed = moveX * moveSpeed;
        
        float accel = IsGrounded() ? 12f : 4f;
        float newX = Mathf.Lerp(_rb.linearVelocity.x, targetSpeed, accel * Time.fixedDeltaTime);
        
        _rb.linearVelocity = new Vector2(newX, _rb.linearVelocity.y);
    }

    private bool IsGrounded()
    {
        return Physics2D.BoxCast(_coll.bounds.center, _coll.bounds.size, 0f, Vector2.down, 0.1f, groundLayer);
    }
}