using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 7f;
    public float climbSpeed = 5f;
    public float jumpForce = 16f;
    public float swingForce = 45f;

    [Header("Air Control")]
    public float airSpeedMultiplier = 1.4f;
    public float airAcceleration = 30f;

    [Header("Dragging")]
    [Range(0.1f, 1f)] public float dragSpeedMultiplier = 0.8f;

    [Header("Limbo Physics")]
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.12f;
    public float fallGravityMultiplier = 2f;

    [Header("Rope")]
    public float ropeRegrabCooldownTime = 0.3f;
    public float climbReach = 0.8f;
    public float climbCooldownTime = 0.2f;
    public float grabAnchorY = 0.5f;

    [Header("Audio (НОВОЕ)")]
    [Tooltip("Перетащи сюда AudioSource игрока")]
    public AudioSource playerAudioSource;
    [Tooltip("Массив звуков шагов (можно один или несколько для разнообразия)")]
    public AudioClip[] footstepSounds;
    [Tooltip("Частота шагов (0.3 - 0.4 обычно ок)")]
    public float footstepInterval = 0.35f;
    private float _stepTimer;

    public LayerMask groundLayer;

    // --- Хуки для внешних состояний ---
    public bool ExternalControl { get; set; } = false;
    public int Facing => _facing;
    public bool CanLedgeGrab => !ExternalControl && !_isOnLadder && !_isOnRope;

    private Rigidbody2D _rb;
    private BoxCollider2D _coll;
    private IInputProvider _input;

    private bool _isDragging = false;
    public bool IsDragging => _isDragging;
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
    private int _facing = 1;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _coll = GetComponent<BoxCollider2D>();
        _input = GetComponent<IInputProvider>();
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _defaultGravity = _rb.gravityScale;
        
        // Если AudioSource не назначен, пробуем найти его на этом же объекте
        if (playerAudioSource == null) playerAudioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (_input == null || ExternalControl) return;

        if (_ropeRegrabCooldown > 0f) _ropeRegrabCooldown -= Time.deltaTime;

        if (IsGrounded() || _isOnLadder || _isOnRope)
            _coyoteTimeCounter = coyoteTime;
        else
            _coyoteTimeCounter -= Time.deltaTime;

        if (_input.IsJumpPressed())
            _jumpBufferCounter = jumpBufferTime;
        else
            _jumpBufferCounter -= Time.deltaTime;

        if (_jumpBufferCounter > 0f && _coyoteTimeCounter > 0f)
        {
            if (_isOnRope) SetOnRope(false, null);
            if (_isOnLadder) SetOnLadder(false);

            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
            _coyoteTimeCounter = 0f;
            _jumpBufferCounter = 0f;
        }

        // --- ЛОГИКА ШАГОВ (НОВОЕ) ---
        HandleFootstepAudio();
    }

    private void HandleFootstepAudio()
    {
        // Играем звук только если мы на земле и реально идем
        if (IsGrounded() && Mathf.Abs(_input.GetHorizontalInput()) > 0.1f && !_isOnLadder)
        {
            _stepTimer -= Time.deltaTime;
            if (_stepTimer <= 0)
            {
                if (playerAudioSource != null && footstepSounds != null && footstepSounds.Length > 0)
                {
                    // Выбираем случайный звук из массива, чтобы не было монотонно
                    int randomIndex = Random.Range(0, footstepSounds.Length);
                    playerAudioSource.PlayOneShot(footstepSounds[randomIndex], 0.4f);
                }

                // Если тянем ящик — шаги в 1.5 раза медленнее
                float currentInterval = _isDragging ? footstepInterval * 1.5f : footstepInterval;
                _stepTimer = currentInterval;
            }
        }
        else
        {
            // Сбрасываем таймер, чтобы первый шаг при начале движения звучал сразу
            _stepTimer = 0f;
        }
    }

    void FixedUpdate()
    {
        if (_input == null || ExternalControl) return;

        float h = _input.GetHorizontalInput();
        float v = _input.GetVerticalInput();
        if (Mathf.Abs(h) > 0.1f) _facing = h > 0 ? 1 : -1;

        if (_rb.linearVelocity.y < 0 && !_isOnLadder && !_isOnRope)
            _rb.gravityScale = _defaultGravity * fallGravityMultiplier;
        else
            _rb.gravityScale = _defaultGravity;

        if (_isOnRope && _activeRopeSegment != null)
        {
            _rb.AddForce(new Vector2(h * swingForce, 0f), ForceMode2D.Force);
            if (_climbCooldown > 0) _climbCooldown -= Time.fixedDeltaTime;
            if (Mathf.Abs(v) > 0.1f && _climbCooldown <= 0) TryClimb(v);
        }
        else if (_isOnLadder)
        {
            _rb.linearVelocity = new Vector2(h * moveSpeed * 0.5f, v * climbSpeed);
        }
        else if (IsGrounded())
        {
            float speed = _isDragging ? moveSpeed * dragSpeedMultiplier : moveSpeed;
            _rb.linearVelocity = new Vector2(h * speed, _rb.linearVelocity.y);
        }
        else
        {
            float airMax = moveSpeed * airSpeedMultiplier;
            float targetX = h * airMax;
            float newX = Mathf.MoveTowards(_rb.linearVelocity.x, targetX, airAcceleration * Time.fixedDeltaTime);
            _rb.linearVelocity = new Vector2(newX, _rb.linearVelocity.y);
        }
    }

    private void TryClimb(float direction)
    {
        if (_activeRopeSegment == null || _ropeJoint == null) return;
        Vector2 center = _activeRopeSegment.position;
        int hitCount = Physics2D.OverlapCircleNonAlloc(center, climbReach, _overlapResults);

        Rigidbody2D best = null; float bestDist = float.MaxValue;
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
            if (dist < bestDist) { bestDist = dist; best = segRb; }
        }
        if (best != null) { _activeRopeSegment = best; _ropeJoint.connectedBody = best; _climbCooldown = climbCooldownTime; }
    }

    public void SetOnRope(bool state, Rigidbody2D segment)
    {
        if (state)
        {
            if (_isOnRope || _ropeRegrabCooldown > 0f || segment == null) return;
            _isOnRope = true;
            _rb.bodyType = RigidbodyType2D.Dynamic;
            AttachToRope(segment);
        }
        else
        {
            _isOnRope = false;
            _ropeRegrabCooldown = ropeRegrabCooldownTime;
            if (_ropeJoint != null) Destroy(_ropeJoint);
            _ropeJoint = null; _activeRopeSegment = null;
        }
    }

    private void AttachToRope(Rigidbody2D segment)
    {
        _activeRopeSegment = segment;
        if (_ropeJoint == null) _ropeJoint = gameObject.AddComponent<HingeJoint2D>();
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

    public bool IsGrounded() =>
        Physics2D.BoxCast(_coll.bounds.center, _coll.bounds.size, 0f, Vector2.down, 0.1f, groundLayer);
}