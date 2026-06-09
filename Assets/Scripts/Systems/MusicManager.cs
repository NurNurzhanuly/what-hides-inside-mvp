using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Settings")]
    public AudioClip ambientClip;
    [Range(0f, 1f)] public float volume = 0.5f;

    private AudioSource _audioSource;

    private void Awake()
    {
        // Логика "Одиночки" (Singleton): если объект уже есть, новый удаляется
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ВАЖНО: музыка не удалится при перезагрузке сцены
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Настройка AudioSource
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.clip = ambientClip;
        _audioSource.loop = true; // Зацикливаем
        _audioSource.volume = volume;
        _audioSource.spatialBlend = 0f; // 2D звук (звучит везде одинаково)
        _audioSource.playOnAwake = false;

        if (ambientClip != null)
        {
            _audioSource.Play();
        }
    }

    // Метод для смены музыки (если понадобится в будущем)
    public void ChangeMusic(AudioClip newClip)
    {
        if (_audioSource.clip == newClip) return;
        _audioSource.Stop();
        _audioSource.clip = newClip;
        _audioSource.Play();
    }
}