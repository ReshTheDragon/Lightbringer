using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;

public class SettingsController : MonoBehaviour
{
    [Header("UI Components")]
    public Toggle fullscreenToggle;
    public Slider volume;
    public Button saveButton;

    [Header("Settings")]
    public Settings gameSettings;
    private string settingsPath;

    void OnEnable()
    {
        settingsPath = Application.persistentDataPath + "/gamesettings.json";

        // Add listeners
        fullscreenToggle.onValueChanged.AddListener(delegate { FullscreenToggle(); });
        volume.onValueChanged.AddListener(delegate { VolumeChange(); });
        saveButton.onClick.AddListener(delegate { SaveSettings(); });

        // Load existing settings
        LoadSettings();
    }

    public void FullscreenToggle()
    {
        gameSettings.fullscreen = fullscreenToggle.isOn;
        Screen.fullScreen = fullscreenToggle.isOn;
    }

    public void VolumeChange()
    {
        gameSettings.volume = volume.value;
        AudioListener.volume = volume.value;
    }

    public void SaveSettings()
    {
        string jsonData = JsonUtility.ToJson(gameSettings, true);
        File.WriteAllText(settingsPath, jsonData);
        Debug.Log("Settings saved successfully!");

        // Close options menu
        if (MenuController.instance != null)
        {
            MenuController.instance.closeOptions();
        }
    }

    public void LoadSettings()
    {
        if (File.Exists(settingsPath))
        {
            try
            {
                string json = File.ReadAllText(settingsPath);
                gameSettings = JsonUtility.FromJson<Settings>(json);
                //Debug.Log("Settings loaded successfully!");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Error loading settings file: " + e.Message + ". Using default settings.");
                gameSettings = new Settings();
            }
        }
        else
        {
            Debug.Log("No settings file found. Creating new default settings.");
            gameSettings = new Settings();
        }

        // Apply settings to UI
        fullscreenToggle.isOn = gameSettings.fullscreen;
        volume.value = gameSettings.volume;

        // Apply settings to game
        ApplySettings();
    }

    private void ApplySettings()
    {
        Screen.fullScreen = gameSettings.fullscreen;
        AudioListener.volume = gameSettings.volume;
    }

    void OnDisable()
    {
        // Remove listeners to prevent memory leaks
        fullscreenToggle.onValueChanged.RemoveAllListeners();
        volume.onValueChanged.RemoveAllListeners();
        saveButton.onClick.RemoveAllListeners();
    }
}