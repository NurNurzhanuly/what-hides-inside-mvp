using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 7f;
    public float climbSpeed = 5f;
    public float jumpForce = 11f;
    public float swingForce = 45f;

    [Header("Dragging")]
    [Range(0.1f, 1f)]
    public float dragSpeedMultiplier = 0.8f;

    [Header("Limbo Physics")]
    [Tooltip("Окно, в котором прыжок ещё засчитывается после схода с края")]
    public float coyoteTime = 0.15f;
    [Tooltip("Окно, в котором нажатие прыжка кэшируется ПЕРЕД приземлением")]
    public float jumpBufferTime = 0.12f;
    public float fallGravityMultiplier = 2f;

    [Header("Rope")]
    [Tooltip("Пауза перед повторным хватанием после прыжка с верёвки")]
    public float ropeRegrabCooldownTime = 0.3f;
    [Tooltip("Радиус поиска соседнего сегмента при лазании. Чуть больше расстояния между двумя сегментами, но меньше двойного")]
    public float climbReach = 0.8f;
    [Tooltip("Пауза между перехватами при лазании по верёвке")]
    public float climbCooldownTime = 0.2f;
    [Tooltip("Насколько выше центра игрока точка хвата (примерно полроста)")]
    public float grabAnchorY = 0.5f;

    public LayerMask groundLayer;

    private Rigidbody2D _rb;
    private BoxCollider2D _coll;
    private IInputProvider _input;

    private bool _isDragging = false;
    private bool _isOnLadder = false;
    private bool _isOnRope = false;
    private Rigidbody2D _activeRopeSegment;
    private HingeJoint2D _ropeJoint;
    private float _climbCooldown = 0f;
    private float _ropeRegrabCooldown = 0f;
    private readonly Collider2D[] _overlapResults = new Collider2D[8];

    private float _coyoteTimeCounter;
    private float _jumpBufferCounter;
    private float _defaultGravity;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _coll = GetComponent<BoxCollider2D>();
        _input = GetComponent<IInputProvider>();
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _defaultGravity = _rb.gravityScale;
    }

    void Update()
    {
        if (_input == null) return;

        if (_ropeRegrabCooldown > 0f) _ropeRegrabCooldown -= Time.deltaTime;

        // --- Coyote time: помним, что недавно был "на опоре" ---
        if (IsGrounded() || _isOnLadder || _isOnRope)
            _coyoteTimeCounter = coyoteTime;
        else
            _coyoteTimeCounter -= Time.deltaTime;

        // --- Jump buffer: помним, что недавно нажали прыжок ---
        if (_input.IsJumpPressed())
            _jumpBufferCounter = jumpBufferTime;
        else
            _jumpBufferCounter -= Time.deltaTime;

        // --- Прыжок срабатывает, когда совпали оба окна ---
        if (_jumpBufferCounter > 0f && _coyoteTimeCounter > 0f)
        {
            if (_isOnRope) SetOnRope(false, null);
            if (_isOnLadder) SetOnLadder(false);

            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);

            // Гасим оба счётчика, чтобы одно нажатие не дало двойной прыжок
            _coyoteTimeCounter = 0f;
            _jumpBufferCounter = 0f;
        }
    }

    void FixedUpdate()
    {
        if (_input == null) return;

        float h = _input.GetHorizontalInput();
        float v = _input.GetVerticalInput();

        if (_rb.linearVelocity.y < 0 && !_isOnLadder && !_isOnRope)
            _rb.gravityScale = _defaultGravity * fallGravityMultiplier;
        else
            _rb.gravityScale = _defaultGravity;

        if (_isOnRope && _activeRopeSegment != null)
        {
            _rb.AddForce(new Vector2(h * swingForce, 0f), ForceMode2D.Force);

            if (_climbCooldown > 0) _climbCooldown -= Time.fixedDeltaTime;
            if (Mathf.Abs(v) > 0.1f && _climbCooldown <= 0)
                TryClimb(v);
        }
        else if (_isOnLadder)
        {
            _rb.linearVelocity = new Vector2(h * moveSpeed * 0.5f, v * climbSpeed);
        }
        else
        {
            float speed = _isDragging ? moveSpeed * dragSpeedMultiplier : moveSpeed;
            _rb.linearVelocity = new Vector2(h * speed, _rb.linearVelocity.y);
        }
    }

    private void TryClimb(float direction)
    {
        if (_activeRopeSegment == null || _ropeJoint == null) return;

        Vector2 center = _activeRopeSegment.position;
        int hitCount = Physics2D.OverlapCircleNonAlloc(center, climbReach, _overlapResults);

        Rigidbody2D best = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = _overlapResults[i];
            if (hit == null) continue;

            Rigidbody2D segRb = hit.attachedRigidbody;
            if (segRb == null || segRb == _activeRopeSegment) continue;
            if (!hit.CompareTag("Rope")) continue;

            float dy = segRb.position.y - center.y;
            if (direction > 0 && dy <= 0.01f) continue;
            if (direction < 0 && dy >= -0.01f) continue;

            float dist = Vector2.Distance(segRb.position, center);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = segRb;
            }
        }

        if (best != null)
        {
            _activeRopeSegment = best;
            _ropeJoint.connectedBody = best;
            _climbCooldown = climbCooldownTime;
        }
    }

    public void SetOnRope(bool state, Rigidbody2D segment)
    {
        if (state)
        {
            if (_isOnRope) return;
            if (_ropeRegrabCooldown > 0f) return;
            if (segment == null) return;

            _isOnRope = true;
            _rb.bodyType = RigidbodyType2D.Dynamic;
            AttachToRope(segment);
        }
        else
        {
            _isOnRope = false;
            _ropeRegrabCooldown = ropeRegrabCooldownTime;
            if (_ropeJoint != null) Destroy(_ropeJoint);
            _ropeJoint = null;
            _activeRopeSegment = null;
        }
    }

    private void AttachToRope(Rigidbody2D segment)
    {
        _activeRopeSegment = segment;

        if (_ropeJoint == null)
            _ropeJoint = gameObject.AddComponent<HingeJoint2D>();

        _ropeJoint.autoConfigureConnectedAnchor = false;
        _ropeJoint.connectedBody = segment;
        _ropeJoint.anchor = new Vector2(0f, grabAnchorY);
        _ropeJoint.connectedAnchor = Vector2.zero;
    }

    public void SetOnLadder(bool state)
    {
        _isOnLadder = state;
        _rb.bodyType = state ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
        if (state) _rb.linearVelocity = Vector2.zero;
    }

    public void SetDragging(bool state) => _isDragging = state;

    public bool IsGrounded() => Physics2D.BoxCast(_coll.bounds.center, _coll.bounds.size, 0f, Vector2.down, 0.1f, groundLayer);
}