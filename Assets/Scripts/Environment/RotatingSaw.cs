using UnityEngine;

public class RotatingSaw : MonoBehaviour
{
    [Header("Вращение")]
    [Tooltip("Ссылка на дочерний объект с картинкой (Visuals)")]
    public Transform sawVisuals; 
    public float rotationSpeed = 400f;

    [Header("Движение (Патруль)")]
    public bool isPatrolling = false;
    [Tooltip("На сколько сдвинется пила от точки спавна (X, Y)")]
    public Vector3 targetOffset;
    public float moveSpeed = 2f;
    [Tooltip("Пауза на краях маршрута")]
    public float waitTimeAtEnds = 0.2f;

    private Vector3 _startPos;
    private Vector3 _targetPos;
    private bool _movingToEnd = true;
    private float _currentWaitTime = 0f;

    void Awake()
    {
        // Запоминаем точки маршрута
        _startPos = transform.position;
        _targetPos = _startPos + targetOffset;
    }

    void Update()
    {
        // 1. ВРАЩЕНИЕ (Крутим только картинку!)
        if (sawVisuals != null)
        {
            // Используем Space.Self, чтобы она крутилась вокруг своей центральной оси
            sawVisuals.Rotate(0, 0, rotationSpeed * Time.deltaTime, Space.Self);
        }
        else
        {
            Debug.LogWarning($"Пила {gameObject.name} не имеет ссылки на Visuals!");
        }

        // 2. ДВИЖЕНИЕ (Двигаем весь родительский объект туда-сюда)
        if (isPatrolling)
        {
            if (_currentWaitTime > 0)
            {
                _currentWaitTime -= Time.deltaTime;
                return; // Ждем на краю
            }

            Vector3 target = _movingToEnd ? _targetPos : _startPos;
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target) < 0.01f)
            {
                _movingToEnd = !_movingToEnd; // Разворачиваемся
                _currentWaitTime = waitTimeAtEnds; // Начинаем ждать
            }
        }
    }

    // Убийство игрока
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Ищем скрипт IDamageable у того, кто в нас врезался
        IDamageable damageable = collision.GetComponent<IDamageable>() ?? collision.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(100f);
        }
    }
}