using UnityEngine;

public interface IInputProvider
{
    float GetHorizontalInput();
    bool IsJumpPressed();
    bool IsInteractPressed();
}