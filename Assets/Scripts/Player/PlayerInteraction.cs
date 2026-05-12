using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float checkDistance = 0.5f;
    public LayerMask interactableLayer;
    
    private Rigidbody2D _rb;
    private IInputProvider _input;
    private FixedJoint2D _joint;
    private GameObject _currentObject;
    private PlayerMovement _movement; // Добавили ссылку

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _input = GetComponent<IInputProvider>();
        _movement = GetComponent<PlayerMovement>(); // Находим скрипт движения
    }

    void Update()
    {
        if (_input == null) return;

        if (_input.IsInteractPressed())
        {
            if (_joint == null) TryGrab();
        }
        else
        {
            if (_joint != null) Release();
        }
    }

    private void TryGrab()
    {
        float direction = transform.localScale.x > 0 ? 1 : -1;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right * direction, checkDistance, interactableLayer);

        if (hit.collider != null)
        {
            _currentObject = hit.collider.gameObject;
            _joint = gameObject.AddComponent<FixedJoint2D>();
            _joint.connectedBody = _currentObject.GetComponent<Rigidbody2D>();
            _joint.breakForce = Mathf.Infinity;
            
            // ГОВОРИМ ДВИЖЕНИЮ, ЧТО МЫ ТАЩИМ ЯЩИК!
            if (_movement != null) _movement.SetDragging(true);
        }
    }

    private void Release()
    {
        Destroy(_joint);
        _joint = null;
        _currentObject = null;
        
        // ГОВОРИМ ДВИЖЕНИЮ, ЧТО МЫ ОТПУСТИЛИ ЯЩИК
        if (_movement != null) _movement.SetDragging(false);
    }
}