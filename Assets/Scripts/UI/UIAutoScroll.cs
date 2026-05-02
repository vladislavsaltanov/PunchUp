using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class UIAutoScroll : MonoBehaviour
{
    private ScrollRect _scrollRect;
    private RectTransform _contentPanel;
    private RectTransform _viewportPanel;
    private GameObject _lastSelected;

    private void Awake()
    {
        _scrollRect = GetComponent<ScrollRect>();
        _contentPanel = _scrollRect.content;
        _viewportPanel = _scrollRect.viewport;
    }

    private void LateUpdate()
    {
        if (!SimpleSelectionFrame.ShowFrame) return;

        GameObject selected = EventSystem.current?.currentSelectedGameObject;
        if (selected == null) return;

        if (selected != _lastSelected)
        {
            _lastSelected = selected;
            if (selected.transform.IsChildOf(_contentPanel))
            {
                EnsureVisible(selected.GetComponent<RectTransform>());
            }
        }
    }

    private void EnsureVisible(RectTransform target)
    {
        if (target == null) return;

        float contentHeight = _contentPanel.rect.height;
        float viewportHeight = _viewportPanel.rect.height;
        float scrollRange = contentHeight - viewportHeight;

        if (scrollRange <= 0) return;

        // Calculate absolute distance from the bottom of the content to the target center
        // target.localPosition.y is relative to content pivot, 
        // _contentPanel.rect.min.y is the distance from pivot to the bottom edge.
        float distFromBottom = target.localPosition.y - _contentPanel.rect.min.y;
        
        // Center the element by offsetting by half of the viewport height
        float normalizedPos = (distFromBottom - (viewportHeight / 2f)) / scrollRange;

        _scrollRect.verticalNormalizedPosition = Mathf.Clamp01(normalizedPos);
    }
}
