using UnityEngine;

public class DeathTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        var dmg = other.GetComponentInParent<IDamageable>();
        if (dmg != null) dmg.TakeDamage(9999f);
    }
}