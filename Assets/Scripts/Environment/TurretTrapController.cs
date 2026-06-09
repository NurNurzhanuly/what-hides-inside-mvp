using UnityEngine;
using System.Collections;

public class TurretTrapController : MonoBehaviour
{
    [Header("Ловушка")]
    public Turret[] childTurrets;
    public float synchronizedChargeTime = 0.5f;

    [Header("Движение (Waypoint Mover)")]
    public Transform[] waypoints;
    public float moveSpeed = 3f;
    public float rotationSpeed = 90f;

    private bool _isTrapSprung = false;
    private bool _isMoving = false;
    private int _currentIndex = 0;
    private bool _pathCompleted = false; 

    void Start()
    {
        if (waypoints != null && waypoints.Length > 0)
        {
            transform.position = waypoints[0].position;
            transform.rotation = waypoints[0].rotation;
            _currentIndex = 1;
        }
    }

    void Update()
    {
        if (!_isMoving || waypoints == null || waypoints.Length == 0) return;


        if (_currentIndex >= waypoints.Length)
        {
            if (!_pathCompleted) FinishTrap(); 
            return;
        }

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

    private void FinishTrap()
    {
        _pathCompleted = true;
        _isMoving = false;

        foreach (var t in childTurrets)
        {
            t.StopContinuousFire();
        }

        Debug.Log("[TurretTrap] Конечная точка достигнута. Огонь прекращен.");
    }

    public void SpringTheTrap()
    {
        if (_isTrapSprung) return;
        _isTrapSprung = true;
        StartCoroutine(TrapSequence());
    }

    private IEnumerator TrapSequence()
    {
        foreach (var t in childTurrets)
        {
            t.TriggerSynchronizedCharge(synchronizedChargeTime);
        }

        yield return new WaitForSeconds(synchronizedChargeTime);

        foreach (var t in childTurrets)
        {
            t.StartContinuousFire();
        }

        _isMoving = true;
    }
}