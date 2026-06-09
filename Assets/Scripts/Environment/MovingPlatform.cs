using UnityEngine;
using System.Collections.Generic;

public class MovingPlatform : TriggerableObject
{
    public Vector3 targetOffset;
    public float speed = 3f;
    private Vector3 _startPos;
    private Vector3 _targetPos;
    private bool _isActive = false;

    private List<Transform> _passengers = new List<Transform>();
    private Vector3 _previousPos;

    void Awake()
    {
        _startPos = transform.position;
        _targetPos = _startPos + targetOffset;
        _previousPos = transform.position;
    }

    public override void Activate() => _isActive = true;
    public override void Deactivate() => _isActive = false;

    void Update()
    {
        Vector3 destination = _isActive ? _targetPos : _startPos;
        transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
        

        Vector3 delta = transform.position - _previousPos;
        

        for (int i = _passengers.Count - 1; i >= 0; i--)
        {
            if (_passengers[i] != null)
            {
                _passengers[i].position += delta;
            }
            else
            {
                _passengers.RemoveAt(i);
            }
        }
        
        _previousPos = transform.position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (!_passengers.Contains(collision.transform))
            {
                _passengers.Add(collision.transform);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (_passengers.Contains(collision.transform))
            {
                _passengers.Remove(collision.transform);
            }
        }
    }
}