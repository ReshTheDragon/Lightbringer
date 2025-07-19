using UnityEngine;

[System.Serializable]
public class Settings
{
    [Header("Display Settings")]
    public bool fullscreen = true;

    [Header("Audio Settings")]
    [Range(0f, 1f)]
    public float volume = 1f;

    // Constructor with default values
    public Settings()
    {
        fullscreen = true;
        volume = 1f;
    }

    // Constructor with custom values
    public Settings(bool fullscreen, float volume)
    {
        this.fullscreen = fullscreen;
        this.volume = volume;
    }
}