using UnityEngine;

public class RotatingSaw : MonoBehaviour
{
    public enum SparkPlacement { Bottom, Top, Left, Right, Manual }

    [Header("Вращение визуальной части")]
    public Transform sawVisuals;
    public float rotationSpeed = 500f;

    [Header("Эффекты (VFX)")]
    public ParticleSystem sparksEffect;
    public ParticleSystem dustEffect;
    public SparkPlacement placement = SparkPlacement.Bottom;
    public float offsetDistance = 1.2f;
    
    [Header("Настройка направления")]
    [Tooltip("Базовый угол вылета (подправь, чтобы летело параллельно поверхности)")]
    public float rotationOffset = 0f; 
    [Tooltip("Автоматически перекидывать искры на другую сторону при патруле?")]
    public bool autoFlipOnReturn = true;

    [Header("Движение (Патруль)")]
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
        // 1. Вращаем саму пилу
        if (sawVisuals != null)
            sawVisuals.Rotate(0, 0, rotationSpeed * Time.deltaTime, Space.Self);

        // 2. Двигаем пилу по маршруту
        if (isPatrolling) 
            HandleMovement();
        
        // 3. Управляем искрами и пылью
        UpdateEffects();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying) UpdateEffects();
    }

    private void UpdateEffects()
    {
        if (sparksEffect == null) return;

        // --- 1. ПОЗИЦИЯ (Где находится корень эффекта) ---
        Vector3 localPos = Vector3.zero;
        if (placement != SparkPlacement.Manual)
        {
            switch (placement)
            {
                case SparkPlacement.Bottom: localPos = Vector2.down * offsetDistance; break;
                case SparkPlacement.Top:    localPos = Vector2.up * offsetDistance; break;
                case SparkPlacement.Left:   localPos = Vector2.left * offsetDistance; break;
                case SparkPlacement.Right:  localPos = Vector2.right * offsetDistance; break;
            }

            // ГЛАВНАЯ ЛОГИКА: Если едем обратно (к старту), инвертируем позицию
            if (isPatrolling && autoFlipOnReturn && Application.isPlaying && !_movingToEnd)
            {
                localPos = -localPos;
            }

            sparksEffect.transform.localPosition = localPos;
            if (dustEffect != null) dustEffect.transform.localPosition = localPos;
        }

        // --- 2. НАПРАВЛЕНИЕ (Куда смотрят искры) ---
        float finalAngle = rotationOffset;

        if (isPatrolling && Application.isPlaying)
        {
            Vector3 moveDir = (transform.position - _lastPosition).normalized;
            
            if (moveDir.magnitude > 0.001f)
            {
                // Считаем угол "от движения" (назад) + твой оффсет
                finalAngle = Mathf.Atan2(-moveDir.y, -moveDir.x) * Mathf.Rad2Deg + rotationOffset;
            }
            _lastPosition = transform.position;
        }
        else if (autoFlipOnReturn && Application.isPlaying && !_movingToEnd)
        {
            // Если пила стоит на паузе в конце пути, но уже развернулась - зеркалим угол
            finalAngle += 180f;
        }

        Quaternion targetRot = Quaternion.Euler(0, 0, finalAngle);
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable damageable = collision.GetComponent<IDamageable>() ?? collision.GetComponentInParent<IDamageable>();
        if (damageable != null) damageable.TakeDamage(100f);
    }
}