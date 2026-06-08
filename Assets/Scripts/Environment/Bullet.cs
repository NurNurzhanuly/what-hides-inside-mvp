using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    public float speed = 30f; // Увеличил скорость по умолчанию для драйва
    public float lifeTime = 3f;

    [Tooltip("Префаб искр при попадании (ParticleSystem + AutoDestroy, Play On Awake ВКЛ)")]
    public GameObject impactEffect;

    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        
        // Задаем скорость пуле
        _rb.linearVelocity = transform.right * speed;

        // === РАБОТА СО СЛЕДОМ (Trail) ===
        TrailRenderer tr = GetComponent<TrailRenderer>();
        if (tr != null)
        {
            tr.Clear(); // Очищаем старые точки, чтобы след шел ровно от дула
        }

        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Проверяем на урон
        IDamageable damageable = collision.GetComponent<IDamageable>() ?? collision.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(100f);
            SpawnImpact();
            Destroy(gameObject);
            return;
        }

        // Игнорируем триггеры (зоны камеры, лестницы и т.д.)
        if (collision.isTrigger) return;

        // Если врезались в стену или пол
        SpawnImpact();
        Destroy(gameObject);
    }

    private void SpawnImpact()
    {
        if (impactEffect != null)
        {
            Instantiate(impactEffect, transform.position, Quaternion.identity);
        }
    }
}