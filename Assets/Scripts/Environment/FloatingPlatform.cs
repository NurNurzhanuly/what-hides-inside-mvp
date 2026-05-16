using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))] // Скрипт сам добавит Rigidbody2D, если его нет
public class FloatingPlatform : MonoBehaviour
{
    [Header("Настройки маршрута")]
    [Tooltip("На сколько сдвинется платформа от начальной точки (X, Y)")]
    public Vector3 targetOffset;
    public float speed = 3f;

    [Header("Настройки паузы")]
    [Tooltip("Сколько секунд платформа стоит на месте, доехав до края")]
    public float waitTimeAtEnds = 0.5f;

    private Vector3 _startPos;
    private Vector3 _targetPos;
    private bool _movingToTarget = true;
    private float _currentWaitTime = 0f;

    // Списки для перевозки игрока и ящиков (чтобы они не скользили по платформе)
    private List<Transform> _passengers = new List<Transform>();
    private Vector3 _previousPos;
    private Rigidbody2D _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        // Обязательно делаем платформу кинематической, чтобы она не падала
        _rb.bodyType = RigidbodyType2D.Kinematic; 
        
        _startPos = transform.position;
        _targetPos = _startPos + targetOffset;
        _previousPos = transform.position;
    }

    void FixedUpdate()
    {
        // 1. Обработка паузы на краях
        if (_currentWaitTime > 0)
        {
            _currentWaitTime -= Time.fixedDeltaTime;
            _previousPos = transform.position; // Запоминаем позицию стоянки
            return;
        }

        // 2. Выбор цели (едем к B или возвращаемся к A)
        Vector3 destination = _movingToTarget ? _targetPos : _startPos;

        // 3. Физически двигаем платформу
        _rb.MovePosition(Vector3.MoveTowards(transform.position, destination, speed * Time.fixedDeltaTime));

        // 4. Проверка: доехали ли?
        if (Vector3.Distance(transform.position, destination) < 0.01f)
        {
            _movingToTarget = !_movingToTarget; // Разворот
            _currentWaitTime = waitTimeAtEnds;  // Запускаем таймер паузы
        }

        // 5. Двигаем пассажиров (Игрок, Ящик и т.д.)
        Vector3 delta = transform.position - _previousPos;
        for (int i = _passengers.Count - 1; i >= 0; i--)
        {
            if (_passengers[i] != null)
            {
                _passengers[i].position += delta;
            }
            else
            {
                _passengers.RemoveAt(i); // Очищаем мертвых/уничтоженных пассажиров
            }
        }
        
        _previousPos = transform.position;
    }

    // Игрок или ящик наступил на платформу
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Перевозим всё, что имеет Rigidbody (игрока, ящики, капканы)
        if (collision.rigidbody != null && !_passengers.Contains(collision.transform))
        {
            _passengers.Add(collision.transform);
        }
    }

    // Игрок или ящик спрыгнул
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.rigidbody != null && _passengers.Contains(collision.transform))
        {
            _passengers.Remove(collision.transform);
        }
    }
}