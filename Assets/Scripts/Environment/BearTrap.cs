using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BearTrap : MonoBehaviour
{
    [Header("Settings")]
    public float minMassToTrigger = 1f;

    [Header("Visuals & Audio")]
    public Sprite openSprite;
    public Sprite closedSprite;
    public AudioClip snapSound;

    private SpriteRenderer _sr;
    private AudioSource _audio;
    private Collider2D _triggerCollider; // Будем хранить именно триггер
    private bool _isSprung = false;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _audio = GetComponent<AudioSource>();

        // Ищем среди всех коллайдеров именно тот, который отвечает за урон (Is Trigger = true)
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (var c in colliders)
        {
            if (c.isTrigger)
            {
                _triggerCollider = c;
                break;
            }
        }

        if (_sr != null && openSprite != null) _sr.sprite = openSprite;
    }

    private void HandleHit(GameObject hitObject)
    {
        if (_isSprung) return;
        if (hitObject == null) return;

        Debug.Log($"BearTrap: Detected {hitObject.name}");

        IDamageable damageable = hitObject.GetComponent<IDamageable>() ?? hitObject.GetComponentInParent<IDamageable>() ?? hitObject.GetComponentInChildren<IDamageable>();
        if (damageable != null)
        {
            Snap();
            damageable.TakeDamage(1f);
            return;
        }

        Rigidbody2D rb = hitObject.GetComponent<Rigidbody2D>() ?? hitObject.GetComponentInParent<Rigidbody2D>();
        if (rb != null && rb.mass >= minMassToTrigger)
        {
            Snap();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Капкан реагирует на Игрока и на Интерактивные объекты (ящики и т.д.)
        if (collision.CompareTag("Player") || collision.CompareTag("Interactable"))
        {
            HandleHit(collision.gameObject);
        }
    }

    private void Snap()
    {
        _isSprung = true;
        if (_sr != null && closedSprite != null) _sr.sprite = closedSprite;
        if (_audio != null && snapSound != null) _audio.PlayOneShot(snapSound);
        
        // Отключаем ТОЛЬКО триггер. Твердый коллайдер остается, чтобы капкан можно было таскать и он не падал под пол!
        if (_triggerCollider != null) _triggerCollider.enabled = false;

        Logger.Instance?.Log("BearTrap snapped");
    }
}