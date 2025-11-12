using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GlobalEventHandler : MonoBehaviour
{
    #region Singleton
    public static GlobalEventHandler Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (actionEventPairs.Count == 0)
            LoadEventsFromJson();
    }
    #endregion
    #region Saving and restoring events
    [SerializeField]
    string eventsFilePath = "/globalEventsNames.json";

    // Saving events to json
    [ContextMenu("Save to JSON")]
    public void SaveEventsToJson()
    {
        string json = JsonConvert.SerializeObject(actionEventPairs);

        try
        {
            System.IO.File.WriteAllText(Application.persistentDataPath + eventsFilePath, json);
            Debug.Log("Events saved to JSON successfully.\n" + Application.persistentDataPath + eventsFilePath);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save events to JSON: {e.Message}");
        }
    }

    // Loading events from json
    [ContextMenu("Load events from JSON")]
    public void LoadEventsFromJson()
    {
        try
        {
            if (System.IO.File.Exists(Application.persistentDataPath + eventsFilePath))
            {
                string json = System.IO.File.ReadAllText(Application.persistentDataPath + eventsFilePath);
                actionEventPairs = JsonConvert.DeserializeObject<List<ActionEventPair>>(json);
                Debug.Log("Events loaded from JSON successfully.");
            }
            else
            {
                Debug.LogWarning("Events file not found. Using default events.");
                actionEventPairs = new List<ActionEventPair>();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load events from JSON: {e.Message}");
        }
    }
    #endregion

    public List<ActionEventPair> actionEventPairs = new List<ActionEventPair>();

    public Action GetActionByName(string actionName) => actionEventPairs.Find(pair => pair.actionName == actionName).action;
}

[Serializable]
public struct ActionEventPair
{
    [JsonProperty("actionName")]
    public string actionName;

    [JsonIgnore]
    public Action action;
}