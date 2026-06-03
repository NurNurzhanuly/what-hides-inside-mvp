using UnityEngine;

public class KeyboardInputReader : MonoBehaviour, IInputProvider
{
    public float GetHorizontalInput()
    {
        return Input.GetAxisRaw("Horizontal");
    }

    public float GetVerticalInput()
    {
        return Input.GetAxisRaw("Vertical");
    }

    public bool IsJumpPressed()
    {
        return Input.GetButtonDown("Jump");
    }

    // Зажатие — нужно для перетаскивания (держишь = тащишь)
    public bool IsInteractPressed()
    {
        return Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.LeftControl);
    }

    // Нажатие только в кадре нажатия — рычаг переключится один раз
    public bool IsInteractDown()
    {
        return Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.LeftControl);
    }
}