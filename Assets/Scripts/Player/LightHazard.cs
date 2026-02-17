using UnityEngine;

public class LightHazard : MonoBehaviour
{
    public float damageIntensity = 20f;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(damageIntensity);
        }
    }
}