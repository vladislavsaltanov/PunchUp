using UnityEngine;

public class RunManager : MonoBehaviour
{
    #region Singleton (i dont like this too trust me)
    public static RunManager Instance { get; private set; }
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

    public int CurrentFloor { get; private set; } = 1;
    public bool IsRunActive { get; private set; }

    public StatisticData LastResult { get; private set; }

    public void StartRun()
    {
        CurrentFloor = 1;
        IsRunActive = true;
        PlayerPrefs.SetInt("totalRuns", PlayerPrefs.GetInt("totalRuns", 0) + 1);

        var s = StatisticsHandler.Instance;
        s.statisticData = new StatisticData
        {
            run_number = (uint)(PlayerPrefs.GetInt("totalRuns", 0))
        };

        s.StartTimer();
    }
    public void OnFloorCleared()
    {
        StatisticsHandler.Instance.statisticData.floors_cleared++;
        // perhaps saving data? 
        
        CurrentFloor++;
    }

    public async Awaitable EndRun(string cause = "unknown")
    {
        if (!IsRunActive) return;
        IsRunActive = false;

        var s = StatisticsHandler.Instance;
        s.StopTimer();

        var data = s.statisticData;
        data.cause_of_death = cause;
        data.floor_of_death = (uint)CurrentFloor;

        LastResult = new StatisticData(data);

        await PlaytestReporter.SendSessionAsync(new SessionData
        {
            statisticData = data,
            globalStatisticsData = s.globalStatisticsData
        });

        s.FinalSave();
        EndScreenController.Instance.Show(cause != "end");
        s.statisticData = new StatisticData();
    }
}