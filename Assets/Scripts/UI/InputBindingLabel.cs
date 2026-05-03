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
        if (_labelText == null) _labelText = GetComponent<TextMeshProUGUI>();

        _action = InputManager.Instance.GetAction(_actionName);

        UpdateLabel();
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

        // Returns "E", "Button South", "Space", etc.
        _labelText.text = "Нажмите " + _action.GetBindingDisplayString();
    }
}
