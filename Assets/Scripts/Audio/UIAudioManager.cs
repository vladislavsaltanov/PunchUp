using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIAudioManager : MonoBehaviour
{
    public static UIAudioManager Instance { get; private set; }

    private GameObject lastSelected;
    private readonly HashSet<Selectable> initializedSelectables = new();

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

    private void Update()
    {
        HandleKeyboardSelection();
        AttachToAllSelectables();
    }

    private void HandleKeyboardSelection()
    {
        if (EventSystem.current == null)
            return;

        GameObject current = EventSystem.current.currentSelectedGameObject;

        if (current == null)
            return;

        if (current != lastSelected)
        {
            lastSelected = current;

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayUIHover();
        }
    }

    private void AttachToAllSelectables()
    {
        Selectable[] all = Selectable.allSelectablesArray;

        foreach (Selectable selectable in all)
        {
            if (selectable == null)
                continue;

            if (initializedSelectables.Contains(selectable))
                continue;

            var trigger = selectable.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = selectable.gameObject.AddComponent<EventTrigger>();

            var entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };
            entry.callback.AddListener((_) =>
            {
                if (selectable.interactable && AudioManager.Instance != null)
                    AudioManager.Instance.PlayUIHover();
            });

            trigger.triggers.Add(entry);

            if (selectable is Button button)
            {
                button.onClick.AddListener(() =>
                {
                    if (button.interactable && AudioManager.Instance != null)
                        AudioManager.Instance.PlayUIClick();
                });
            }

            initializedSelectables.Add(selectable);
        }
    }
}
