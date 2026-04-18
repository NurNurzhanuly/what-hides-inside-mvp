using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Collider2D))]
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
    private Collider2D _coll;
    private bool _isSprung = false;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _audio = GetComponent<AudioSource>();
        _coll = GetComponent<Collider2D>();

        // Убедитесь, что в инспекторе Is Trigger УЖЕ включен!
        if (_sr != null && openSprite != null) _sr.sprite = openSprite;
    }

    private void HandleHit(GameObject hitObject)
    {
        if (_isSprung) return;
        if (hitObject == null) return;

        Debug.Log($"BearTrap: Detected {hitObject.name}"); // Добавили лог

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
        // Проверяем теги, как договаривались
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
        
        // Отключаем коллайдер, чтобы игрок мог пройти сквозь закрытый капкан
        if (_coll != null) _coll.enabled = false;

        Logger.Instance?.Log("BearTrap snapped");
    }
}