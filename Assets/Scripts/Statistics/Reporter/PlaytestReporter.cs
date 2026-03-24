using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class PlaytestReporter
{
    private const string QUEUE_FILE = "pending_sessions.json";
    private const string PLAYER_ID_KEY = "player_id";

    public static string ApiUrl = "https://api.madebykowie.ru/punchup/session";
    public static string PlaytestToken = "token_MrMCUzFcBkHoCOT4KvDzX6YNQoeQ25wW";

    public static int MaxRetries = 3;
    public static float RetryDelaySeconds = 1.5f;
    public static int TimeoutSeconds = 10;

    private static string QueueFilePath => Path.Combine(Application.persistentDataPath, QUEUE_FILE);

    [Serializable]
    private class FlatSessionDto
    {
        public string player_id;
        public string os;
        public int screen_width;
        public int screen_height;
        public string timezone;

        public int floors_cleared;
        public int flasks_picked;
        public int playtime_sec;
        public string graphics_quality;
        public float volume_master;

        public uint deaths;
        public uint kills;
        public uint items_picked;
    }

    [Serializable]
    private class QueueWrapper
    {
        public List<FlatSessionDto> items = new();
    }

    public static void SendSession(SessionData data) => _ = SendSessionAsync(data);

    public static async Awaitable SendSessionAsync(SessionData data)
    {
        if (!TryBuildPayload(data, out var payload))
            return;

        var pending = LoadQueueSafe();
        pending.Add(payload);

        var unsent = new List<FlatSessionDto>();
        foreach (var item in pending)
        {
            bool ok = await PostWithRetryAsync(item);
            if (!ok) unsent.Add(item);
        }

        SaveQueueSafe(unsent);
    }

    private static bool TryBuildPayload(SessionData data, out FlatSessionDto dto)
    {
        dto = null;

        if (data == null || data.globalStatisticsData == null || data.statisticData == null)
            return false;

        string playerId = PlayerPrefs.GetString(PLAYER_ID_KEY, "").Trim();
        if (string.IsNullOrEmpty(playerId))
        {
            Debug.LogWarning($"[PlaytestReporter] PlayerPrefs '{PLAYER_ID_KEY}' is empty.");
            return false;
        }

        dto = new FlatSessionDto
        {
            player_id = playerId,
            os = SystemInfo.operatingSystem,
            screen_width = Screen.currentResolution.width,
            screen_height = Screen.currentResolution.height,
            timezone = TimeZoneInfo.Local.Id,

            floors_cleared = (int)data.statisticData.floors_cleared,
            flasks_picked = (int)data.statisticData.flasks_picked,
            playtime_sec = (int)data.statisticData.total_playtime,
            graphics_quality = data.globalStatisticsData.graphics_quality,
            volume_master = data.globalStatisticsData.volume_master,

            deaths = data.statisticData.deaths,
            kills = data.statisticData.kills,
            items_picked = data.statisticData.items_picked
        };

        return true;
    }

    private static async Awaitable<bool> PostWithRetryAsync(FlatSessionDto dto)
    {
        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            bool ok = await PostOnceAsync(dto);
            if (ok) return true;

            if (attempt < MaxRetries)
                await Awaitable.WaitForSecondsAsync(RetryDelaySeconds);
        }
        return false;
    }

    private static async Awaitable<bool> PostOnceAsync(FlatSessionDto dto)
    {
        string json = JsonUtility.ToJson(dto);
        byte[] body = Encoding.UTF8.GetBytes(json);

        using var req = new UnityWebRequest(ApiUrl, "POST");
        req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("X-Playtest-Token", PlaytestToken);
        req.timeout = TimeoutSeconds;

        await req.SendWebRequest();

        bool ok = req.result == UnityWebRequest.Result.Success &&
                  req.responseCode >= 200 && req.responseCode < 300;

        if (!ok)
            Debug.LogWarning($"[PlaytestReporter] Failed ({req.responseCode}): {req.error}. Body: {req.downloadHandler?.text}");

        return ok;
    }

    private static List<FlatSessionDto> LoadQueueSafe()
    {
        try
        {
            if (!File.Exists(QueueFilePath)) return new List<FlatSessionDto>();

            string json = File.ReadAllText(QueueFilePath);
            var wrapper = string.IsNullOrWhiteSpace(json) ? null : JsonUtility.FromJson<QueueWrapper>(json);
            var result = wrapper?.items ?? new List<FlatSessionDto>();

            File.Delete(QueueFilePath); // read -> delete old
            return result;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[PlaytestReporter] LoadQueueSafe error: {e.Message}");
            return new List<FlatSessionDto>();
        }
    }

    private static void SaveQueueSafe(List<FlatSessionDto> items)
    {
        try
        {
            if (items == null || items.Count == 0)
            {
                if (File.Exists(QueueFilePath)) File.Delete(QueueFilePath);
                return;
            }

            string json = JsonUtility.ToJson(new QueueWrapper { items = items });
            string tmp = QueueFilePath + ".tmp";

            File.WriteAllText(tmp, json);
            if (File.Exists(QueueFilePath)) File.Delete(QueueFilePath);
            File.Move(tmp, QueueFilePath);
        }
        catch (Exception e)
        {
            Debug.LogError($"[PlaytestReporter] SaveQueueSafe error: {e.Message}");
        }
    }
}

[Serializable]
public class SessionData
{
    public GlobalStatisticsData globalStatisticsData;
    public StatisticData statisticData;
}