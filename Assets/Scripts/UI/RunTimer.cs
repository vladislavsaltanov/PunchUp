using UnityEngine;
using TMPro;

public class RunTimer : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;
    private async void OnEnable()
    {
        UpdateTimerText();
        await TimerLoop();
    }

    private async Awaitable TimerLoop()
    {
        while (this != null && isActiveAndEnabled)
        {
            await Awaitable.WaitForSecondsAsync(1f);

            if (this == null || !isActiveAndEnabled)
                break;

            if (RunManager.Instance != null && RunManager.Instance.IsRunActive)
            {
                UpdateTimerText();
            }
        }
    }

    private void UpdateTimerText()
    {
        if (StatisticsHandler.Instance == null || timerText == null)
            return;

        float elapsedTime = StatisticsHandler.Instance.GetElapsedRunTime();

        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);

        timerText.text = "<color=grey>" + string.Format("{0}:{1:00}", minutes, seconds) + "</color>";
    }
}
