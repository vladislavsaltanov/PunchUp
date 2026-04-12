using UnityEngine;

public class TutorialHandler : MonoBehaviour
{
    [SerializeField] string[] tooltips;
    [SerializeField] float delay = 2f, tooltipDuration = 5f;

    private void Start()
    {
        if (PlayerPrefs.GetInt("tutorial_passed", 0) == 0)
            _ = delayedStart();
    }

    async Awaitable delayedStart()
    {
        await Awaitable.WaitForSecondsAsync(delay);
        InfoPopUpScreenController.Instance.ShowMultiple(tooltips, tooltipDuration);
        await Awaitable.WaitForSecondsAsync(delay + tooltips.Length * tooltipDuration);
        PlayerPrefs.SetInt("tutorial_passed", 1);
        PlayerPrefs.Save();
    }
}
