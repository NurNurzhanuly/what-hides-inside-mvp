using UnityEngine;
using System.IO;

public class Logger : MonoBehaviour
{
    public static Logger Instance { get; private set; }
    private string _filePath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _filePath = Path.Combine(Application.persistentDataPath, "game_log.txt");
            File.WriteAllText(_filePath, "=== Log Started: " + System.DateTime.Now + " ===\n");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Log(string message)
    {
        string logLine = $"[{System.DateTime.Now:HH:mm:ss}] {message}\n";
        File.AppendAllText(_filePath, logLine);
        Debug.Log(logLine);
    }
}