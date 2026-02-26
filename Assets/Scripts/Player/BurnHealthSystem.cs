using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class BurnHealthSystem : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float recoveryRate = 15f;
    
    private float _currentHealth;
    private bool _isBeingBurned;

    [Header("Events")]
    public UnityEvent<float> onHealthChanged; 

    private void Awake() 
    {
        _currentHealth = maxHealth;
    }

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
            RestartLevel();
        }
    }

    private void RestartLevel()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}