using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class InputBindingLabel : MonoBehaviour
{
    [SerializeField] private string _actionName;
    [SerializeField] private TextMeshProUGUI _labelText;

    private InputAction _action;

    private void Awake()
    {
        if (_labelText == null)
            _labelText = GetComponent<TextMeshProUGUI>();
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
            UpdateLabel();
        else
            Debug.LogError($"Экшен '{_actionName}' не найден!");
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
            UpdateLabel();
    }

    private void UpdateLabel()
    {
        if (_action == null || _labelText == null)
            return;

        string deviceLayout = InputManager.Instance?.GetCurrentDeviceLayout() ?? "Keyboard";

        string result = null;
        var bindings = _action.bindings;

        for (int i = 0; i < bindings.Count; i++)
        {
            var binding = bindings[i];
            if (binding.isComposite || binding.isPartOfComposite)
                continue;

            var path = binding.effectivePath;
            if (string.IsNullOrEmpty(path)) continue;

            int start = path.IndexOf('<');
            int end = path.IndexOf('>');
            if (start < 0 || end < 0) continue;

            string bindingLayout = path.Substring(start + 1, end - start - 1);

            if (!InputSystem.IsFirstLayoutBasedOnSecond(deviceLayout, bindingLayout)
                && deviceLayout != bindingLayout)
                continue;

            result = InputControlPath.ToHumanReadableString(
                path,
                InputControlPath.HumanReadableStringOptions.OmitDevice |
                InputControlPath.HumanReadableStringOptions.UseShortNames
            );
            break;
        }

        _labelText.text = !string.IsNullOrEmpty(result)
            ? "Нажмите " + result
            : "Клавиша не назначена";
    }
}
