using System;
using System.Threading;
using TMPro;
using UnityEngine;

public class InfoPopUpScreenController : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private Animator _animator;

    private const float HideAnimDuration = 0.4f;
    private const float HideImmediateDuration = 0.2f;
    private const string ShowTrigger = "ShowPopUp";
    private const string HideTrigger = "HidePopUp";
    private const string ShowStateName = "Show";

    public static InfoPopUpScreenController Instance { get; private set; }

    private CancellationTokenSource _cts;

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

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }

    public void Show(string message, float duration, float delay = 0f, Action onComplete = null)
    {
        var token = ResetCancellation();
        ShowAsync(message, duration, delay, onComplete, token).IgnoreExceptions();
    }

    public void ShowMultiple(string[] messages, float duration, float delay = 0f)
    {
        if (messages == null || messages.Length == 0) return;

        var token = ResetCancellation();
        ShowMultipleAsync(messages, duration, delay, token).IgnoreExceptions();
    }

    public void HidePopUpImmediately(Action onComplete = null)
    {
        var token = ResetCancellation();
        HideImmediateAsync(onComplete, token).IgnoreExceptions();
    }

    private async Awaitable ShowAsync(
        string message, float duration, float delay,
        Action onComplete, CancellationToken token)
    {
        if (delay > 0f)
            await Awaitable.WaitForSecondsAsync(delay, token);

        // ≈сли уже показываетс€ - скрыть и подождать анимацию
        if (_animator.GetCurrentAnimatorStateInfo(0).IsName(ShowStateName))
        {
            _animator.SetTrigger(HideTrigger);
            await Awaitable.WaitForSecondsAsync(HideAnimDuration, token);
        }

        _text.text = message;
        _animator.SetTrigger(ShowTrigger);

        await Awaitable.WaitForSecondsAsync(duration, token);

        _animator.SetTrigger(HideTrigger);
        onComplete?.Invoke();
    }

    private async Awaitable ShowMultipleAsync(
        string[] messages, float duration,
        float delay, CancellationToken token)
    {
        if (delay > 0f)
            await Awaitable.WaitForSecondsAsync(delay, token);

        foreach (var message in messages)
        {
            await ShowAsync(message, duration, 0f, null, token);
            await Awaitable.WaitForSecondsAsync(HideAnimDuration, token);
        }
    }

    private async Awaitable HideImmediateAsync(Action onComplete, CancellationToken token)
    {
        _animator.SetTrigger(HideTrigger);
        await Awaitable.WaitForSecondsAsync(HideImmediateDuration, token);
        onComplete?.Invoke();
    }

    private CancellationToken ResetCancellation()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        return _cts.Token;
    }
}
internal static class AwaitableExtensions
{
    public static async void IgnoreExceptions(this Awaitable awaitable)
    {
        try { await awaitable; }
        catch (OperationCanceledException) {}
        catch (Exception e) { Debug.LogException(e); }
    }
}