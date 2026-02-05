using UnityEngine;

public class Sounds : MonoBehaviour
{
    public static Sounds Instance { get; private set; }

    [Header("Clips")]
    public AudioClip popClip;
    public AudioClip errorClip;

    [Header("Settings")]
    [Range(0f, 1f)] public float popVolume = 0.25f;
    [Range(0f, 1f)] public float errorVolume = 0.35f;

    private AudioSource _audio;

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // AudioSource
        _audio = GetComponent<AudioSource>();
        if (_audio == null)
            _audio = gameObject.AddComponent<AudioSource>();

        _audio.playOnAwake = false;
    }

    public void PlayPop()
    {
        if (popClip == null) return;
        _audio.PlayOneShot(popClip, popVolume);
    }

    public void PlayError()
    {
        if (errorClip == null) return;
        _audio.PlayOneShot(errorClip, errorVolume);
    }
}