using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

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
        ApplyAllSettings();
    }
    #endregion

    public enum VolumeSetting { MainVolume, MusicVolume, SFXVolume }
    public GameSettingsData Data = new GameSettingsData();
    private string SavePath => Path.Combine(Application.persistentDataPath, "settings.json");

    public void ApplyAllSettings()
    {
        // Apply Audio
        SetVolume(VolumeSetting.MainVolume, Data.masterVolume);
        SetVolume(VolumeSetting.MusicVolume, Data.musicVolume);
        SetVolume(VolumeSetting.SFXVolume, Data.sfxVolume);

        // Apply Video
        ApplyVideoSettings();

        // Apply Post-Processing
        Volume globalVolume = FindFirstObjectByType<Volume>();
        if (globalVolume != null)
        {
            SetEffectActive<Bloom>(Data.bloomActive);
            SetEffectActive<ChromaticAberration>(Data.chromaticActive);
            SetEffectActive<LensDistortion>(Data.lensDistortionActive);
        }
    }

    public void ApplyVideoSettings()
    {
        QualitySettings.vSyncCount = Data.vsync ? 1 : 0;
        Screen.fullScreen = Data.isFullscreen;

        if (Data.fpsCap == 0)
            Application.targetFrameRate = -1;
        else
            Application.targetFrameRate = Data.fpsCap;
    }

    public void SetVolume(VolumeSetting type, float value)
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

        FMOD.Studio.Bus bus = FMODUnity.RuntimeManager.GetBus(busPath);
        if (bus.isValid()) bus.setVolume(value);
    }

    public void SetEffectActive<T>(bool active) where T : VolumeComponent
    {
        Volume globalVolume = FindFirstObjectByType<Volume>();
        if (globalVolume != null && globalVolume.profile.TryGet<T>(out var effect))
        {
            effect.active = active;
            if (typeof(T) == typeof(Bloom)) Data.bloomActive = active;
            else if (typeof(T) == typeof(ChromaticAberration)) Data.chromaticActive = active;
            else if (typeof(T) == typeof(LensDistortion)) Data.lensDistortionActive = active;
        }
    }

    public void Save()
    {
        string json = JsonUtility.ToJson(Data, true);
        File.WriteAllText(SavePath, json);

        GameObject screen = FindInactiveObjectByName("SettingsScreen");
        if (screen != null) screen.SetActive(false);

        UIManager.Instance?.FocusUI();
        GameObject mainScreen = FindInactiveObjectByName("MainScreen");
        if (mainScreen != null) mainScreen.SetActive(true);
    }

    public void Load()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            Data = JsonUtility.FromJson<GameSettingsData>(json);
        }
    }

    public void Open()
    {
        GameObject screen = FindInactiveObjectByName("SettingsScreen");
        if (screen != null) screen.SetActive(true);
    }

    public void Close()
    {
        Load();
        ApplyAllSettings();

        GameObject screen = FindInactiveObjectByName("SettingsScreen");
        if (screen != null) screen.SetActive(false);

        UIManager.Instance?.FocusUI();
        GameObject mainScreen = FindInactiveObjectByName("MainScreen");
        if (mainScreen != null) mainScreen.SetActive(true);
    }

    private GameObject FindInactiveObjectByName(string name)
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject go in allObjects)
        {
            // We only care about objects that are actually in the scene (not prefabs)
            if (go.name == name && go.scene.isLoaded)
            {
                return go;
            }
        }
        return null;
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
    public int fpsCap = 0;
    public bool bloomActive = true;
    public bool chromaticActive = true;
    public bool lensDistortionActive = true;
}
