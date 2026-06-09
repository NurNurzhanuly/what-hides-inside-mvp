using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    public float speed = 30f; 
    public float lifeTime = 3f;

    [Tooltip("Префаб искр при попадании (ParticleSystem + AutoDestroy, Play On Awake ВКЛ)")]
    public GameObject impactEffect;

    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        
        _rb.linearVelocity = transform.right * speed;

        TrailRenderer tr = GetComponent<TrailRenderer>();
        if (tr != null)
        {
            tr.Clear(); 
        }

        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        IDamageable damageable = collision.GetComponent<IDamageable>() ?? collision.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(100f);
            SpawnImpact();
            Destroy(gameObject);
            return;
        }


        if (collision.isTrigger) return;

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