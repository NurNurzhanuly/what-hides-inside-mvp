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
    public float timeBetweenBullets = 0.15f; 
    public float spreadAngle = 3f;          

    [Header("Эффекты")]
    public Light2D muzzleFlashLight; 
    public AudioSource shootAudio;   
    public AudioClip shootClip;

    [Header("Сенсор (Лазер)")]
    public float detectionDistance = 15f; // Длина лазера
    public LayerMask targetLayer; // Поставь тут галочку на Player!

    private bool _isPlayerDetected = false;
    private bool _isShooting = false;

    void Start()
    {
        if (muzzleFlashLight != null) muzzleFlashLight.intensity = 0f;
    }

    void Update()
    {
        // Пускаем луч (лазер) вперед из дула
        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, firePoint.right, detectionDistance, targetLayer);

        // Если луч попал в Игрока
        if (hit.collider != null)
        {
            if (!_isShooting)
            {
                _isShooting = true;
                StartCoroutine(BurstRoutine()); // Начинаем стрелять
            }
        }
        else
        {
            _isShooting = false; // Игрок убежал - перестаем стрелять
        }
    }

    private IEnumerator BurstRoutine()
    {
        while (_isShooting) // Стреляем, ТОЛЬКО ПОКА игрок в лазере
        {
            for (int i = 0; i < bulletsPerBurst; i++)
            {
                if (!_isShooting) break; // Прерываем очередь, если игрок ушел

                if (bulletPrefab != null && firePoint != null)
                {
                    float randomAngle = Random.Range(-spreadAngle, spreadAngle);
                    Quaternion rotation = firePoint.rotation * Quaternion.Euler(0, 0, randomAngle);
                    Instantiate(bulletPrefab, firePoint.position, rotation);
                    
                    StartCoroutine(MuzzleFlashEffect());
                }
                yield return new WaitForSeconds(timeBetweenBullets);
            }
            yield return new WaitForSeconds(burstRate);
        }
    }

    private IEnumerator MuzzleFlashEffect()
    {
        if (shootAudio != null && shootClip != null) shootAudio.PlayOneShot(shootClip);
        if (muzzleFlashLight != null) 
        {
            muzzleFlashLight.intensity = 2f; 
            yield return new WaitForSeconds(0.05f); 
            muzzleFlashLight.intensity = 0f; 
        }
    }

    // Для удобства: рисуем лазер в редакторе красной линией
    private void OnDrawGizmos()
    {
        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(firePoint.position, firePoint.position + firePoint.right * detectionDistance);
        }
    }
}