using UnityEngine;

public class RotatingSaw : MonoBehaviour
{
    public float rotationSpeed = 400f;
    public bool isPatrolling = false;
    public Vector3 targetOffset;
    public float moveSpeed = 2f;

    private Vector3 _startPos;
    private Vector3 _endPos;
    private bool _movingToEnd = true;

    void Awake()
    {
        _startPos = transform.position;
        _endPos = _startPos + targetOffset;
    }

    void FixedUpdate()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.fixedDeltaTime);

        if (isPatrolling)
        {
            Vector3 target = _movingToEnd ? _endPos : _startPos;
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.fixedDeltaTime);

            if (Vector3.Distance(transform.position, target) < 0.05f)
            {
                _movingToEnd = !_movingToEnd;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable damageable = collision.GetComponent<IDamageable>() ?? collision.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(100f);
        }
    }
}