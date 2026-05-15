using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    private bool _isActivated = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Если игрок коснулся чекпоинта и он еще не активирован
        if (!_isActivated && collision.CompareTag("Player"))
        {
            _isActivated = true;
            
            // Просим SaveManager сохранить позицию ЭТОГО чекпоинта
            // (или можно сохранять collision.transform.position - позицию самого игрока)
            SaveManager.Instance.SaveCheckpoint(transform.position);

            // Опционально: можно добавить визуальный эффект (например, зажечь лампочку)
            // GetComponent<SpriteRenderer>().color = Color.green;
        }
    }
}