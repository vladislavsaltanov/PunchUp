using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    #region Singleton
    public static SettingsManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Load();
    }
    #endregion
    [SerializeField] private Volume globalVolume;

    [SerializeField] Toggle bloomToggle, chromaticToggle, lensDistortionToggle, vsyncToggle, fullscreenToggle;
    [SerializeField] Slider mainVolumeSlider, musicVolumeSlider, sfxVolumeSlider;

    [Space(10)]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown fpsDropdown;

    [Space(20)]
    [SerializeField] GameObject settingsScreen;

    
    public enum VolumeSetting { MainVolume, MusicVolume, SFXVolume }

    public GameSettingsData Data = new GameSettingsData();
    private string SavePath => Path.Combine(Application.persistentDataPath, "settings.json");

    void Start()
    {
        ApplySettingsToUI();

        mainVolumeSlider.onValueChanged.AddListener(val => SetVolume(VolumeSetting.MainVolume, val));
        musicVolumeSlider.onValueChanged.AddListener(val => SetVolume(VolumeSetting.MusicVolume, val));
        sfxVolumeSlider.onValueChanged.AddListener(val => SetVolume(VolumeSetting.SFXVolume, val));

        SetupResolutionDropdown();

        if (globalVolume != null)
        {
            bloomToggle.isOn = IsEffectActive<Bloom>();
            chromaticToggle.isOn = IsEffectActive<ChromaticAberration>();
            lensDistortionToggle.isOn = IsEffectActive<LensDistortion>();

            bloomToggle.onValueChanged.AddListener(val => SetEffectActive<Bloom>(val));
            chromaticToggle.onValueChanged.AddListener(val => SetEffectActive<ChromaticAberration>(val));
            lensDistortionToggle.onValueChanged.AddListener(val => SetEffectActive<LensDistortion>(val));
        }

        vsyncToggle.onValueChanged.AddListener(val => {
            Data.vsync = val; ApplyVideoSettings();
        });
        fullscreenToggle.onValueChanged.AddListener(val => {
            Data.isFullscreen = val;
            ApplyVideoSettings();
        });

        if (fpsDropdown != null)
        {
            SetupFPSDropdown();

            int index = fpsDropdown.options.FindIndex(opt => opt.text == Data.fpsCap.ToString());
            fpsDropdown.value = index != -1 ? index : 0;

            fpsDropdown.onValueChanged.AddListener(val => {
                if (int.TryParse(fpsDropdown.options[val].text, out int fps))
                    Data.fpsCap = fps;
                else
                    Data.fpsCap = 0;
                ApplyVideoSettings();
            });
        }

        ApplyAllSettings();
    }
    void SetupFPSDropdown()
    {
        if (fpsDropdown == null) return;

        string[] fpsValues = { "0", "30", "60", "120", "144", "240" };

        fpsDropdown.ClearOptions();
        List<string> options = new List<string>(fpsValues);
        fpsDropdown.AddOptions(options);

        int currentIdx = options.IndexOf(Data.fpsCap.ToString());
        fpsDropdown.value = currentIdx != -1 ? currentIdx : 2;
    }
    private void ApplySettingsToUI()
    {
        mainVolumeSlider.value = Data.masterVolume;
        musicVolumeSlider.value = Data.musicVolume;
        sfxVolumeSlider.value = Data.sfxVolume;

        bloomToggle.isOn = Data.bloomActive;
        chromaticToggle.isOn = Data.chromaticActive;
        lensDistortionToggle.isOn = Data.lensDistortionActive;

        vsyncToggle.isOn = Data.vsync;
        fullscreenToggle.isOn = Data.isFullscreen;
    }

    private void SetupResolutionDropdown()
    {
        Resolution[] resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = $"{resolutions[i].width} x {resolutions[i].height}";
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
                currentResolutionIndex = i;
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = Data.resolutionIndex != 0 ? Data.resolutionIndex :
currentResolutionIndex;
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }

    void SetResolution(int resolutionIndex)
    {
        Resolution res = Screen.resolutions[resolutionIndex];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        Data.resolutionIndex = resolutionIndex;
    }

    void ApplyVideoSettings()
    {
        QualitySettings.vSyncCount = Data.vsync ? 1 : 0;
        Screen.fullScreen = Data.isFullscreen;

        if (Data.fpsCap == 0)
            Application.targetFrameRate = -1;
        else
            Application.targetFrameRate = Data.fpsCap;
    }

    void SetVolume(VolumeSetting type, float value)
    {
        string busPath = "";
        switch (type)
        {
            case VolumeSetting.MainVolume:
                Data.masterVolume = value;
                busPath = "bus:/";
                break;
            case VolumeSetting.MusicVolume:
                Data.musicVolume = value;
                busPath = "bus:/Music";
                break;
            case VolumeSetting.SFXVolume:
                Data.sfxVolume = value;
                busPath = "bus:/SFX";
                break;
        }

        // Apply to FMOD
        FMOD.Studio.Bus bus = FMODUnity.RuntimeManager.GetBus(busPath);
        if (bus.isValid()) bus.setVolume(value);
    }

    public void ApplyAllSettings()
    {
        // Apply Audio
        SetVolume(VolumeSetting.MainVolume, Data.masterVolume);
        SetVolume(VolumeSetting.MusicVolume, Data.musicVolume);
        SetVolume(VolumeSetting.SFXVolume, Data.sfxVolume);

        // Apply Video
        ApplyVideoSettings();

        // Apply Post-Processing
        if (globalVolume != null)
        {
            SetEffectActive<Bloom>(Data.bloomActive);
            SetEffectActive<ChromaticAberration>(Data.chromaticActive);
            SetEffectActive<LensDistortion>(Data.lensDistortionActive);
        }
    }

    private bool IsEffectActive<T>() where T : VolumeComponent
    {
        if (globalVolume.profile.TryGet<T>(out var effect))
            return effect.active;
        return false;
    }

    private void SetEffectActive<T>(bool active) where T : VolumeComponent
    {
        if (globalVolume.profile.TryGet<T>(out var effect))
        {
            effect.active = active;
            // Update data object
            if (typeof(T) == typeof(Bloom)) Data.bloomActive = active;
            else if (typeof(T) == typeof(ChromaticAberration)) Data.chromaticActive = active;
            else if (typeof(T) == typeof(LensDistortion)) Data.lensDistortionActive = active;
        }
    }

    public void Open()
    {
        ApplySettingsToUI();
        if (settingsScreen != null) settingsScreen.SetActive(true);
    }

    public void Close()
    {
        Load();
        ApplyAllSettings();

        resolutionDropdown.value = Data.resolutionIndex;
         
        SetVolume(VolumeSetting.MainVolume, Data.masterVolume);
        SetVolume(VolumeSetting.MusicVolume, Data.musicVolume);
        SetVolume(VolumeSetting.SFXVolume, Data.sfxVolume);

        ApplySettingsToUI();
        SetupFPSDropdown();

        UIManager.Instance?.FocusUI();

        if (settingsScreen != null) settingsScreen.SetActive(false);
    }

    public void Save()
    {
        string json = JsonUtility.ToJson(Data, true);
        File.WriteAllText(SavePath, json);

        Close();
    }

    public void Load()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            Data = JsonUtility.FromJson<GameSettingsData>(json);
        }
    }
}
[System.Serializable]
public class GameSettingsData
{
    public float masterVolume = 1f;
    public float musicVolume = 1f;
    public float sfxVolume = 1f;

    public int resolutionIndex = 0;
    public bool isFullscreen = true;
    public bool vsync = true;
    public int fpsCap = 0; // 0 - off, 30, 60, 120, 144, 240

    public bool bloomActive = true;
    public bool chromaticActive = true;
    public bool lensDistortionActive = true;
}
