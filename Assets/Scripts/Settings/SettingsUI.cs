using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private Slider mainVolumeSlider, musicVolumeSlider, sfxVolumeSlider;

    [Header("Video")]
    [SerializeField] private Toggle vsyncToggle, fullscreenToggle;
    [SerializeField] private TMP_Dropdown resolutionDropdown, fpsDropdown;

    [Header("Post-Processing")]
    [SerializeField] private Toggle bloomToggle, chromaticToggle, lensDistortionToggle;

    private void Start()
    {
        InitializeUI();
    }

    private void OnEnable()
    {
        // Sync UI with current data every time the panel is opened
        InitializeUI();
    }

    private void InitializeUI()
    {
        var data = SettingsManager.Instance.Data;

        // Audio
        mainVolumeSlider.value = data.masterVolume;
        musicVolumeSlider.value = data.musicVolume;
        sfxVolumeSlider.value = data.sfxVolume;

        mainVolumeSlider.onValueChanged.AddListener(val => SettingsManager.Instance.SetVolume(SettingsManager.VolumeSetting.MainVolume, val));
        musicVolumeSlider.onValueChanged.AddListener(val => SettingsManager.Instance.SetVolume(SettingsManager.VolumeSetting.MusicVolume, val));
        sfxVolumeSlider.onValueChanged.AddListener(val => SettingsManager.Instance.SetVolume(SettingsManager.VolumeSetting.SFXVolume, val));

        // Video
        vsyncToggle.isOn = data.vsync;
        fullscreenToggle.isOn = data.isFullscreen;
        vsyncToggle.onValueChanged.AddListener(val => { SettingsManager.Instance.Data.vsync = val; SettingsManager.Instance.ApplyVideoSettings(); });
        fullscreenToggle.onValueChanged.AddListener(val => { SettingsManager.Instance.Data.isFullscreen = val; SettingsManager.Instance.ApplyVideoSettings(); });

        SetupFPSDropdown();
        SetupResolutionDropdown();
    }

    private void SetupFPSDropdown()
    {
        string[] fpsValues = { "0", "30", "60", "120", "144", "240" };
        fpsDropdown.ClearOptions();
        fpsDropdown.AddOptions(new List<string>(fpsValues));

        int currentIdx = fpsDropdown.options.FindIndex(opt => opt.text == SettingsManager.Instance.Data.fpsCap.ToString());
        fpsDropdown.value = currentIdx != -1 ? currentIdx : 2;

        fpsDropdown.onValueChanged.AddListener(val => {
            if (int.TryParse(fpsDropdown.options[val].text, out int fps))
                SettingsManager.Instance.Data.fpsCap = fps;
            SettingsManager.Instance.ApplyVideoSettings();
        });
    }

    private void SetupResolutionDropdown()
    {
        Resolution[] uniqueResolutions = Screen.resolutions
            .GroupBy(res => new { res.width, res.height })
            .Select(g => g.Last())
            .ToArray();

        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        int currentIdx = 0;

        for (int i = 0; i < uniqueResolutions.Length; i++)
        {
            string opt = $"{uniqueResolutions[i].width} x {uniqueResolutions[i].height}";
            options.Add(opt);

            if (uniqueResolutions[i].width == Screen.currentResolution.width &&
                uniqueResolutions[i].height == Screen.currentResolution.height)
            {
                currentIdx = i;
            }
        }

        resolutionDropdown.AddOptions(options);

        resolutionDropdown.value = SettingsManager.Instance.Data.resolutionIndex != 0
            ? SettingsManager.Instance.Data.resolutionIndex
            : currentIdx;

        resolutionDropdown.onValueChanged.AddListener(val => {
            Resolution res = uniqueResolutions[val];
            Screen.SetResolution(res.width, res.height, Screen.fullScreen);
            SettingsManager.Instance.Data.resolutionIndex = val;
        });
    }

    public void SaveAndClose()
    {
        SettingsManager.Instance.Save();
        SettingsManager.Instance.ApplyAllSettings();
        // Call UIManager to close screen or similar
    }
}
