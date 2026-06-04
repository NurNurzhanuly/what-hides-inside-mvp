using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal; // Light2D

public class Turret : MonoBehaviour
{
    [Header("Refs")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public Light2D muzzleFlashLight;
    public AudioSource audioSource;

    [Header("Detection")]
    public float detectionDistance = 15f;
    public LayerMask targetLayer;

    [Header("Charge / telegraph (предупреждение перед очередью)")]
    public AudioClip chargeClip;
    [Tooltip("Сколько турель заряжается перед очередью — это фора игроку. 0.5–1.0 норм")]
    public float chargeTime = 0.7f;
    [Tooltip("До какой интенсивности разгорается свет дула во время зарядки")]
    public float chargeLightIntensity = 1.5f;

    [Header("Burst")]
    public AudioClip singleShotClip;
    public int bulletsPerBurst = 10;
    public float timeBetweenBullets = 0.15f;
    public float spreadAngle = 3f;
    [Tooltip("Пауза между очередями после полной очереди")]
    public float burstRate = 3f;

    [Header("Muzzle flash на каждый выстрел")]
    public float flashIntensity = 2f;
    public float flashDuration = 0.05f;

    private bool _isBusy = false;   // заряжается или стреляет
    private float _cooldown = 0f;

    void Awake()
    {
        if (muzzleFlashLight != null) muzzleFlashLight.intensity = 0f;
    }

    void Update()
    {
        if (_cooldown > 0f) _cooldown -= Time.deltaTime;
        if (_isBusy || _cooldown > 0f) return;

        if (TargetInBeam())
            StartCoroutine(ChargeAndFire());
    }

    private bool TargetInBeam()
    {
        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, firePoint.right, detectionDistance, targetLayer);
        return hit.collider != null;
    }

    private IEnumerator ChargeAndFire()
    {
        _isBusy = true;

        // --- Фаза зарядки: звук + разгорается свет (telegraph) ---
        if (chargeClip != null && audioSource != null)
            audioSource.PlayOneShot(chargeClip);

        float t = 0f;
        while (t < chargeTime)
        {
            t += Time.deltaTime;
            if (muzzleFlashLight != null)
                muzzleFlashLight.intensity = Mathf.Lerp(0f, chargeLightIntensity, t / chargeTime);
            yield return null;
        }

        // --- Игрок успел уйти из луча? Отменяем очередь ---
        if (!TargetInBeam())
        {
            if (muzzleFlashLight != null) muzzleFlashLight.intensity = 0f;
            _cooldown = burstRate * 0.5f; // короткая пауза после отменённой зарядки
            _isBusy = false;
            yield break;
        }

        // --- Фаза стрельбы: пуля + короткий звук + вспышка на каждую ---
        for (int i = 0; i < bulletsPerBurst; i++)
        {
            FireOneBullet();
            yield return new WaitForSeconds(timeBetweenBullets);
        }

        if (muzzleFlashLight != null) muzzleFlashLight.intensity = 0f;
        _cooldown = burstRate;
        _isBusy = false;
    }

    private void FireOneBullet()
    {
        float angle = Random.Range(-spreadAngle, spreadAngle);
        Quaternion rot = firePoint.rotation * Quaternion.Euler(0f, 0f, angle);

        if (bulletPrefab != null)
            Instantiate(bulletPrefab, firePoint.position, rot);

        if (singleShotClip != null && audioSource != null)
            audioSource.PlayOneShot(singleShotClip);

        if (muzzleFlashLight != null)
            StartCoroutine(MuzzleFlash());
    }

    private IEnumerator MuzzleFlash()
    {
        muzzleFlashLight.intensity = flashIntensity;
        yield return new WaitForSeconds(flashDuration);
        if (muzzleFlashLight != null) muzzleFlashLight.intensity = 0f;
    }
}