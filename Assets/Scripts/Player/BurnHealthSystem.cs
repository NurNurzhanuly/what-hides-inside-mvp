using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class BurnHealthSystem : MonoBehaviour, IDamageable
{
    public float maxHealth = 100f;
    public float recoveryRate = 20f; // Восстановление в тени
    public UnityEvent<float> onHealthChanged;

    private float _currentHealth;
    private PlayerMovement _movement;
    private bool _isBurningThisFrame;

    private void Awake()
    {
        _currentHealth = maxHealth;
        _movement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        // Если игрока не жгут светом — он лечится (согласно ресерчу)
        if (!_isBurningThisFrame && _currentHealth < maxHealth)
        {
            _currentHealth += recoveryRate * Time.deltaTime;
            _currentHealth = Mathf.Min(_currentHealth, maxHealth);
            UpdateStatus();
        }
        _isBurningThisFrame = false;
    }

    public void TakeDamage(float amount)
    {
        _isBurningThisFrame = true;
        _currentHealth -= amount * Time.deltaTime;
        UpdateStatus();

        if (_currentHealth <= 0)
        {
            if (Logger.Instance != null) Logger.Instance.Log("Player died from light exposure.");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void UpdateStatus()
    {
        float ratio = _currentHealth / maxHealth;
        onHealthChanged?.Invoke(ratio);
        
        // Твоя фишка с замедлением
        if (_movement != null) _movement.SetSpeedModifier(Mathf.Lerp(0.3f, 1f, ratio));
    }
}