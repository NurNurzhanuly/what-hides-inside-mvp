using UnityEngine;
using System.Collections;

public class TurretTrapController : MonoBehaviour
{
    [Header("Ловушка")]
    [Tooltip("Пулеметы, которые прикреплены к этой ловушке")]
    public Turret[] childTurrets;
    [Tooltip("Сколько секунд пулеметы заряжаются перед началом мясорубки")]
    public float synchronizedChargeTime = 0.5f;

    [Header("Движение (Waypoint Mover)")]
    public Transform[] waypoints;
    public float moveSpeed = 3f;
    public float rotationSpeed = 90f;

    private bool _isTrapSprung = false;
    private bool _isMoving = false;
    private int _currentIndex = 0;

    void Start()
    {
        // При старте ставим риг на первую точку
        if (waypoints != null && waypoints.Length > 0)
        {
            transform.position = waypoints[0].position;
            transform.rotation = waypoints[0].rotation;
            _currentIndex = 1;
        }
    }

    void Update()
    {
        // Движение по точкам работает только после срабатывания ловушки
        if (!_isMoving || waypoints == null || waypoints.Length == 0) return;
        if (_currentIndex >= waypoints.Length) return;

        Transform targetWP = waypoints[_currentIndex];

        transform.position = Vector3.MoveTowards(transform.position, targetWP.position, moveSpeed * Time.deltaTime);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetWP.rotation, rotationSpeed * Time.deltaTime);

        float dist = Vector3.Distance(transform.position, targetWP.position);
        float angle = Quaternion.Angle(transform.rotation, targetWP.rotation);

        if (dist < 0.01f && angle < 0.1f)
        {
            _currentIndex++;
        }
    }

    // Эту функцию вызовет пулемет, когда его луч заденет игрока
    public void SpringTheTrap()
    {
        if (_isTrapSprung) return; // Ловушка срабатывает только один раз
        _isTrapSprung = true;

        Debug.Log("[TurretTrap] Ловушка захлопнулась! Начинаем синхронную зарядку.");
        StartCoroutine(TrapSequence());
    }

    private IEnumerator TrapSequence()
    {
        // 1. Даем команду всем пулеметам начать зарядку (визуально пугаем игрока)
        foreach (var t in childTurrets)
        {
            t.TriggerSynchronizedCharge(synchronizedChargeTime);
        }

        // Ждем пока они зарядятся
        yield return new WaitForSeconds(synchronizedChargeTime);

        // 2. Даем команду всем пулеметам стрелять БЕСКОНЕЧНО
        foreach (var t in childTurrets)
        {
            t.StartContinuousFire();
        }

        // 3. Поехали!
        _isMoving = true;
    }
}