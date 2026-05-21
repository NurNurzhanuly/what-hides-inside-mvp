using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    [Header("Ссылки")]
    [Tooltip("Перетащи сюда дочерний объект CharacterModel из Иерархии")]
    public Transform visualsTransform; 
    
    private Rigidbody2D _rb;
    private PlayerMovement _movement;
    private Animator _animator;
    private FixedJoint2D _joint; 

    void Awake()
    {
        // 1. Ищем Физику и Движение на самом ИГРОКЕ (Родителе)
        _rb = GetComponent<Rigidbody2D>();
        _movement = GetComponent<PlayerMovement>();
        
        // 2. Ищем Аниматор на КАРТИНКЕ (Дочернем объекте CharacterModel)
        if (visualsTransform != null)
        {
            _animator = visualsTransform.GetComponent<Animator>();
        }
        else
        {
            Debug.LogError("PlayerVisuals: Не назначен объект Visuals Transform в Инспекторе!");
        }
    }

    void Update()
    {
        if (_rb == null || _animator == null || visualsTransform == null) return;
        
        _joint = GetComponent<FixedJoint2D>();
        float currentSpeed = Mathf.Abs(_rb.linearVelocity.x);
        
        // ==========================================
        // 1. УПРАВЛЕНИЕ АНИМАТОРОМ
        // ==========================================
        _animator.SetFloat("Speed", currentSpeed);

        if (_movement != null)
        {
            // Передаем твой параметр прыжка в Animator
            _animator.SetBool("IsGrounded", _movement.IsGrounded());
        }

        // ==========================================
        // 2. ПОВОРОТ КАРТИНКИ
        // ==========================================
        bool isDragging = _joint != null;
        if (currentSpeed > 0.1f && !isDragging)
        {
            float dir = Mathf.Sign(_rb.linearVelocity.x); 
            
            // ВАЖНО: Мы поворачиваем (отзеркаливаем) ТОЛЬКО дочерний объект visualsTransform!
            // Физическая коробка родителя остается неподвижной.
            visualsTransform.localScale = new Vector3(dir * Mathf.Abs(visualsTransform.localScale.x), visualsTransform.localScale.y, visualsTransform.localScale.z);
        }
    }
}