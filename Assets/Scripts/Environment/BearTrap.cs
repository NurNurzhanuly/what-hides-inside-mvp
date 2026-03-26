using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BearTrap : MonoBehaviour
{
    public float minMassToTrigger = 1f; 
    public Sprite openSprite;
    public Sprite closedSprite;
    public AudioClip snapSound;

    private SpriteRenderer _sr;
    private AudioSource _audio;
    private bool _isSprung = false; 

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _audio = GetComponent<AudioSource>();
        if (_sr != null && openSprite != null) _sr.sprite = openSprite;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isSprung) return;
        if (collision.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(100f);
            Snap();
            return;
        }
        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        if (rb != null && rb.mass >= minMassToTrigger) Snap();
    }

    private void Snap()
    {
        _isSprung = true;
        if (_sr != null && closedSprite != null) _sr.sprite = closedSprite;
        if (snapSound != null) _audio.PlayOneShot(snapSound);
    }
}