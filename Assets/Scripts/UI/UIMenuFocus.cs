using UnityEngine;
using UnityEngine.EventSystems;

public class UIMenuFocus : MonoBehaviour
{
    [SerializeField] private GameObject _firstSelected;

    private async void OnEnable()
    {
        SimpleSelectionFrame.ResetParent();
        
        // Ensure EventSystem is ready and clear old selection
        if (EventSystem.current == null) return;
        EventSystem.current.SetSelectedGameObject(null);

        // Wait for the end of frame to ensure UI layout is updated and EventSystem is stable
        await Awaitable.NextFrameAsync();
        await Awaitable.NextFrameAsync();

        if (_firstSelected != null && _firstSelected.activeInHierarchy)
        {
            EventSystem.current.SetSelectedGameObject(_firstSelected);
        }
    }

    private void Update()
    {
        if (EventSystem.current == null) return;

        GameObject current = EventSystem.current.currentSelectedGameObject;

        if (current == null && SimpleSelectionFrame.ShowFrame)
        {
            if (_firstSelected != null && _firstSelected.activeInHierarchy)
                EventSystem.current.SetSelectedGameObject(_firstSelected);
        }
    }
}
