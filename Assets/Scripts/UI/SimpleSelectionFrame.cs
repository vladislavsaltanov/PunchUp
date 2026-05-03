using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class SimpleSelectionFrame : MonoBehaviour
{
    private static SimpleSelectionFrame _manager;
    public Color frameColor = Color.whiteSmoke;
    public float thickness = 2f;

    private GameObject _visualFrame;
    private RectTransform _baseRect;
    private RectTransform[] _lines = new RectTransform[4];
    private Image[] _lineImages = new Image[4];

    public static bool ShowFrame = false;

    public static void ResetParent() { }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        if (_manager == null)
        {
            var go = new GameObject("SelectionFrame_Manager");
            _manager = go.AddComponent<SimpleSelectionFrame>();
            DontDestroyOnLoad(go);
        }
    }

    private void Awake()
    {
        // Subscribing to input only once in Awake
        InputSystem.onActionChange += (obj, change) => {
            if (change == InputActionChange.ActionPerformed)
            {
                var action = obj as InputAction;
                if (action?.activeControl != null)
                {
                    var device = action.activeControl.device;
                    ShowFrame = (device is Gamepad || device is Keyboard);
                }
            }
        };
    }

    private void LateUpdate()
    {
        if (EventSystem.current == null || !ShowFrame)
        {
            SetVisible(false);
            return;
        }

        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null || !selected.activeInHierarchy)
        {
            SetVisible(false);
            return;
        }

        RectTransform targetRect = GetTargetRect(selected);
        if (targetRect == null) return;

        if (_visualFrame == null)
        {
            CreateVisualFrame();
        }

        if (_visualFrame.transform.parent != targetRect.transform)
        {
            _visualFrame.transform.SetParent(targetRect.transform, false);
        }

        SetVisible(true);
        UpdateLines(targetRect);
    }

    private void CreateVisualFrame()
    {
        _visualFrame = new GameObject("SelectionFrame_Visual", typeof(RectTransform));
        _baseRect = _visualFrame.GetComponent<RectTransform>();
        _baseRect.anchorMin = new Vector2(0.5f, 0.5f);
        _baseRect.anchorMax = new Vector2(0.5f, 0.5f);
        _baseRect.pivot = new Vector2(0.5f, 0.5f);

        for (int i = 0; i < 4; i++)
        {
            GameObject line = new GameObject("Line", typeof(RectTransform));
            line.transform.SetParent(_visualFrame.transform);
            _lines[i] = line.GetComponent<RectTransform>();
            _lines[i].anchorMin = new Vector2(0.5f, 0.5f);
            _lines[i].anchorMax = new Vector2(0.5f, 0.5f);
            _lineImages[i] = line.AddComponent<Image>();
            _lineImages[i].color = frameColor;
            _lineImages[i].raycastTarget = false;
        }
    }

    private RectTransform GetTargetRect(GameObject go)
    {
        if (go.TryGetComponent<RectTransform>(out var rt)) return rt;
        var btn = go.GetComponentInParent<Button>();
        return btn != null ? btn.GetComponent<RectTransform>() : null;
    }

    private void UpdateLines(RectTransform target)
    {
        float w = target.rect.width;
        float h = target.rect.height;
        float t = thickness;

        _baseRect.anchoredPosition = Vector2.zero;
        _baseRect.localScale = Vector3.one;

        _lines[0].sizeDelta = new Vector2(w, t); _lines[0].anchoredPosition = new Vector2(0, h / 2f);
        _lines[1].sizeDelta = new Vector2(w, t); _lines[1].anchoredPosition = new Vector2(0, -h / 2f);
        _lines[2].sizeDelta = new Vector2(t, h); _lines[2].anchoredPosition = new Vector2(-w / 2f, 0);
        _lines[3].sizeDelta = new Vector2(t, h); _lines[3].anchoredPosition = new Vector2(w / 2f, 0);
    }

    private void SetVisible(bool visible)
    {
        if (_visualFrame == null) return;
        if (_lineImages[0].enabled == visible) return;
        foreach (var img in _lineImages) img.enabled = visible;
    }
}
