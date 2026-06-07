using UnityEngine;

public class RotatingSaw : MonoBehaviour
{
    public enum SparkPlacement { Bottom, Top, Left, Right, Manual }

    [Header("Вращение")]
    public Transform sawVisuals;
    public float rotationSpeed = 500f;

    [Header("Настройка Эффектов")]
    public ParticleSystem sparksEffect;
    public ParticleSystem dustEffect;
    public SparkPlacement placement = SparkPlacement.Bottom;
    public float offsetDistance = 1.2f;
    
    [Header("Направление (Rotation Offset)")]
    [Range(-360, 360)]
    public float rotationOffset = 0f; 
    public bool flipByDirection = true;

    [Header("Движение")]
    public bool isPatrolling = false;
    public Vector3 targetOffset;
    public float moveSpeed = 2f;
    public float waitTimeAtEnds = 0.2f;

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
        if (sawVisuals != null)
            sawVisuals.Rotate(0, 0, rotationSpeed * Time.deltaTime, Space.Self);

        if (isPatrolling) HandleMovement();
        
        UpdateEffects();
    }

    // Это заставляет искры крутиться прямо в редакторе, когда ты двигаешь ползунок
    private void OnValidate()
    {
        UpdateEffects();
    }

    private void UpdateEffects()
    {
        if (sparksEffect == null) return;

        // 1. Позиция
        if (placement != SparkPlacement.Manual)
        {
            Vector3 localPos = Vector3.zero;
            switch (placement)
            {
                case SparkPlacement.Bottom: localPos = Vector2.down * offsetDistance; break;
                case SparkPlacement.Top:    localPos = Vector2.up * offsetDistance; break;
                case SparkPlacement.Left:   localPos = Vector2.left * offsetDistance; break;
                case SparkPlacement.Right:  localPos = Vector2.right * offsetDistance; break;
            }
            sparksEffect.transform.localPosition = localPos;
            if (dustEffect != null) dustEffect.transform.localPosition = localPos;
        }

        // 2. Направление (ВОТ ТУТ ПРАВКА)
        float currentAngle = rotationOffset;

        // Если едем - добавляем разворот. Если стоим - просто юзаем оффсет.
        if (isPatrolling && flipByDirection && Application.isPlaying)
        {
            Vector3 moveDir = (transform.position - _lastPosition).normalized;
            if (moveDir.magnitude > 0.01f)
            {
                // Считаем угол "от движения" + оффсет
                currentAngle = Mathf.Atan2(-moveDir.y, -moveDir.x) * Mathf.Rad2Deg + rotationOffset;
                // Чтобы угол был локальным относительно наклона пилы:
                currentAngle -= transform.eulerAngles.z; 
            }
            _lastPosition = transform.position;
        }

        Quaternion targetRot = Quaternion.Euler(0, 0, currentAngle);
        sparksEffect.transform.localRotation = targetRot;
        if (dustEffect != null) dustEffect.transform.localRotation = targetRot;
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