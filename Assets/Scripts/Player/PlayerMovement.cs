using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 7f;
    public float climbSpeed = 5f;
    public float jumpForce = 11f;
    public float swingForce = 45f;
    public LayerMask groundLayer;
    
    private Rigidbody2D _rb;
    private BoxCollider2D _coll;
    private IInputProvider _input;
    
    private bool _isDragging = false;
    private bool _isOnLadder = false;
    private bool _isOnRope = false;
    private Rigidbody2D _activeRopeSegment;
    private float _climbCooldown = 0f;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _coll = GetComponent<BoxCollider2D>();
        _input = GetComponent<IInputProvider>();
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Update()
    {
        if (_input == null) return;

        if (_input.IsJumpPressed())
        {
            if (IsGrounded() || _isOnLadder || _isOnRope)
            {
                if (_isOnRope) SetOnRope(false, null);
                if (_isOnLadder) SetOnLadder(false);
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
            }
        }
    }

    void FixedUpdate()
    {
        if (_input == null) return;

        float h = _input.GetHorizontalInput();
        float v = _input.GetVerticalInput();

        if (_isOnRope && _activeRopeSegment != null)
        {
            _rb.MovePosition(Vector2.Lerp(_rb.position, _activeRopeSegment.position, 20f * Time.fixedDeltaTime));
            
            _activeRopeSegment.AddForce(new Vector2(h * swingForce, 0));

            if (_climbCooldown > 0) _climbCooldown -= Time.fixedDeltaTime;
            if (Mathf.Abs(v) > 0.1f && _climbCooldown <= 0)
            {
                TrySwitchSegment(v);
            }
        }
        else if (_isOnLadder)
        {
            _rb.linearVelocity = new Vector2(h * moveSpeed * 0.5f, v * climbSpeed);
        }
        else
        {
            float speed = _isDragging ? moveSpeed * 0.4f : moveSpeed;
            _rb.linearVelocity = new Vector2(h * speed, _rb.linearVelocity.y);
        }
    }

    private void TrySwitchSegment(float direction)
    {
        float offset = direction > 0 ? 0.7f : -0.7f;
        Vector2 checkPoint = (Vector2)_activeRopeSegment.position + Vector2.up * offset;
        
        Collider2D[] hits = Physics2D.OverlapCircleAll(checkPoint, 0.6f);
        foreach (var hit in hits)
        {
            if (hit.gameObject != _activeRopeSegment.gameObject && hit.CompareTag("Rope"))
            {
                _activeRopeSegment = hit.GetComponent<Rigidbody2D>();
                _climbCooldown = 0.15f; 
                return;
            }
        }
    }

    public void SetOnRope(bool state, Rigidbody2D segment)
    {
        _isOnRope = state;
        _activeRopeSegment = segment;
        
        if (state)
        {
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.linearVelocity = Vector2.zero;
        }
        else
        {
            _rb.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    public void SetOnLadder(bool state)
    {
        _isOnLadder = state;
        _rb.bodyType = state ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
        if (state) _rb.linearVelocity = Vector2.zero;
    }

    public void SetDragging(bool state) => _isDragging = state;

    private bool IsGrounded() => Physics2D.BoxCast(_coll.bounds.center, _coll.bounds.size, 0f, Vector2.down, 0.1f, groundLayer);
}