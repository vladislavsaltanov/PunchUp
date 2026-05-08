using UnityEngine;

public class TutorialHandler : MonoBehaviour
{
    [TextArea(3, 10)]
    [SerializeField] private string[] tooltips;

    [SerializeField] private float startDelay = 2f;
    [SerializeField] private float tooltipDuration = 5f;

    private async void Start()
    {
        if (PlayerPrefs.GetInt("tutorial_passed", 0) == 1)
            return;

        await RunTutorialAsync();
    }

    [ContextMenu("RunTutorial")]
    private async Awaitable RunTutorialAsync()
    {
        if (InfoPopUpScreenController.Instance == null)
        {
            Debug.LogWarning(
                "InfoPopUpScreenController.Instance not found.");
            return;
        }

        await Awaitable.WaitForSecondsAsync(startDelay);

        await InfoPopUpScreenController.Instance.ShowMultiple(
            tooltips,
            tooltipDuration
        );

        PlayerPrefs.SetInt("tutorial_passed", 1);
        PlayerPrefs.Save();
    }
}
