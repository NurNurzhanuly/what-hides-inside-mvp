using UnityEngine;

public class KeyboardInputReader : MonoBehaviour, IInputProvider
{
    public float GetHorizontalInput() => Input.GetAxisRaw("Horizontal");
    public float GetVerticalInput() => Input.GetAxisRaw("Vertical");
    public bool IsJumpPressed() => Input.GetButtonDown("Jump");
    public bool IsInteractPressed() => Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.LeftControl);
    public bool IsInteractDown() => Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.LeftControl);
}