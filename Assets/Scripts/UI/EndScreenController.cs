using TMPro;
using UnityEngine;

public class EndScreenController : MonoBehaviour
{
    #region Singleton
    public static EndScreenController Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] TextMeshProUGUI runNumber;
    [SerializeField] TextMeshProUGUI timeText;

    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject textPrefab;

    public void Show(bool died)
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        var runManager = RunManager.Instance;
        var lastResult = runManager.LastResult;

        titleText.text = died ? "вы умерли :(" : "ран завершен!";
        runNumber.text = $"забег {lastResult.run_number}";

        if (died)
            AddText($"причина смерти: {lastResult.cause_of_death}");

        AddText($"пройдено этажей: {lastResult.floors_cleared}");
        AddText($"пробирок собрано: {lastResult.flasks_picked}");
        AddText($"предметов собрано: {lastResult.items_picked}");
        AddText($"врагов уничтожено: {lastResult.kills}");

        timeText.text = $"время: {FormatTime(lastResult.total_playtime)}";
    }

    private void AddText(string content)
    {
        var textObject = Instantiate(textPrefab, contentParent);
        var textComponent = textObject.GetComponent<TextMeshProUGUI>();
        if (textComponent != null)
            textComponent.text = content;
    }

    private string FormatTime(float totalSeconds)
    {
        int minutes = Mathf.FloorToInt(totalSeconds / 60);
        int seconds = Mathf.FloorToInt(totalSeconds % 60);
        return $"{minutes:D2}:{seconds:D2}";
    }
}
