using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeath : MonoBehaviour, IDamageable
{
    public void TakeDamage(float amount)
    {
        Debug.Log($"PlayerDeath.TakeDamage({amount}) called");
        Die();
    }

    private void Die()
    {
        Debug.Log("PlayerDeath.Die() -> restarting level");
        // Небольшая пауза, чтобы увидеть эффект перед перезагрузкой
        Invoke(nameof(RestartLevel), 0.15f);
    }

    private void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}