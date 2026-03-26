using UnityEngine;

public class KeyboardInputReader : MonoBehaviour, IInputProvider
{
    public float GetHorizontalInput() => Input.GetAxisRaw("Horizontal");
    public float GetVerticalInput() => Input.GetAxisRaw("Vertical"); // ПРОВЕРЬ ЭТУ СТРОКУ
    public bool IsJumpPressed() => Input.GetButtonDown("Jump");
    public bool IsInteractPressed() => Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.LeftControl);
}