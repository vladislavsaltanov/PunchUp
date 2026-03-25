using UnityEngine;

public class StatisticsHandler : MonoBehaviour
{
    #region Singleton -- REWRITE
    public static StatisticsHandler Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(this);
        Instance = this;
    }
    #endregion

    public GlobalStatisticsData globalStatisticsData;
    public StatisticData statisticData;

    // should be called first as we check for player id
    void TutorialPolicyCheck()
    {
        // check if playerid is null, if so init data and set tutorial and privacy policy flag
        if (!PlayerPrefs.HasKey("player_id"))
        {
            PlayerPrefs.SetInt("privacy_policy_shown", 0);
            PlayerPrefs.SetInt("tutorial_passed", 0);
        }
    }

    void InitGlobalData()
    {
        if (!PlayerPrefs.HasKey("player_id"))
            PlayerPrefs.SetString("player_id", System.Guid.NewGuid().ToString());

        globalStatisticsData = new GlobalStatisticsData
        {
            player_id = PlayerPrefs.GetString("player_id"),
            os = SystemInfo.operatingSystem,
            screen_height = (ushort)Screen.height,
            screen_width = (ushort)Screen.width,
            timezone = System.TimeZoneInfo.Local.Id,
            graphics_quality = QualitySettings.names[QualitySettings.GetQualityLevel()],
            // rewrite for fmod later
            volume_master = AudioListener.volume
        };

        PlayerPrefs.Save();
    }

    public void ResetData()
    {
        PlayerPrefs.SetInt("current_floor", 0);
        statisticData = new StatisticData();
        PlayerPrefs.Save();
    }

    private void Start()
    {
        TutorialPolicyCheck();
        InitGlobalData();
    }
} 