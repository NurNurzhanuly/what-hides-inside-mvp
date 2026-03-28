using UnityEngine;

public class MovingPlatform : TriggerableObject
{
    public Vector3 targetOffset;
    public float speed = 3f;
    private Vector3 _startPos;
    private Vector3 _targetPos;
    private bool _isActive = false;

    void Awake()
    {
        _startPos = transform.position;
        _targetPos = _startPos + targetOffset;
    }

    public override void Activate() => _isActive = true;
    public override void Deactivate() => _isActive = false;

    void Update()
    {
        Vector3 destination = _isActive ? _targetPos : _startPos;
        transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.gameObject.activeInHierarchy)
            {
                collision.transform.SetParent(transform, true);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.gameObject.activeInHierarchy)
            {
                collision.transform.SetParent(null);
            }
        }
    }
    
    private void OnDisable()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && player.transform.parent == transform)
        {
            player.transform.SetParent(null);
        }
    }
}