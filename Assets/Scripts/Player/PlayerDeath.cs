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
        Invoke(nameof(RestartLevel), 0.15f);
    }

    private void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}