using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float checkDistance = 0.8f;
    public LayerMask interactableLayer;
    
    private IInputProvider _input;
    private FixedJoint2D _joint;

    void Awake()
    {
        _input = GetComponent<IInputProvider>();
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
            _joint = gameObject.AddComponent<FixedJoint2D>();
            _joint.connectedBody = hit.collider.gameObject.GetComponent<Rigidbody2D>();
            _joint.enableCollision = false; 
            GetComponent<PlayerMovement>().SetDragging(true);
        }
    }

    private void Release()
    {
        if (_joint != null)
        {
            Destroy(_joint);
            _joint = null;
            GetComponent<PlayerMovement>().SetDragging(false);
        }
    }
}