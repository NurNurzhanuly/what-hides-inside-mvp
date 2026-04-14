using UnityEngine;

public class SwitchBox : MonoBehaviour
{
    public TriggerableObject targetDevice;
    public Color activeColor = Color.green;
    private Color _idleColor;
    private SpriteRenderer _sr;
    private bool _isActivated = false;
    private bool _canInteract = false;
    private IInputProvider _playerInput;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        if (_sr != null) _idleColor = _sr.color;
    }

    void Update()
    {
        if (_canInteract && _playerInput != null && _playerInput.IsInteractPressed())
        {
            Toggle();
        }
    }

    private void Toggle()
    {
        _isActivated = !_isActivated;
        if (_sr != null) _sr.color = _isActivated ? activeColor : _idleColor;
        
        if (targetDevice != null)
        {
            if (_isActivated) targetDevice.Activate();
            else targetDevice.Deactivate();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _canInteract = true;
            _playerInput = other.GetComponent<IInputProvider>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) _canInteract = false;
    }
}