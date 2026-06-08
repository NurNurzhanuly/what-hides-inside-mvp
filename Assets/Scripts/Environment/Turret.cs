using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Turret : MonoBehaviour
{
    [Header("Ловушка (Limbo)")]
    public TurretTrapController trapController;
    public float continuousFireRate = 0.04f;

    [Header("Refs")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public Light2D muzzleFlashLight;
    public AudioSource audioSource;

    [Header("Партиклы дула")]
    [Tooltip("ParticleSystem вспышки, ребёнок firePoint. Play On Awake выключить!")]
    public ParticleSystem muzzleParticles;
    [Tooltip("Сколько частиц на один выстрел")]
    public int muzzleParticleCount = 4;
    [Tooltip("Множитель яркости частиц, когда игрок в темноте (пещера)")]
    [Range(0.1f, 1f)] public float darkParticleAlpha = 0.5f;

    [Header("Detection")]
    public float detectionDistance = 15f;
    public LayerMask targetLayer;

    [Header("Charge / telegraph")]
    public AudioClip chargeClip;
    public float chargeTime = 0.7f;
    public float chargeLightIntensity = 1.5f;

    [Header("Burst")]
    public AudioClip singleShotClip;
    public int bulletsPerBurst = 10;
    public float timeBetweenBullets = 0.15f;
    public float spreadAngle = 3f;
    public float burstRate = 3f;

    [Header("Muzzle flash")]
    public float flashIntensity = 2f;
    public float flashDuration = 0.05f;

    private bool _isBusy = false;
    private float _cooldown = 0f;
    private bool _isContinuousFireMode = false;

    private Vector3 _lastPosition;
    private Vector2 _currentVelocity;

    void Awake()
    {
        if (muzzleFlashLight != null) muzzleFlashLight.intensity = 0f;
        _lastPosition = transform.position;
    }

    void Update()
    {
        // Вычисляем скорость движения для передачи пулям (чтобы стена была ровной)
        _currentVelocity = (transform.position - _lastPosition) / Time.deltaTime;
        _lastPosition = transform.position;

        if (_cooldown > 0f) _cooldown -= Time.deltaTime;

        // === РЕЖИМ ЛОВУШКИ: БЕСКОНЕЧНЫЙ ОГОНЬ ===
        if (_isContinuousFireMode)
        {
            if (_cooldown <= 0f)
            {
                FireOneBullet();
                _cooldown = continuousFireRate;
            }
            return;
        }

        if (_isBusy || _cooldown > 0f) return;

        // Обнаружение игрока
        if (TargetInBeam())
        {
            if (trapController != null) trapController.SpringTheTrap();
            else StartCoroutine(ChargeAndFireNormal());
        }
    }

    private bool TargetInBeam()
    {
        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, firePoint.right, detectionDistance, targetLayer);
        return hit.collider != null;
    }

    // --- МЕТОДЫ ДЛЯ КОНТРОЛЛЕРА ЛОВУШКИ ---

    public void TriggerSynchronizedCharge(float time)
    {
        _isBusy = true;
        if (chargeClip != null && audioSource != null) audioSource.PlayOneShot(chargeClip);
        StartCoroutine(LightChargeRoutine(time));
    }

    public void StartContinuousFire() => _isContinuousFireMode = true;

    // НОВОЕ: Остановка огня в конце пути
    public void StopContinuousFire()
    {
        _isContinuousFireMode = false;
        _isBusy = false; // Позволяет турели снова работать в обычном режиме, если нужно
        if (muzzleFlashLight != null) muzzleFlashLight.intensity = 0f;
        
        Debug.Log($"[Turret] {gameObject.name} огонь прекращен.");
    }

    private IEnumerator LightChargeRoutine(float time)
    {
        float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            if (muzzleFlashLight != null)
                muzzleFlashLight.intensity = Mathf.Lerp(0f, chargeLightIntensity, t / time);
            yield return null;
        }
    }

    // --- ОБЫЧНЫЙ РЕЖИМ ТУРЕЛИ ---

    private IEnumerator ChargeAndFireNormal()
    {
        _isBusy = true;
        if (chargeClip != null && audioSource != null) audioSource.PlayOneShot(chargeClip);

        yield return StartCoroutine(LightChargeRoutine(chargeTime));

        // В обычном режиме стреляем фиксированную очередь
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
        float currentSpread = _isContinuousFireMode ? 0f : spreadAngle;
        float angle = Random.Range(-currentSpread, currentSpread);
        Quaternion rot = firePoint.rotation * Quaternion.Euler(0f, 0f, angle);

        if (bulletPrefab != null)
        {
            GameObject b = Instantiate(bulletPrefab, firePoint.position, rot);

            // Передаем пуле скорость самой ловушки, чтобы пули летели вместе с ней вбок
            if (_isContinuousFireMode)
            {
                Rigidbody2D rb = b.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity += _currentVelocity;
            }
        }

        EmitMuzzle();

        // Звук выстрела только в обычном режиме (в бесконечном звук идет от Rig)
        if (!_isContinuousFireMode && singleShotClip != null && audioSource != null)
            audioSource.PlayOneShot(singleShotClip);

        if (muzzleFlashLight != null) StartCoroutine(MuzzleFlash());
    }

    private void EmitMuzzle()
    {
        if (muzzleParticles == null) return;

        bool inDark = CaveLightController.Instance != null && CaveLightController.Instance.IsInDark;

        var main = muzzleParticles.main;
        Color c = main.startColor.color;
        c.a = inDark ? darkParticleAlpha : 1f;
        main.startColor = c;

        muzzleParticles.Emit(muzzleParticleCount);
    }

    private IEnumerator MuzzleFlash()
    {
        muzzleFlashLight.intensity = flashIntensity;
        yield return new WaitForSeconds(flashDuration);
        if (muzzleFlashLight != null) muzzleFlashLight.intensity = 0f;
    }
}