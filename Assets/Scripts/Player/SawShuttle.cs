using System.Collections;
using UnityEngine;

// Челночное движение пилы: медленно вперёд (рабочий ход), быстро назад (откат).
// Вешать на ТУ ЖЕ пилу. У её RotatingSaw выключи isPatrolling, вращение/урон оставь.
public class SawShuttle : MonoBehaviour
{
    [Tooltip("Смещение точки B от стартовой точки A. Для горизонтали: (10,0,0)")]
    public Vector3 offset = new Vector3(10f, 0f, 0f);

    [Tooltip("Скорость рабочего хода A -> B (медленно)")]
    public float forwardSpeed = 2f;
    [Tooltip("Скорость отката B -> A (быстро)")]
    public float returnSpeed = 8f;

    [Tooltip("Пауза у точки B перед откатом, сек")]
    public float waitAtB = 0.3f;
    [Tooltip("Пауза у точки A перед новым ходом, сек")]
    public float waitAtA = 0.5f;

    private Vector3 _pointA;
    private Vector3 _pointB;

    void Start()
    {
        _pointA = transform.position;
        _pointB = _pointA + offset;
        StartCoroutine(Loop());
    }

    private IEnumerator Loop()
    {
        while (true)
        {
            yield return MoveTo(_pointB, forwardSpeed);   // медленно вперёд
            if (waitAtB > 0f) yield return new WaitForSeconds(waitAtB);

            yield return MoveTo(_pointA, returnSpeed);     // быстро назад
            if (waitAtA > 0f) yield return new WaitForSeconds(waitAtA);
        }
    }

    private IEnumerator MoveTo(Vector3 target, float speed)
    {
        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            yield return null;
        }
        transform.position = target;
    }
}