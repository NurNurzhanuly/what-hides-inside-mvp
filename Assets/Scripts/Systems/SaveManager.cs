using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    // Ключи для PlayerPrefs
    private const string PosXKey = "PlayerPosX";
    private const string PosYKey = "PlayerPosY";
    private const string HasSaveKey = "HasSave";
    
    // НОВЫЙ КЛЮЧ: Запоминаем экспозицию света!
    private const string LightExposureKey = "SavedLightExposure";

    [HideInInspector] 
    public bool isRespawning = false; 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ТЕПЕРЬ МЕТОД ПРИНИМАЕТ НЕ ТОЛЬКО ПОЗИЦИЮ, НО И СВЕТ
    public void SaveCheckpoint(Vector2 position, float currentExposure)
    {
        PlayerPrefs.SetFloat(PosXKey, position.x);
        PlayerPrefs.SetFloat(PosYKey, position.y);
        PlayerPrefs.SetFloat(LightExposureKey, currentExposure); // Сохраняем свет
        PlayerPrefs.SetInt(HasSaveKey, 1);
        PlayerPrefs.Save();
        
        Debug.Log($"[SaveManager] Чекпоинт сохранен. Свет: {currentExposure}");
    }

    public bool HasSavedGame()
    {
        return PlayerPrefs.GetInt(HasSaveKey, 0) == 1;
    }

    public Vector2 LoadCheckpointPosition(Vector2 defaultPosition)
    {
        if (HasSavedGame())
        {
            return new Vector2(PlayerPrefs.GetFloat(PosXKey), PlayerPrefs.GetFloat(PosYKey));
        }
        return defaultPosition;
    }

    // НОВЫЙ МЕТОД: Получаем сохраненный свет
    public float LoadCheckpointExposure(float defaultExposure)
    {
        if (HasSavedGame())
        {
            return PlayerPrefs.GetFloat(LightExposureKey);
        }
        return defaultExposure;
    }

    public void ClearSave()
    {
        PlayerPrefs.DeleteKey(PosXKey);
        PlayerPrefs.DeleteKey(PosYKey);
        PlayerPrefs.DeleteKey(LightExposureKey); // Удаляем свет
        PlayerPrefs.DeleteKey(HasSaveKey);
        PlayerPrefs.Save();
    }
}