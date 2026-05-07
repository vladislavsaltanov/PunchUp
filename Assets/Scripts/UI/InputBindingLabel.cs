using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class InputBindingLabel : MonoBehaviour
{
    [SerializeField] private string _actionName;
    [SerializeField] private TextMeshProUGUI _labelText;
    [SerializeField] private string _schemeName = "Game";

    private InputAction _action;

    private void Awake()
    {
        if (_labelText == null) _labelText = GetComponent<TextMeshProUGUI>();
    }

    private async void Start()
    {
        await InitializeAsync();
    }

    private async Awaitable InitializeAsync()
    {
        await Awaitable.NextFrameAsync();

        if (InputManager.Instance == null)
        {
            Debug.LogWarning("InputManager.Instance еще не доступен.");
            return;
        }

        _action = InputManager.Instance.GetAction(_actionName);

        if (_action != null)
        {
            UpdateLabel();
        }
        else
        {
            Debug.LogError($"Экшен '{_actionName}' не найден!");
        }
    }

    private void OnEnable()
    {
        InputSystem.onActionChange += HandleActionChange;
    }

    private void OnDisable()
    {
        InputSystem.onActionChange -= HandleActionChange;
    }

    private void HandleActionChange(object obj, InputActionChange change)
    {
        if (change == InputActionChange.ActionPerformed)
        {
            UpdateLabel();
        }
    }

    private void UpdateLabel()
    {
        if (_action == null || _labelText == null) return;

        string displayString = _action.GetBindingDisplayString(
        bindingMask: InputBinding.MaskByGroup(_schemeName)
        );

        if (string.IsNullOrEmpty(displayString))
        {
            displayString = _action.GetBindingDisplayString();
        }

        if (!string.IsNullOrEmpty(displayString))
        {
            _labelText.text = "Нажмите " + displayString;
        }
        else
        {
            _labelText.text = "Клавиша не назначена";
        }
    }
}
