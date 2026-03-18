using UnityEngine;

// Этот скрипт реализует интерфейс IInputProvider
public class KeyboardInputReader : MonoBehaviour, IInputProvider
{
    // Метод выдает вектор движения (влево/вправо)
    public Vector2 GetMoveDirection() 
    {
        return new Vector2(Input.GetAxisRaw("Horizontal"), 0);
    }

    // Метод выдает true, если нажат Пробел
    public bool IsJumpPressed() 
    {
        return Input.GetButtonDown("Jump");
    }
}