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

    public Awaitable Show(
        string message,
        float duration,
        float delay = 0f,
        Action onComplete = null)
    {
        var token = ResetCancellation();

        return ShowAsync(
            message,
            duration,
            delay,
            onComplete,
            token);
    }

    public Awaitable ShowMultiple(
        string[] messages,
        float duration,
        float delay = 0f)
    {
        if (messages == null || messages.Length == 0)
            return Awaitable.NextFrameAsync();

        var token = ResetCancellation();

        return ShowMultipleAsync(
            messages,
            duration,
            delay,
            token);
    }

    public Awaitable HidePopUpImmediately(
        Action onComplete = null)
    {
        var token = ResetCancellation();

        return HideImmediateAsync(
            onComplete,
            token);
    }

    private async Awaitable ShowAsync(
        string message,
        float duration,
        float delay,
        Action onComplete,
        CancellationToken token)
    {
        if (delay > 0f)
        {
            await Awaitable.WaitForSecondsAsync(
                delay,
                token);
        }

        if (_animator
            .GetCurrentAnimatorStateInfo(0)
            .IsName(ShowStateName))
        {
            _animator.ResetTrigger(ShowTrigger);
            _animator.SetTrigger(HideTrigger);

            await Awaitable.WaitForSecondsAsync(
                HideAnimDuration,
                token);
        }

        _animator.ResetTrigger(HideTrigger);

        string formatted =
            InputTooltipFormatter.Format(message);

        _text.text = formatted;

        _animator.SetTrigger(ShowTrigger);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            token.ThrowIfCancellationRequested();

            string updated =
                InputTooltipFormatter.Format(message);

            if (_text.text != updated)
            {
                _text.text = updated;
            }

            elapsed += Time.deltaTime;

            await Awaitable.NextFrameAsync(token);
        }

        _animator.ResetTrigger(ShowTrigger);
        _animator.SetTrigger(HideTrigger);

        await Awaitable.WaitForSecondsAsync(
            HideAnimDuration,
            token);

        onComplete?.Invoke();
    }

    private async Awaitable ShowMultipleAsync(
        string[] messages,
        float duration,
        float delay,
        CancellationToken token)
    {
        if (delay > 0f)
        {
            await Awaitable.WaitForSecondsAsync(
                delay,
                token);
        }

        foreach (var message in messages)
        {
            token.ThrowIfCancellationRequested();

            await ShowAsync(
                message,
                duration,
                0f,
                null,
                token);
        }
    }

    private async Awaitable HideImmediateAsync(
        Action onComplete,
        CancellationToken token)
    {
        _animator.ResetTrigger(ShowTrigger);
        _animator.SetTrigger(HideTrigger);

        await Awaitable.WaitForSecondsAsync(
            HideImmediateDuration,
            token);

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
