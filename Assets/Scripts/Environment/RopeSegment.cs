using UnityEngine;

public class RopeSegment : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement movement = other.GetComponent<PlayerMovement>();
            if (movement != null) movement.SetOnRope(true, GetComponent<Rigidbody2D>());
        }
    }
}