using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    private bool _isActivated = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isActivated || !collision.CompareTag("Player")) return;

        if (SaveManager.Instance != null && SaveManager.Instance.IsSavedCheckpoint(transform.position))
        {
            _isActivated = true; 
            Debug.Log("[Checkpoint] это чекпоинт респавна — пропускаю пересохранение");
            return;
        }

        _isActivated = true;
        bool inDark = CaveLightController.Instance != null && CaveLightController.Instance.IsInDark;
        Debug.Log($"[Checkpoint] сохраняю. inDark={inDark}, позиция={transform.position}");
        SaveManager.Instance.SaveCheckpoint(transform.position, inDark);
    }
}