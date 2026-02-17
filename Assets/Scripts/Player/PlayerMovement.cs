using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    private Rigidbody2D _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        _rb.linearVelocity = new Vector2(moveX * moveSpeed, _rb.linearVelocity.y);

        // Прыжок
        if (Input.GetButtonDown("Jump") && Mathf.Abs(_rb.linearVelocity.y) < 0.01f)
        {
            _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }
}