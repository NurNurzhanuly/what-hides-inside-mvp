using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public float lifeTime = 3f; // Чтобы пуля не летала вечно, если промахнулась

    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        // Пуля летит в сторону своего "правого" вектора (убедитесь, что пуля смотрит вправо в префабе)
        _rb.linearVelocity = transform.right * speed;
        
        // Самоуничтожение, если не попали в цель
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Если попали в объект, который можно убить
        IDamageable damageable = collision.GetComponent<IDamageable>() ?? collision.GetComponentInParent<IDamageable>();
        
        if (damageable != null)
        {
            damageable.TakeDamage(100f);
        }

        // В любом случае (попали в игрока или в стену) пуля исчезает
        Destroy(gameObject);
    }
}