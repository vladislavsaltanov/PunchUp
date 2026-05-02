using UnityEngine;
using UnityEngine.EventSystems;

public class UIMenuFocus : MonoBehaviour
{
    [SerializeField] private GameObject _firstSelected;

    private async void OnEnable()
    {
        SimpleSelectionFrame.ResetParent();
        if (EventSystem.current == null) return;

        EventSystem.current.SetSelectedGameObject(null);

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
