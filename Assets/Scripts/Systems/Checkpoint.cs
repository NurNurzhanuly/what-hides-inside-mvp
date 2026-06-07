using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    private bool _isActivated = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isActivated || !collision.CompareTag("Player")) return;
        _isActivated = true;

        bool inDark = CaveLightController.Instance != null && CaveLightController.Instance.IsInDark;
        SaveManager.Instance.SaveCheckpoint(transform.position, inDark);
    }
}