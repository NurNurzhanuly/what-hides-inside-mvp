using UnityEngine;

public class RotatingSaw : MonoBehaviour
{
    // В режиме Manual ты сама двигаешь искры руками в Scene, скрипт их не трогает
    public enum SparkPlacement { Bottom, Top, Left, Right, Manual }

    [Header("Вращение картинки")]
    public Transform sawVisuals;
    public float rotationSpeed = 500f;

    [Header("Настройка Эффектов (VFX)")]
    public ParticleSystem sparksEffect;
    public ParticleSystem dustEffect;
    public SparkPlacement placement = SparkPlacement.Bottom;
    [Tooltip("Дистанция от центра (только для авто-режимов)")]
    public float offsetDistance = 1.2f;
    
    [Header("Направление искр")]
    [Tooltip("Угол вылета. Работает ВСЕГДА, даже если пила стоит")]
    public float rotationOffset = 0f; 
    [Tooltip("Разворачивать искры против движения при патруле?")]
    public bool flipByDirection = true;

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
        // 1. Крутим картинку
        if (sawVisuals != null)
            sawVisuals.Rotate(0, 0, rotationSpeed * Time.deltaTime, Space.Self);

        // 2. Двигаем пилу
        if (isPatrolling) HandleMovement();
        
        // 3. Управляем эффектами
        UpdateEffects();
    }

    // Это чтобы всё менялось в реальном времени, когда ты крутишь ползунки в инспекторе
    private void OnValidate()
    {
        UpdateEffects();
    }

    private void UpdateEffects()
    {
        if (sparksEffect == null) return;

        // --- 1. ПОЗИЦИЯ (Transform) ---
        // Если Manual - мы ВООБЩЕ не трогаем позицию, двигай объект Sparks руками в окне Scene
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

        // --- 2. НАПРАВЛЕНИЕ (Rotation) ---
        float finalAngle = rotationOffset;

        // Если пила едет - считаем угол от движения, если стоит - берем просто rotationOffset
        if (isPatrolling && flipByDirection && Application.isPlaying)
        {
            Vector3 moveDir = (transform.position - _lastPosition).normalized;
            if (moveDir.magnitude > 0.001f)
            {
                finalAngle = Mathf.Atan2(-moveDir.y, -moveDir.x) * Mathf.Rad2Deg + rotationOffset;
            }
            _lastPosition = transform.position;
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