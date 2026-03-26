using UnityEngine;

public class KeyboardInputReader : MonoBehaviour, IInputProvider
{
    public float GetHorizontalInput()
    {
        return Input.GetAxisRaw("Horizontal");
    }

    public bool IsJumpPressed()
    {
        return Input.GetButtonDown("Jump");
    }

    public bool IsInteractPressed()
    {
        return Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.LeftControl);
    }
}