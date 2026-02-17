using UnityEngine;
using UnityEngine.Events;

public class BurnHealthSystem : MonoBehaviour, IDamageable
{
    [Header("Settings")]
    public float maxHealth = 100f;
    public float recoveryRate = 5f;
    
    private float _currentHealth;
    private bool _isBeingBurned;

    public UnityEvent<float> onHealthChanged;
    public UnityEvent onDeath;

    private void Awake() => _currentHealth = maxHealth;

    private void Update()
    {
        if (!_isBeingBurned && _currentHealth < maxHealth)
        {
            _currentHealth += recoveryRate * Time.deltaTime;
            _currentHealth = Mathf.Min(_currentHealth, maxHealth);
            onHealthChanged?.Invoke(_currentHealth / maxHealth);
        }
        
        _isBeingBurned = false; 
    }

    public void TakeDamage(float amount)
    {
        _isBeingBurned = true;
        _currentHealth -= amount * Time.deltaTime;
        
        onHealthChanged?.Invoke(_currentHealth / maxHealth);

        if (_currentHealth <= 0)
        {
            onDeath?.Invoke();
            Debug.Log("Shadow Boy dissolved...");
        }
    }
}