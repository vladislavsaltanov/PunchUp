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
    }
    #endregion

    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] float fadeDuration = 3f;

    [Space(10)]

    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] TextMeshProUGUI runNumber;
    [SerializeField] TextMeshProUGUI timeText;

    [Space(10)]

    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject textPrefab;

    public async Awaitable Show(bool died = false)
    {
        canvasGroup.gameObject.SetActive(true);
        canvasGroup.alpha = 0;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

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

        await FadeTo(1);
    }

    public async void Hide()
    {
        await FadeTo(0);
        canvasGroup.gameObject.SetActive(false);
    }

    private async Awaitable FadeTo(float targetAlpha)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        if (targetAlpha > 0)
        {
            canvasGroup.gameObject.SetActive(true);
            canvasGroup.blocksRaycasts = true;
        }

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            await Awaitable.NextFrameAsync();
        }

        canvasGroup.alpha = targetAlpha;

        if (targetAlpha == 0)
        {
            canvasGroup.blocksRaycasts = false;
        }
    }

    #region etc
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
    #endregion
}
