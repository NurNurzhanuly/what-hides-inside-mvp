using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class BurnHealthSystem : MonoBehaviour, IDamageable
{
    public float maxHealth = 100f;
    public UnityEvent<float> onHealthChanged;

    private float _currentHealth;
    private PlayerMovement _movement;

    private void Awake()
    {
        _currentHealth = maxHealth;
        _movement = GetComponent<PlayerMovement>();
    }

    public void TakeDamage(float amount)
    {
        _currentHealth -= amount * Time.deltaTime;
        float ratio = _currentHealth / maxHealth;
        
        onHealthChanged?.Invoke(ratio);
        if (_movement != null) _movement.SetSpeedModifier(Mathf.Lerp(0.3f, 1f, ratio));

        if (_currentHealth <= 0) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}