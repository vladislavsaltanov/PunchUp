using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class RunManager : MonoBehaviour
{
    #region Singleton
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

    [Header("Settings")]
    [SerializeField] LevelNameHandler levelNameHandler;
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private string _mainMenuSceneName = "MainMenu";

    public PlayerController Player { get; private set; }
    public int CurrentFloor { get; private set; } = 1;
    public bool IsRunActive { get; private set; }
    public StatisticData LastResult { get; private set; }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == _mainMenuSceneName)
        {
            CleanupRun();
            return;
        }

        ResetEventSystem();
        BindCanvasesToCamera();

        if (IsRunActive && Player == null)
        {
            SpawnPlayer();
        }
    }

    private void BindCanvasesToCamera()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (var canvas in canvases)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                canvas.worldCamera = mainCam;
            }
        }
    }

    private void ResetEventSystem()
    {
        var es = EventSystem.current;
        if (es != null)
        {
            Destroy(es.gameObject);
        }

        GameObject newEsGo = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        DontDestroyOnLoad(newEsGo);
    }

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

        SceneTransitionManager.SwitchScene(GetRandomLevel());
    }

    private void SpawnPlayer()
    {
        if (Player != null) return;
        
        GameObject playerGo = Instantiate(_playerPrefab, Vector3.zero, Quaternion.identity);
        Player = playerGo.GetComponent<PlayerController>();
        DontDestroyOnLoad(playerGo);
    }

    public async System.Threading.Tasks.ValueTask EndRun(string cause = "unknown")
    {
        if (!IsRunActive) return;
        IsRunActive = false;

        var s = StatisticsHandler.Instance;
        s.StopTimer();

        var data = s.statisticData;
        if (cause != "end")
            data.cause_of_death = cause;
        data.floor_of_death = (uint)CurrentFloor;

        LastResult = new StatisticData(data); 
        s.FinalSave();
        await EndScreenController.Instance.Show(cause != "end");

        _ = PlaytestReporter.SendSessionAsync(new SessionData
        {
            statisticData = data,
            globalStatisticsData = s.globalStatisticsData
        });

        s.statisticData = new StatisticData();
    }

    public async void RestartRun()
    {
        if (IsRunActive)
            await EndRun("restart");

        CleanupRun();
        StartRun();
        SceneTransitionManager.SwitchScene(GetRandomLevel());
    }

    public void OnFloorCleared(bool bossBattle = false)
    {
        StatisticsHandler.Instance.statisticData.floors_cleared++;
        CurrentFloor++;
        Time.timeScale = 1f;
        UIManager.Instance.isPaused = false;

        if (CurrentFloor % 5 == 0 || bossBattle)
            SceneTransitionManager.SwitchScene("BossBattle");
        else
            SceneTransitionManager.SwitchScene(GetRandomLevel());
    }

    public string GetRandomLevel()
    {
        if (levelNameHandler == null || levelNameHandler.levelNames.Length == 0)
        {
            Debug.LogError("LevelNameHandler is not set up correctly.");
            return "MainMenu";
        }

        return levelNameHandler.levelNames[Random.Range(0, levelNameHandler.levelNames.Length)];
    }

    public void OnBossDeath()
    {
        System.Threading.Tasks.ValueTask valueTask = EndRun("end");

        InputManager.Instance.SwitchScenario(InputManager.ActionScenario.UI);
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }


    public void CleanupRun()
    {
        DifficultyManager.Instance.Reset();
        if (Player != null)
        {
            Destroy(Player.gameObject);
            Player = null;
        }
        IsRunActive = false;
    }
}
