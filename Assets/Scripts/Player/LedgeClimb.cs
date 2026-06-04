using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class LedgeClimb : MonoBehaviour
{
    [Tooltip("Обычно = тот же слой, что groundLayer у PlayerMovement")]
    public LayerMask ledgeLayer;

    [Header("Сенсоры края")]
    public float wallCheckDist = 0.5f;
    public float lowerCheckHeight = 0.3f;
    public float upperCheckHeight = 1.1f;

    [Header("Поза и тайминги")]
    public float hangBelowTop = 1.0f;
    public float climbDuration = 0.25f;
    public float ledgeGraceTime = 0.15f;
    public float ledgeRegrabTime = 0.3f;
    public float catchMaxRiseSpeed = 1.0f;

    private enum State { None, Hanging, Climbing }
    private State _state = State.None;

    private PlayerMovement _pm;
    private Rigidbody2D _rb;
    private BoxCollider2D _coll;
    private IInputProvider _input;

    private Transform _grabbed;
    private Vector3 _hangOffset;
    private Vector3 _topOffset;
    private int _climbDir = 1;
    private bool _enteredFromAbove = false;   // как вошли в вис
    private float _grace = 0f;
    private float _regrabCd = 0f;
    private float _climbT = 0f;

    void Awake()
    {
        _pm = GetComponent<PlayerMovement>();
        _rb = GetComponent<Rigidbody2D>();
        _coll = GetComponent<BoxCollider2D>();
        _input = GetComponent<IInputProvider>();
        if (ledgeLayer == 0) ledgeLayer = _pm.groundLayer;
    }

    void FixedUpdate()
    {
        if (_input == null) return;
        if (_regrabCd > 0f) _regrabCd -= Time.fixedDeltaTime;

        switch (_state)
        {
            case State.None: TryGrab(); break;
            case State.Hanging: Hang(); break;
            case State.Climbing: Climb(); break;
        }
    }

    private void TryGrab()
    {
        if (!_pm.CanLedgeGrab || _regrabCd > 0f) return;
        float v = _input.GetVerticalInput();

        if (_pm.IsGrounded())
        {
            if (v < -0.5f && DetectFromAbove(out float wx, out float ty, out Transform g))
                EnterHang(wx, ty, -_pm.Facing, g, true);   // вошли СВЕРХУ
        }
        else
        {
            if (_rb.linearVelocity.y <= catchMaxRiseSpeed &&
                DetectFromBelow(out float wx, out float ty, out Transform g))
                EnterHang(wx, ty, _pm.Facing, g, false);   // вошли СНИЗУ
        }
    }

    private bool DetectFromBelow(out float wallX, out float topY, out Transform grabbed)
    {
        wallX = 0f; topY = 0f; grabbed = null;
        Vector2 dir = Vector2.right * _pm.Facing;
        Vector2 c = _coll.bounds.center;

        RaycastHit2D low = Physics2D.Raycast(c + Vector2.up * lowerCheckHeight, dir, wallCheckDist, ledgeLayer);
        RaycastHit2D high = Physics2D.Raycast(c + Vector2.up * upperCheckHeight, dir, wallCheckDist, ledgeLayer);
        if (low.collider == null || high.collider != null) return false;

        wallX = low.point.x;
        Vector2 topOrigin = new Vector2(wallX + dir.x * 0.15f, c.y + upperCheckHeight + 0.5f);
        RaycastHit2D top = Physics2D.Raycast(topOrigin, Vector2.down, upperCheckHeight + 1.0f, ledgeLayer);
        if (top.collider == null) return false;

        topY = top.point.y;
        grabbed = top.rigidbody ? top.rigidbody.transform : top.collider.transform;
        return true;
    }

    private bool DetectFromAbove(out float wallX, out float topY, out Transform grabbed)
    {
        wallX = 0f; topY = 0f; grabbed = null;
        Vector2 dir = Vector2.right * _pm.Facing;

        Vector2 footUnder = new Vector2(_coll.bounds.center.x, _coll.bounds.min.y + 0.05f);
        Vector2 footFwd = new Vector2(_coll.bounds.center.x + dir.x * (_coll.bounds.extents.x + 0.2f), _coll.bounds.min.y + 0.05f);

        RaycastHit2D under = Physics2D.Raycast(footUnder, Vector2.down, 0.3f, ledgeLayer);
        RaycastHit2D front = Physics2D.Raycast(footFwd, Vector2.down, 0.4f, ledgeLayer);
        if (under.collider == null || front.collider != null) return false;

        wallX = _coll.bounds.center.x + dir.x * _coll.bounds.extents.x;
        topY = under.point.y;
        grabbed = under.rigidbody ? under.rigidbody.transform : under.collider.transform;
        return true;
    }

    private void EnterHang(float wallX, float topY, int climbDir, Transform grabbed, bool fromAbove)
    {
        _state = State.Hanging;
        _climbDir = climbDir;
        _enteredFromAbove = fromAbove;
        _grace = ledgeGraceTime;
        _pm.ExternalControl = true;
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.linearVelocity = Vector2.zero;

        Vector2 hangCenter = new Vector2(wallX - climbDir * (_coll.bounds.extents.x * 0.4f), topY - hangBelowTop);
        transform.position += (Vector3)(hangCenter - (Vector2)_coll.bounds.center);

        Vector2 climbCenter = new Vector2(wallX + climbDir * (_coll.bounds.extents.x + 0.05f), topY + _coll.bounds.extents.y + 0.02f);
        Vector3 climbPos = transform.position + (Vector3)(climbCenter - (Vector2)_coll.bounds.center);

        _grabbed = grabbed;
        _hangOffset = transform.position - grabbed.position;
        _topOffset = climbPos - grabbed.position;
    }

    private void Hang()
    {
        if (_grabbed == null) { Release(); return; }
        _rb.MovePosition((Vector2)_grabbed.position + (Vector2)_hangOffset);

        if (_grace > 0f) { _grace -= Time.fixedDeltaTime; return; }

        float v = _input.GetVerticalInput();

        if (v > 0.5f)
        {
            // ВВЕРХ — забраться наверх
            _state = State.Climbing; _climbT = 0f;
        }
        else if (v < -0.5f)
        {
            // ВНИЗ — зависит от того, как вошли:
            if (_enteredFromAbove)
            {
                // свесился сверху и передумал → вернуться наверх (та же анимация подъёма)
                _state = State.Climbing; _climbT = 0f;
            }
            else
            {
                // зацепился снизу → отпуститься и упасть
                Release();
            }
        }
    }

    private void Climb()
    {
        if (_grabbed == null) { Finish(); return; }
        _climbT += Time.fixedDeltaTime;
        float u = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(_climbT / climbDuration));
        Vector2 off = Vector2.Lerp(_hangOffset, _topOffset, u);
        _rb.MovePosition((Vector2)_grabbed.position + off);

        if (_climbT >= climbDuration)
        {
            transform.position = (Vector3)((Vector2)_grabbed.position + (Vector2)_topOffset);
            Finish();
        }
    }

    private void Release()
    {
        _state = State.None;
        _rb.bodyType = RigidbodyType2D.Dynamic;
        _pm.ExternalControl = false;
        _regrabCd = ledgeRegrabTime;
        _rb.linearVelocity = new Vector2(-_climbDir * 1.5f, 0f);
        _grabbed = null;
    }

    private void Finish()
    {
        _state = State.None;
        _rb.bodyType = RigidbodyType2D.Dynamic;
        _rb.linearVelocity = Vector2.zero;
        _pm.ExternalControl = false;
        _regrabCd = ledgeRegrabTime;
        _grabbed = null;
    }

    void OnDrawGizmosSelected()
    {
        var coll = GetComponent<BoxCollider2D>();
        if (coll == null) return;
        Vector2 c = coll.bounds.center;
        int f = (_pm != null && Application.isPlaying) ? _pm.Facing : 1;
        Vector2 dir = Vector2.right * f;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(c + Vector2.up * lowerCheckHeight, c + Vector2.up * lowerCheckHeight + dir * wallCheckDist);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(c + Vector2.up * upperCheckHeight, c + Vector2.up * upperCheckHeight + dir * wallCheckDist);
    }
} 