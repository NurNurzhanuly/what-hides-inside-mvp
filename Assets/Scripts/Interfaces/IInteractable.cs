using UnityEngine;

public interface IInteractable
{
    bool TryPush(Vector2 direction);
}