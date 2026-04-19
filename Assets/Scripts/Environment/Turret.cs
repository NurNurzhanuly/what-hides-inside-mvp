using UnityEngine;
using System.Collections;

public class Turret : MonoBehaviour
{
    [Header("Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    
    [Header("Burst Settings")]
    public float burstRate = 3f;            // Как часто повторяется вся очередь
    public int bulletsPerBurst = 10;        // Количество пуль в одной очереди
    public float timeBetweenBullets = 0.15f; // Пауза между выстрелами внутри очереди

    [Header("Accuracy")]
    public float spreadAngle = 3f;          // Максимальное отклонение в градусах

    void Start()
    {
        StartCoroutine(BurstRoutine());
    }

    private IEnumerator BurstRoutine()
    {
        while (true)
        {
            // Стреляем очередь
            for (int i = 0; i < bulletsPerBurst; i++)
            {
                if (bulletPrefab != null && firePoint != null)
                {
                    // Вычисляем случайный угол отклонения
                    float randomAngle = Random.Range(-spreadAngle, spreadAngle);
                    
                    // Создаем итоговый поворот на основе FirePoint + случайный разброс
                    Quaternion rotation = firePoint.rotation * Quaternion.Euler(0, 0, randomAngle);
                    
                    // Спавним пулю
                    Instantiate(bulletPrefab, firePoint.position, rotation);
                }
                
                // Ждем время перед следующим выстрелом в этой же очереди
                yield return new WaitForSeconds(timeBetweenBullets);
            }

            // Ждем время перед началом следующей очереди
            yield return new WaitForSeconds(burstRate);
        }
    }
}