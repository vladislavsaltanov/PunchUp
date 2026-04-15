using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

[RequireComponent(typeof(TMP_Text))]
public class AdvancedTextAnimator : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public enum TextAnimationType
    {
        Instant,
        Typewriter,
        GlitchReveal,
        StaticNoise,
        Decrypt,
        ShakeReveal,
    }

    public enum CoverageMode
    {
        LeftToRight,
        RightToLeft,
        CenterOut,
        Random,
    }

    [Serializable]
    public struct TriggerConfig
    {
        public bool enabled;
        public TextAnimationType type;
        [Range(0.05f, 3f)]
        public float duration;
    }

    [Header("Content")]
    [TextArea(2, 6)]
    [SerializeField] private string initialText = "MARATHON";
    [SerializeField] private TextAnimationType animationType = TextAnimationType.GlitchReveal;
    [SerializeField] private float duration = 1f;

    [Header("Coverage")]
    [Range(0f, 1f)]
    [SerializeField] private float animationCoverage = 1f;
    [SerializeField] private CoverageMode coverageMode = CoverageMode.LeftToRight;

    [Header("Glitch Speed")]
    [SerializeField] private float glitchInterval = 0.08f;

    [Header("Typewriter")]
    [SerializeField] private string cursorChar = "█";
    [SerializeField] private float cursorBlinkRate = 0.5f;
    [SerializeField] private bool keepCursorAfterReveal = true;

    [Header("Shake")]
    [SerializeField] private float shakeIntensity = 3f;
    [SerializeField] private float shakeSpeed = 40f;
    [SerializeField] private bool shakeWhileAnimating = false;
    [SerializeField] private bool enableIdleShake = false;
    [SerializeField] private float idleShakeIntensity = 1f;

    [Header("Pointer Triggers")]
    [SerializeField] private bool usePointerTriggers = false;
    [SerializeField] private TriggerConfig onHoverEnter = new() { enabled = true, type = TextAnimationType.GlitchReveal, duration = 0.3f };
    [SerializeField] private TriggerConfig onHoverExit = new() { enabled = false, type = TextAnimationType.StaticNoise, duration = 0.2f };
    [SerializeField] private TriggerConfig onClickTrigger = new() { enabled = true, type = TextAnimationType.Decrypt, duration = 0.4f };

    [SerializeField] TMP_Text _tmp;
    private CancellationTokenSource _mainCts;
    private CancellationTokenSource _triggerCts;
    private string _currentText = "";
    private int _mainGen = 0;
    private int _triggerGen = 0;

    private static readonly string GlitchChars = "!@#$%^&*<>[]{}|█▓▒░?/\\~";

    public bool IsAnimating { get; private set; }
    public bool IsTriggerAnimating { get; private set; }

    [ContextMenu("Awake")]
    private void Awake()
    {
        if (_tmp == null)
            _tmp = GetComponent<TMP_Text>();

        initialText = _tmp.text ?? "placeholder";
        _tmp.raycastTarget = usePointerTriggers;

        foreach (var sub in GetComponentsInChildren<TMP_SubMeshUI>())
            sub.raycastTarget = usePointerTriggers;
    }
    private void Start() => Play(initialText, animationType, duration);
    private void OnDestroy() { CancelMain(); CancelTrigger(); }

    private void Update()
    {
        if (enableIdleShake && !IsAnimating && !IsTriggerAnimating)
            ApplyVertexShake(idleShakeIntensity, shakeSpeed);
    }

    public void SetText(string text, TextAnimationType type, float animDuration = -1f)
        => Play(text, type, animDuration < 0 ? duration : animDuration);

    public void SetText(string text)
        => Play(text, animationType, duration);

    public void SetTextInstant(string text)
    {
        CancelMain();
        _currentText = text;
        _tmp.text = text;
    }

    public void TriggerEffect(TextAnimationType type, float triggerDuration = 0.3f)
    {
        if (IsAnimating) return;
        CancelTrigger();
        _triggerCts = new CancellationTokenSource();
        int gen = ++_triggerGen;
        _ = PlayOverlay(type, triggerDuration, _currentText, _triggerCts.Token, gen);
    }

    public void TriggerEffect(TriggerConfig cfg)
    {
        if (cfg.enabled) TriggerEffect(cfg.type, cfg.duration);
    }

    public void OnPointerEnter(PointerEventData _) { if (usePointerTriggers) TriggerEffect(onHoverEnter); }
    public void OnPointerExit(PointerEventData _) { if (usePointerTriggers) TriggerEffect(onHoverExit); }
    public void OnPointerClick(PointerEventData _) { if (usePointerTriggers) TriggerEffect(onClickTrigger); }

    private void Play(string text, TextAnimationType type, float dur)
    {
        CancelMain();
        CancelTrigger();
        _currentText = text;
        _mainCts = new CancellationTokenSource();
        int gen = ++_mainGen;
        var token = _mainCts.Token;

        _ = type switch
        {
            TextAnimationType.Instant => PlayInstant(text, gen),
            TextAnimationType.Typewriter => PlayTypewriter(text, dur, token, gen),
            TextAnimationType.GlitchReveal => PlayGlitchReveal(text, dur, token, gen),
            TextAnimationType.StaticNoise => PlayStaticNoise(text, dur, token, gen),
            TextAnimationType.Decrypt => PlayDecrypt(text, dur, token, gen),
            TextAnimationType.ShakeReveal => PlayShakeReveal(text, dur, token, gen),
            _ => PlayInstant(text, gen),
        };
    }

    private void CancelMain()
    {
        _mainCts?.Cancel(); _mainCts?.Dispose(); _mainCts = null;
        IsAnimating = false;
    }

    private void CancelTrigger()
    {
        _triggerCts?.Cancel(); _triggerCts?.Dispose(); _triggerCts = null;
        IsTriggerAnimating = false;
    }

    private async Awaitable PlayOverlay(TextAnimationType type, float dur, string text, CancellationToken token, int gen)
    {
        IsTriggerAnimating = true;
        try
        {
            _ = type switch
            {
                TextAnimationType.GlitchReveal => PlayGlitchReveal(text, dur, token, -1),
                TextAnimationType.StaticNoise => PlayStaticNoise(text, dur, token, -1),
                TextAnimationType.Decrypt => PlayDecrypt(text, dur, token, -1),
                TextAnimationType.ShakeReveal => PlayShakeReveal(text, dur, token, -1),
                TextAnimationType.Typewriter => PlayTypewriter(text, dur, token, -1),
                _ => PlayInstant(text, -1),
            };

            await Awaitable.WaitForSecondsAsync(dur, token);
            _tmp.text = text;
        }
        finally
        {
            if (_triggerGen == gen) IsTriggerAnimating = false;
        }
    }

    // Вызывается в конце каждого анимационного кадра.
    // Применяет shake поверх уже обновлённого меша, если включён shakeWhileAnimating.
    private void PostFrame(bool[] covered)
    {
        if (shakeWhileAnimating)
            ApplyVertexShake(shakeIntensity, shakeSpeed, covered);
    }


    private bool[] BuildCoverage(int len)
    {
        var covered = new bool[len];
        if (len == 0) return covered;

        int count = Mathf.RoundToInt(len * Mathf.Clamp01(animationCoverage));
        if (count == 0) return covered;

        switch (coverageMode)
        {
            case CoverageMode.LeftToRight:
                for (int i = 0; i < count; i++) covered[i] = true;
                break;

            case CoverageMode.RightToLeft:
                for (int i = len - count; i < len; i++) covered[i] = true;
                break;

            case CoverageMode.CenterOut:
                int mid = len / 2;
                int start = Mathf.Max(0, mid - count / 2);
                int end = Mathf.Min(len, start + count);
                for (int i = start; i < end; i++) covered[i] = true;
                break;

            case CoverageMode.Random:
                int[] idx = new int[len];
                for (int i = 0; i < len; i++) idx[i] = i;
                for (int i = len - 1; i > 0; i--)
                {
                    int j = Random.Range(0, i + 1);
                    (idx[i], idx[j]) = (idx[j], idx[i]);
                }
                for (int i = 0; i < count; i++) covered[idx[i]] = true;
                break;
        }

        return covered;
    }

    private async Awaitable PlayInstant(string text, int gen)
    {
        _tmp.text = text;
        await Awaitable.NextFrameAsync();
        if (_mainGen == gen) IsAnimating = false;
    }

    private async Awaitable PlayTypewriter(string text, float dur, CancellationToken token, int gen)
    {
        if (gen >= 0) IsAnimating = true;
        try
        {
            var covered = BuildCoverage(text.Length);
            float charDelay = text.Length > 0 ? dur / text.Length : 0.04f;
            char[] display = text.ToCharArray();
            _tmp.SetCharArray(display, 0, display.Length);

            for (int i = 0; i < text.Length; i++)
            {
                token.ThrowIfCancellationRequested();
                if (!covered[i])
                {
                    await Awaitable.WaitForSecondsAsync(charDelay, token);
                    continue;
                }

                display[i] = text[i];
                int next = NextCoveredIndex(covered, i + 1);
                if (next >= 0) display[next] = cursorChar[0];
                _tmp.SetCharArray(display, 0, display.Length);
                PostFrame(covered);
                await Awaitable.WaitForSecondsAsync(charDelay, token);
            }

            if (keepCursorAfterReveal)
                await BlinkCursor(text, token);
            else
                _tmp.text = text;
        }
        finally
        {
            if (gen >= 0 && _mainGen == gen) IsAnimating = false;
        }
    }

    private async Awaitable BlinkCursor(string text, CancellationToken token)
    {
        bool visible = true;
        while (!token.IsCancellationRequested)
        {
            _tmp.text = text + (visible ? cursorChar : " ");
            visible = !visible;
            await Awaitable.WaitForSecondsAsync(cursorBlinkRate, token);
        }
    }

    private async Awaitable PlayGlitchReveal(string text, float dur, CancellationToken token, int gen)
    {
        if (gen >= 0) IsAnimating = true;
        try
        {
            int len = text.Length;
            var covered = BuildCoverage(len);
            float step = len > 0 ? dur / len : 0.05f;
            char[] display = new char[len];

            for (int i = 0; i < len; i++)
                display[i] = covered[i] ? (text[i] == ' ' ? ' ' : RandomGlitch()) : text[i];
            _tmp.SetCharArray(display, 0, len);

            float lastGlitch = 0f;
            for (int i = 0; i < len; i++)
            {
                if (!covered[i]) continue;

                float end = Time.realtimeSinceStartup + step;
                while (Time.realtimeSinceStartup < end)
                {
                    token.ThrowIfCancellationRequested();
                    if (glitchInterval <= 0f || Time.realtimeSinceStartup - lastGlitch >= glitchInterval)
                    {
                        for (int j = 0; j < i; j++) if (covered[j]) display[j] = text[j];
                        for (int j = i; j < len; j++) if (covered[j]) display[j] = text[j] == ' ' ? ' ' : RandomGlitch();
                        _tmp.SetCharArray(display, 0, len);
                        PostFrame(covered);
                        lastGlitch = Time.realtimeSinceStartup;
                    }
                    await Awaitable.NextFrameAsync(token);
                }

                display[i] = text[i];
                _tmp.SetCharArray(display, 0, len);
            }

            _tmp.text = text;
        }
        finally
        {
            if (gen >= 0 && _mainGen == gen) IsAnimating = false;
        }
    }

    private async Awaitable PlayStaticNoise(string text, float dur, CancellationToken token, int gen)
    {
        if (gen >= 0) IsAnimating = true;
        try
        {
            int len = text.Length;
            var covered = BuildCoverage(len);
            char[] display = text.ToCharArray();

            float end = Time.realtimeSinceStartup + dur;
            float lastGlitch = 0f;
            while (Time.realtimeSinceStartup < end)
            {
                token.ThrowIfCancellationRequested();
                if (glitchInterval <= 0f || Time.realtimeSinceStartup - lastGlitch >= glitchInterval)
                {
                    for (int i = 0; i < len; i++)
                        display[i] = (covered[i] && text[i] != ' ') ? RandomGlitch() : text[i];
                    _tmp.SetCharArray(display, 0, len);
                    PostFrame(covered);
                    lastGlitch = Time.realtimeSinceStartup;
                }
                await Awaitable.NextFrameAsync(token);
            }

            _tmp.text = text;
        }
        finally
        {
            if (gen >= 0 && _mainGen == gen) IsAnimating = false;
        }
    }

    private async Awaitable PlayDecrypt(string text, float dur, CancellationToken token, int gen)
    {
        if (gen >= 0) IsAnimating = true;
        try
        {
            int len = text.Length;
            var covered = BuildCoverage(len);
            float step = len > 0 ? dur / len : 0.06f;

            char[] display = new char[len];
            for (int i = 0; i < len; i++)
                display[i] = covered[i] ? '_' : text[i];
            _tmp.SetCharArray(display, 0, len);

            for (int i = 0; i < len; i++)
            {
                if (!covered[i] || text[i] == ' ')
                {
                    display[i] = text[i];
                    continue;
                }

                float end = Time.realtimeSinceStartup + step;
                float lastGlitch = 0f;
                while (Time.realtimeSinceStartup < end)
                {
                    token.ThrowIfCancellationRequested();
                    if (glitchInterval <= 0f || Time.realtimeSinceStartup - lastGlitch >= glitchInterval)
                    {
                        display[i] = RandomGlitch();
                        _tmp.SetCharArray(display, 0, len);
                        PostFrame(covered);
                        lastGlitch = Time.realtimeSinceStartup;
                    }
                    await Awaitable.NextFrameAsync(token);
                }

                display[i] = text[i];
                _tmp.SetCharArray(display, 0, len);
            }

            _tmp.text = text;
        }
        finally
        {
            if (gen >= 0 && _mainGen == gen) IsAnimating = false;
        }
    }

    private async Awaitable PlayShakeReveal(string text, float dur, CancellationToken token, int gen)
    {
        if (gen >= 0) IsAnimating = true;
        try
        {
            _tmp.text = text;
            _tmp.ForceMeshUpdate();
            var covered = BuildCoverage(text.Length);

            float end = Time.realtimeSinceStartup + dur;
            while (Time.realtimeSinceStartup < end)
            {
                token.ThrowIfCancellationRequested();
                float t = (end - Time.realtimeSinceStartup) / dur;
                ApplyVertexShake(shakeIntensity * t, shakeSpeed, covered);
                await Awaitable.NextFrameAsync(token);
            }

            _tmp.ForceMeshUpdate();
        }
        finally
        {
            if (gen >= 0 && _mainGen == gen) IsAnimating = false;
        }
    }

    private void ApplyVertexShake(float intensity, float speed, bool[] covered = null)
    {
        _tmp.ForceMeshUpdate();
        var info = _tmp.textInfo;

        for (int i = 0; i < info.characterCount; i++)
        {
            if (!info.characterInfo[i].isVisible) continue;
            if (covered != null && i < covered.Length && !covered[i]) continue;

            int matIdx = info.characterInfo[i].materialReferenceIndex;
            int vertIdx = info.characterInfo[i].vertexIndex;
            var verts = info.meshInfo[matIdx].vertices;

            float oy = Mathf.Sin(Time.time * speed + i * 1.3f) * intensity;
            float ox = Mathf.Sin(Time.time * speed * 0.7f + i) * intensity * 0.4f;
            var offset = new Vector3(ox, oy, 0);

            for (int v = 0; v < 4; v++)
                verts[vertIdx + v] += offset;
        }

        for (int i = 0; i < info.meshInfo.Length; i++)
        {
            var mesh = info.meshInfo[i].mesh;
            mesh.vertices = info.meshInfo[i].vertices;
            _tmp.UpdateGeometry(mesh, i);
        }
    }

    private char RandomGlitch()
        => GlitchChars[Random.Range(0, GlitchChars.Length)];

    private static int NextCoveredIndex(bool[] covered, int from)
    {
        for (int i = from; i < covered.Length; i++)
            if (covered[i]) return i;
        return -1;
    }
}