using System.Threading;
using UnityEngine;

public class RuntimeHint : MonoBehaviour
{
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] float transitionDuration = 0.25f;

    CancellationTokenSource cts;
    private void Awake()
    {
        cts = new CancellationTokenSource();
        canvasGroup.alpha = 0f; // Start hidden
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // stop running routine
        cts.Cancel();
        cts = new CancellationTokenSource();
        _ = TransitionRoutine(true);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        // stop running routine
        cts.Cancel();
        cts = new CancellationTokenSource();
        _ = TransitionRoutine(false);
    }

    async Awaitable TransitionRoutine(bool fadeIn)
    {
        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;
        float targetAlpha = fadeIn ? 1f : 0f;
        while (elapsed < transitionDuration)
        {
            if (cts.IsCancellationRequested) return;
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / transitionDuration);
            await Awaitable.NextFrameAsync(); // Wait for the next frame
        }
        canvasGroup.alpha = targetAlpha; // Ensure it ends at the exact target alpha
    }
}
