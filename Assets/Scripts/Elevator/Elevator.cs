using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Elevator : MonoBehaviour, IInteractable
{

    [Header("Settings")]
    [SerializeField] int sceneBuildIndex;

    [Header("Visuals")]
    [SerializeField] GameObject promptUI;
    [SerializeField] GameObject decisionUI;
    bool decisionOpen;

    [Header("Audio")]
    [SerializeField] ElevatorAudio elevatorAudio;

    private void Start()
    {
        if (promptUI != null)
            promptUI.SetActive(false);

        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        UIManager.Instance.onPause += Hide;
    }

    void OnDestroy()
    {
        UIManager.Instance.onPause -= Hide;
    }

    void Hide(bool hide)
    {
        if (!decisionOpen) return;
        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        decisionOpen = false;
        decisionUI.SetActive(!hide);
    }

    public void Interact(PlayerController player)
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        UIManager.Instance.isPaused = !UIManager.Instance.isPaused;

        InputManager.Instance.SwitchScenario(UIManager.Instance.isPaused ? InputManager.ActionScenario.UI : InputManager.ActionScenario.Game);

        decisionOpen = true;
        decisionUI.SetActive(true);
    }

    public void Decision(int option)
    {
        InputManager.Instance.SwitchScenario(InputManager.ActionScenario.Game);

        elevatorAudio.ElevatorOpenHandle();
        elevatorAudio.ElevatorStartMovingHandle();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        RunManager.Instance.OnFloorCleared(option == 1);
    }
    public void ShowPrompt(bool show)
    {
        if (promptUI != null)
            promptUI.SetActive(show);
    }
}
