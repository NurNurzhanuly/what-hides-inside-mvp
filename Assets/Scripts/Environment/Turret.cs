using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Turret : MonoBehaviour
{
    [Header("Ловушка (Limbo)")]
    [Tooltip("Укажи здесь Turret_Rig. Если пусто - турель работает в обычном режиме!")]
    public TurretTrapController trapController;
    public float continuousFireRate = 0.1f;

    [Header("Refs")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public Light2D muzzleFlashLight;
    public AudioSource audioSource;

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
    
    // Флаг для бесконечного режима ловушки
    private bool _isContinuousFireMode = false;

    void Awake()
    {
        if (muzzleFlashLight != null) muzzleFlashLight.intensity = 0f;
    }

    void Update()
    {
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

        // === ОБЫЧНЫЙ РЕЖИМ ИЛИ РЕЖИМ ОЖИДАНИЯ ЛОВУШКИ ===
        if (_isBusy || _cooldown > 0f) return;

        if (TargetInBeam())
        {
            if (trapController != null)
            {
                // Если мы часть большой ловушки - докладываем боссу!
                trapController.SpringTheTrap();
            }
            else
            {
                // Если мы обычная одиночная турель - стреляем сами
                StartCoroutine(ChargeAndFireNormal());
            }
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
        _isBusy = true; // Блокируем обычное поведение
        if (chargeClip != null && audioSource != null) audioSource.PlayOneShot(chargeClip);
        StartCoroutine(LightChargeRoutine(time));
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

    public void StartContinuousFire()
    {
        _isContinuousFireMode = true;
    }

    // --- ОБЫЧНЫЙ РЕЖИМ ТУРЕЛИ (Твой старый код) ---

    private IEnumerator ChargeAndFireNormal()
    {
        _isBusy = true;
        if (chargeClip != null && audioSource != null) audioSource.PlayOneShot(chargeClip);

        yield return StartCoroutine(LightChargeRoutine(chargeTime));

        if (!TargetInBeam())
        {
            if (muzzleFlashLight != null) muzzleFlashLight.intensity = 0f;
            _cooldown = burstRate * 0.5f; 
            _isBusy = false;
            yield break;
        }

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