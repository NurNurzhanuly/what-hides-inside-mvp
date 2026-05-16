using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public float lifeTime = 3f;

    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        // Если пуля не летит, проверь, не стоит ли у нее Body Type = Kinematic
        _rb.linearVelocity = transform.right * speed;
        
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Сначала проверяем: можем ли мы нанести урон? (Игрок)
        IDamageable damageable = collision.GetComponent<IDamageable>() ?? collision.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(100f);
            Destroy(gameObject);
            return;
        }

        // Если объект, в который мы попали, является триггером (лестница, зона камеры и т.д.) пуля летит дальше, игнорируя его
        if (collision.isTrigger)
        {
            return; 
        }
        
        // В этом случае пуля должна разбиться.
        Destroy(gameObject);
    }
}