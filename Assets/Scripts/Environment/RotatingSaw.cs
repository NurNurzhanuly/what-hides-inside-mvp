using UnityEngine;

public class RotatingSaw : MonoBehaviour
{
    public enum SparkPlacement { Bottom, Top, Left, Right, Manual }

    [Header("Вращение")]
    public Transform sawVisuals;
    public float rotationSpeed = 500f;

    [Header("Движение (Патруль)")]
    public bool isPatrolling = false;
    public Vector3 targetOffset;
    public float moveSpeed = 2f;
    public float waitTimeAtEnds = 0.2f;

    [Header("Настройка Искр (VFX)")]
    public ParticleSystem sparksEffect;
    public SparkPlacement placement = SparkPlacement.Bottom;
    [Tooltip("Расстояние от центра до зубьев")]
    public float offsetDistance = 1.0f;
    [Tooltip("Разворачивать искры против движения?")]
    public bool flipByDirection = true;

    private Vector3 _startPos;
    private Vector3 _targetPos;
    private bool _movingToEnd = true;
    private float _currentWaitTime = 0f;
    private Vector3 _lastPosition;

    void Awake()
    {
        _startPos = transform.position;
        _targetPos = _startPos + targetOffset;
        _lastPosition = transform.position;
    }

    void Update()
    {
        // 1. Вращение визуала
        if (sawVisuals != null)
            sawVisuals.Rotate(0, 0, rotationSpeed * Time.deltaTime, Space.Self);

        // 2. Движение
        if (isPatrolling) HandleMovement();

        // 3. Позиционирование искр
        if (sparksEffect != null) UpdateSparks();
    }

    // Эта функция обновляет положение искр в реальном времени в редакторе
    void OnValidate()
    {
        if (sparksEffect != null) UpdateSparks();
    }

    private void UpdateSparks()
    {
        if (placement == SparkPlacement.Manual) return;

        // Ставим искры на границу пилы
        Vector3 newPos = Vector3.zero;
        switch (placement)
        {
            case SparkPlacement.Bottom: newPos = Vector3.down * offsetDistance; break;
            case SparkPlacement.Top:    newPos = Vector3.up * offsetDistance; break;
            case SparkPlacement.Left:   newPos = Vector3.left * offsetDistance; break;
            case SparkPlacement.Right:  newPos = Vector3.right * offsetDistance; break;
        }
        sparksEffect.transform.localPosition = newPos;

        // Если пила едет, разворачиваем хвост искр в обратную сторону
        if (flipByDirection && isPatrolling && Application.isPlaying)
        {
            Vector3 moveDir = (transform.position - _lastPosition).normalized;
            _lastPosition = transform.position;

            if (moveDir.magnitude > 0.001f)
            {
                float angle = Mathf.Atan2(-moveDir.y, -moveDir.x) * Mathf.Rad2Deg;
                sparksEffect.transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }

    private void HandleMovement()
    {
        if (_currentWaitTime > 0)
        {
            _currentWaitTime -= Time.deltaTime;
            return;
        }
        Vector3 target = _movingToEnd ? _targetPos : _startPos;
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            _movingToEnd = !_movingToEnd;
            _currentWaitTime = waitTimeAtEnds;
        }
    }
}