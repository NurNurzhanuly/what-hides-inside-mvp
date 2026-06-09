using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    [Header("Ссылки")]
    public Transform visualsTransform; 
    
    private Rigidbody2D _rb;
    private PlayerMovement _movement;
    private LedgeClimb _ledge;
    private Animator _animator;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _movement = GetComponent<PlayerMovement>();
        _ledge = GetComponent<LedgeClimb>();
        
        if (visualsTransform != null)
            _animator = visualsTransform.GetComponent<Animator>();
    }

    void Update()
    {
        if (_rb == null || _animator == null || visualsTransform == null) return;
        
        float currentSpeed = Mathf.Abs(_rb.linearVelocity.x);
        

        _animator.SetFloat("Speed", currentSpeed);
        _animator.SetBool("IsGrounded", _movement.IsGrounded());
        

        if (_ledge != null)
        {
            _animator.SetBool("isHanging", _ledge.IsHanging);
        }


        bool isHanging = (_ledge != null && _ledge.IsHanging);
        bool isDragging = (_movement != null && _movement.IsDragging());

        if (currentSpeed > 0.1f && !isHanging && !isDragging)
        {
            float dir = Mathf.Sign(_rb.linearVelocity.x); 
            visualsTransform.localScale = new Vector3(dir * Mathf.Abs(visualsTransform.localScale.x), visualsTransform.localScale.y, visualsTransform.localScale.z);
        }
    }
}