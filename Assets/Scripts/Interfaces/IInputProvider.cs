using UnityEngine;

public interface IInputProvider
{
    float GetHorizontalInput();
    float GetVerticalInput();
    bool IsJumpPressed();
    bool IsInteractPressed();
}