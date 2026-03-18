using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float jumpForce = 14f;
    public LayerMask groundLayer;

    private Rigidbody2D _rb;
    private BoxCollider2D _coll;
    private float _moveX;
    private float _baseSpeed;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _coll = GetComponent<BoxCollider2D>();
        _rb.gravityScale = 4f; 
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _baseSpeed = moveSpeed;
    }

    void Update()
    {
        _moveX = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
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