using UnityEngine;

public interface IInputProvider
{
    float GetHorizontalInput();
    float GetVerticalInput();
    bool IsJumpPressed();
    bool IsInteractPressed();   // зажато — для перетаскивания ящиков
    bool IsInteractDown();      // нажато в этом кадре — для рычагов/рубильников
}