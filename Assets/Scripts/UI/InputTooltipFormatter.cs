using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.InputSystem;

public static class InputTooltipFormatter
{
    private static readonly Regex TokenRegex =
        new(@"\{(.*?)\}");

    public static string Format(string text)
    {
        var playerInput = InputManager.Instance.GetComponent<PlayerInput>();

        if (playerInput == null)
            return text;

        string scheme = playerInput.currentControlScheme;

        return TokenRegex.Replace(text, match =>
        {
            string actionName = match.Groups[1].Value;

            return GetBindingName(actionName, scheme);
        });
    }

    private static string GetBindingName(
    string actionName,
    string scheme)
    {
        InputAction action =
            InputManager.Instance.GetAction(actionName);

        if (action == null)
            return "?";

        var bindings = action.bindings;

        for (int i = 0; i < bindings.Count; i++)
        {
            var binding = bindings[i];

            // Composite root
            if (binding.isComposite)
            {
                return GetCompositeDisplayString(
                    action,
                    i,
                    scheme);
            }

            // Regular binding
            if (!binding.isPartOfComposite)
            {
                if (!InputBinding.MaskByGroup(scheme)
                    .Matches(binding))
                {
                    continue;
                }

                return GetReadableBindingName(
                    action,
                    i);
            }
        }

        return "?";
    }

    private static string GetCompositeDisplayString(
        InputAction action,
        int compositeIndex,
        string scheme)
    {
        var bindings = action.bindings;

        List<string> parts = new();

        for (int i = compositeIndex + 1;
             i < bindings.Count &&
             bindings[i].isPartOfComposite;
             i++)
        {
            var binding = bindings[i];

            if (!InputBinding.MaskByGroup(scheme)
                .Matches(binding))
            {
                continue;
            }

            string display =
                GetReadableBindingName(action, i);

            if (string.IsNullOrEmpty(display))
                continue;

            if (!parts.Contains(display))
            {
                parts.Add(display);
            }
        }

        // WASD
        if (parts.Count == 4 &&
            parts.Contains("W") &&
            parts.Contains("A") &&
            parts.Contains("S") &&
            parts.Contains("D"))
        {
            return "WASD";
        }

        // Arrow keys
        if (parts.Count == 4 &&
            parts.Exists(x => x.Contains("Arrow")))
        {
            return "Arrow Keys";
        }

        return string.Join(" / ", parts);
    }

    private static string GetReadableBindingName(
        InputAction action,
        int bindingIndex)
    {
        string path =
            action.bindings[bindingIndex].effectivePath;

        return path switch
        {
            // Mouse
            "<Mouse>/leftButton" => "LMB",
            "<Mouse>/rightButton" => "RMB",
            "<Mouse>/middleButton" => "MMB",

            // Keyboard
            "<Keyboard>/space" => "Space",
            "<Keyboard>/escape" => "Esc",

            "<Keyboard>/leftShift" => "Shift",
            "<Keyboard>/rightShift" => "Shift",

            "<Keyboard>/leftCtrl" => "Ctrl",
            "<Keyboard>/rightCtrl" => "Ctrl",

            "<Keyboard>/leftAlt" => "Alt",
            "<Keyboard>/rightAlt" => "Alt",

            "<Keyboard>/upArrow" => "↑",
            "<Keyboard>/downArrow" => "↓",
            "<Keyboard>/leftArrow" => "←",
            "<Keyboard>/rightArrow" => "→",

            // Xbox gamepad
            "<Gamepad>/buttonSouth" => "A",
            "<Gamepad>/buttonEast" => "B",
            "<Gamepad>/buttonWest" => "X",
            "<Gamepad>/buttonNorth" => "Y",

            "<Gamepad>/leftShoulder" => "LB",
            "<Gamepad>/rightShoulder" => "RB",

            "<Gamepad>/leftTrigger" => "LT",
            "<Gamepad>/rightTrigger" => "RT",

            "<Gamepad>/start" => "Start",
            "<Gamepad>/select" => "Back",

            "<Gamepad>/leftStickPress" => "L3",
            "<Gamepad>/rightStickPress" => "R3",

            "<Gamepad>/leftStick" => "Left Stick",
            "<Gamepad>/rightStick" => "Right Stick",

            "<Gamepad>/dpad" => "D-Pad",

            // Fallback
            _ => action.GetBindingDisplayString(bindingIndex)
        };
    }
}
