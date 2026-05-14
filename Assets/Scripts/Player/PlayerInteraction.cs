using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float checkDistance = 0.8f;
    public LayerMask interactableLayer;
    
    private IInputProvider _input;
    private FixedJoint2D _joint;
    private GameObject _currentObject;
    private PlayerMovement _movement;

    void Awake()
    {
        _input = GetComponent<IInputProvider>();
        _movement = GetComponent<PlayerMovement>();
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
            _currentObject = hit.collider.gameObject; // ОШИБКА БЫЛА ЗДЕСЬ. Теперь мы запоминаем объект
            Rigidbody2D targetRb = _currentObject.GetComponent<Rigidbody2D>();

            if (targetRb != null)
            {
                _joint = gameObject.AddComponent<FixedJoint2D>();
                _joint.connectedBody = targetRb;
                _joint.breakForce = Mathf.Infinity;
                
                if (_movement != null) _movement.SetDragging(true);
            }
        }
    }

    private void Release()
    {
        if (_joint != null) Destroy(_joint);
        _joint = null;
        _currentObject = null;
        
        if (_movement != null) _movement.SetDragging(false);
    }
}