using UnityEngine;

public class Lever : MonoBehaviour
{
    public TriggerableObject targetDevice;
    public Sprite offSprite;
    public Sprite onSprite;
    
    private bool _isOn = false;
    private SpriteRenderer _sr;
    private bool _playerInRange = false;
    private IInputProvider _input;

    void Awake()
    {
        _sr = GetComponentInChildren<SpriteRenderer>();
        if (offSprite != null) _sr.sprite = offSprite;
    }

    void Update()
    {
        if (_playerInRange && _input != null && _input.IsInteractPressed())
        {
            Toggle();
        }
    }

    private void Toggle()
    {
        _isOn = !_isOn;
        if (_sr != null) _sr.sprite = _isOn ? onSprite : offSprite;
        
        if (targetDevice != null)
        {
            if (_isOn) targetDevice.Activate();
            else targetDevice.Deactivate();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = true;
            _input = other.GetComponent<IInputProvider>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = false;
        }
    }
}