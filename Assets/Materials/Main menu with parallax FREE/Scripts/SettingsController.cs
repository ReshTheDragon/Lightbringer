using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;

public class SettingsController : MonoBehaviour
{
    public Toggle fullscreenToggle;
    public Dropdown resolutionDrop;
    public Dropdown textQualityDrop;
    public Dropdown antialiasingDrop;
    public Dropdown vSyncDrop;
    public Slider volume;
    public Button saveButton;
    public Resolution[] resolutions;
    public Settings gameSettings;

    private string settingsPath;

    void OnEnable()
    {
        settingsPath = Application.persistentDataPath + "/gamesettings.json";
        fullscreenToggle.onValueChanged.AddListener(delegate { FullscreenToggle(); });
        volume.onValueChanged.AddListener(delegate { VolumeChange(); });
        saveButton.onClick.AddListener(delegate { saveSettings(); });
        loadSettings();
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

    public void saveSettings()
    {
        string jsonData = JsonUtility.ToJson(gameSettings, true);
        File.WriteAllText(settingsPath, jsonData);
        MenuController.instance.closeOptions();
    }

    public void loadSettings()
    {
        if (File.Exists(settingsPath))
        {
            try
            {
                string json = File.ReadAllText(settingsPath);
                gameSettings = JsonUtility.FromJson<Settings>(json);
            }
            catch
            {
                Debug.LogWarning("Lỗi đọc file cài đặt. Sử dụng mặc định.");
                gameSettings = new Settings();
            }
        }
        else
        {
            Debug.Log("Chưa có file cài đặt. Tạo mới.");
            gameSettings = new Settings();
        }

        // Gán lại UI từ giá trị settings
        fullscreenToggle.isOn = gameSettings.fullscreen;
        volume.value = gameSettings.volume;
        // Áp dụng settings
        FullscreenToggle();
        VolumeChange();
    }
}
