using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    // Ключи для PlayerPrefs
    private const string PosXKey = "PlayerPosX";
    private const string PosYKey = "PlayerPosY";
    private const string HasSaveKey = "HasSave";

    // Секретная переменная. Если true, значит игрок только что умер и сцена перезагрузилась
    [HideInInspector] 
    public bool isRespawning = false; 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Этот объект НЕ удаляется при перезагрузке сцены
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveCheckpoint(Vector2 position)
    {
        PlayerPrefs.SetFloat(PosXKey, position.x);
        PlayerPrefs.SetFloat(PosYKey, position.y);
        PlayerPrefs.SetInt(HasSaveKey, 1);
        PlayerPrefs.Save();
        
        Debug.Log($"[SaveManager] Чекпоинт сохранен: {position}");
    }

    public bool HasSavedGame()
    {
        return PlayerPrefs.GetInt(HasSaveKey, 0) == 1;
    }

    public Vector2 LoadCheckpoint(Vector2 defaultPosition)
    {
        if (HasSavedGame())
        {
            float x = PlayerPrefs.GetFloat(PosXKey);
            float y = PlayerPrefs.GetFloat(PosYKey);
            return new Vector2(x, y);
        }
        return defaultPosition;
    }

    public void ClearSave()
    {
        PlayerPrefs.DeleteKey(PosXKey);
        PlayerPrefs.DeleteKey(PosYKey);
        PlayerPrefs.DeleteKey(HasSaveKey);
        PlayerPrefs.Save();
        Debug.Log("[SaveManager] Сохранения очищены.");
    }
}