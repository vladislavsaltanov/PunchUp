using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    [field: SerializeField] public float DifficultyMultiplier { get; private set; } = 1.0f;

    private float _timer = 0f;
    private bool _wasRunActive = false;

    [SerializeField] float IntervalSeconds = 120f;
    [SerializeField] float MultiplierIncrease = 0.2f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    private void Update()
    {
        if (RunManager.Instance == null) return;

        bool isRunActive = RunManager.Instance.IsRunActive;

        if (isRunActive && !_wasRunActive)
        {
            ResetDifficulty();
        }
        _wasRunActive = isRunActive;

        if (isRunActive)
        {
            _timer += Time.deltaTime;

            if (_timer >= IntervalSeconds)
            {
                _timer -= IntervalSeconds;
                DifficultyMultiplier += MultiplierIncrease;
            }
        }
    }

    private void ResetDifficulty()
    {
        DifficultyMultiplier = 1.0f;
        _timer = 0f;
    }
}
