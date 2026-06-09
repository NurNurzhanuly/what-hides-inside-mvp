using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private const string PosXKey = "PlayerPosX";
    private const string PosYKey = "PlayerPosY";
    private const string HasSaveKey = "HasSave";
    private const string InDarkKey = "SavedInDark";

    [HideInInspector] public bool isRespawning = false;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    public void SaveCheckpoint(Vector2 position, bool inDark)
    {
        PlayerPrefs.SetFloat(PosXKey, position.x);
        PlayerPrefs.SetFloat(PosYKey, position.y);
        PlayerPrefs.SetInt(InDarkKey, inDark ? 1 : 0);
        PlayerPrefs.SetInt(HasSaveKey, 1);
        PlayerPrefs.Save();
        Debug.Log($"[SaveManager] Чекпоинт сохранён. В темноте: {inDark}");
    }

    public bool HasSavedGame() => PlayerPrefs.GetInt(HasSaveKey, 0) == 1;

    public Vector2 LoadCheckpointPosition(Vector2 defaultPosition)
    {
        if (HasSavedGame())
            return new Vector2(PlayerPrefs.GetFloat(PosXKey), PlayerPrefs.GetFloat(PosYKey));
        return defaultPosition;
    }

    public bool LoadCheckpointInDark() => HasSavedGame() && PlayerPrefs.GetInt(InDarkKey, 0) == 1;


    public bool IsSavedCheckpoint(Vector2 position)
    {
        if (!HasSavedGame()) return false;
        Vector2 saved = new Vector2(PlayerPrefs.GetFloat(PosXKey), PlayerPrefs.GetFloat(PosYKey));
        return Vector2.Distance(saved, position) < 0.1f;
    }

    public void ClearSave()
    {
        PlayerPrefs.DeleteKey(PosXKey);
        PlayerPrefs.DeleteKey(PosYKey);
        PlayerPrefs.DeleteKey(InDarkKey);
        PlayerPrefs.DeleteKey(HasSaveKey);
        PlayerPrefs.Save();
    }
}