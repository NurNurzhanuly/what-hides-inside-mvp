using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class Turret : MonoBehaviour
{
    [Header("Настройки выстрела")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float burstRate = 3f;            
    public int bulletsPerBurst = 10;        
    
    [Tooltip("Время между пулями. Скорострельность звука будет зависеть от этого параметра!")]
    public float timeBetweenBullets = 0.15f; 
    public float spreadAngle = 3f;          

    [Header("Эффекты (Синхронные)")]
    public Light2D muzzleFlashLight; 
    public AudioSource shootAudio;   
    
    [Tooltip("ВАЖНО: Сюда нужно положить КОРОТКИЙ звук ОДНОГО выстрела, а не длинную очередь!")]
    public AudioClip singleShotClip; 

    [Header("Сенсор (Лазер)")]
    public float detectionDistance = 15f; 
    public LayerMask targetLayer; 

    private bool _isPlayerDetected = false;
    private bool _isShooting = false;

    void Start()
    {
        if (muzzleFlashLight != null) muzzleFlashLight.intensity = 0f;
    }

    void Update()
    {
        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, firePoint.right, detectionDistance, targetLayer);

        if (hit.collider != null)
        {
            if (!_isShooting)
            {
                _isShooting = true;
                StartCoroutine(BurstRoutine()); 
            }
        }
        else
        {
            _isShooting = false; 
        }
    }

    private IEnumerator BurstRoutine()
    {
        while (_isShooting) 
        {
            for (int i = 0; i < bulletsPerBurst; i++)
            {
                if (!_isShooting) break; 

                if (bulletPrefab != null && firePoint != null)
                {
                    // 1. Создаем пулю
                    float randomAngle = Random.Range(-spreadAngle, spreadAngle);
                    Quaternion rotation = firePoint.rotation * Quaternion.Euler(0, 0, randomAngle);
                    Instantiate(bulletPrefab, firePoint.position, rotation);
                    
                    // 2. ИДЕАЛЬНАЯ СИНХРОНИЗАЦИЯ: В этот же кадр включаем звук и вспышку
                    StartCoroutine(MuzzleFlashAndSound());
                }
                
                // 3. Ждем время до следующей пули (пауза между выстрелами)
                yield return new WaitForSeconds(timeBetweenBullets);
            }

            yield return new WaitForSeconds(burstRate);
        }
    }

    private IEnumerator MuzzleFlashAndSound()
    {
        // ИГРАЕМ ОДИНОЧНЫЙ ЗВУК
        if (shootAudio != null && singleShotClip != null) 
        {
            shootAudio.PlayOneShot(singleShotClip);
        }

        // МИГАЕМ СВЕТОМ
        if (muzzleFlashLight != null) 
        {
            muzzleFlashLight.intensity = 2f; 
            yield return new WaitForSeconds(0.05f); // Короткая вспышка на 50 мс
            muzzleFlashLight.intensity = 0f; 
        }
    }

    private void OnDrawGizmos()
    {
        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(firePoint.position, firePoint.position + firePoint.right * detectionDistance);
        }
    }
}