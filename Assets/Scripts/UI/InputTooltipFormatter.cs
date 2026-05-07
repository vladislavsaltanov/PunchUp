using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

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

    private static string GetBindingName(string actionName, string scheme)
    {
        InputAction action = InputManager.Instance.GetAction(actionName);

        if (action == null)
            return "?";

        var bindings = action.bindings;

        bool isGamepad = scheme.Contains("Gamepad");

        List<string> results = new();
        string fallback = null;

        for (int i = 0; i < bindings.Count; i++)
        {
            var binding = bindings[i];

            if (binding.isPartOfComposite)
                continue;

            // Composite (WASD etc.)
            if (binding.isComposite)
            {
                bool hasValidPart = false;

                for (int j = i + 1;
                     j < bindings.Count && bindings[j].isPartOfComposite;
                     j++)
                {
                    var part = bindings[j];

                    bool isKeyboardPart =
                        part.effectivePath.StartsWith("<Keyboard>") ||
                        part.effectivePath.StartsWith("<Mouse>");

                    bool isGamepadPart =
                        part.effectivePath.StartsWith("<Gamepad>");

                    if ((isGamepad && isGamepadPart) ||
                        (!isGamepad && isKeyboardPart))
                    {
                        hasValidPart = true;
                        break;
                    }
                }

                if (hasValidPart)
                {
                    string composite = GetCompositeDisplayString(action, i);
                    results.Add(composite);
                }

                continue;
            }

            string display = GetReadableBindingName(action, i);

            if (string.IsNullOrEmpty(display))
                continue;

            fallback ??= display;

            bool isBindingGamepad =
                binding.effectivePath.StartsWith("<Gamepad>");

            bool isBindingKeyboard =
                binding.effectivePath.StartsWith("<Keyboard>") ||
                binding.effectivePath.StartsWith("<Mouse>");

            bool matchesDevice =
                (isGamepad && isBindingGamepad) ||
                (!isGamepad && isBindingKeyboard);

            if (matchesDevice)
            {
                results.Add(display);
            }
        }

        if (results.Count > 0)
            return string.Join(" / ", results);

        return fallback ?? "?";
    }

    private static string GetCompositeDisplayString(InputAction action, int compositeIndex)
    {
        var bindings = action.bindings;

        List<string> parts = new();

        for (int i = compositeIndex + 1;
             i < bindings.Count && bindings[i].isPartOfComposite;
             i++)
        {
            string display = GetReadableBindingName(action, i);

            if (!string.IsNullOrEmpty(display) &&
                !parts.Contains(display))
            {
                parts.Add(display);
            }
        }

        // WASD (English + Russian fallback)
        if (parts.Count == 4 &&
            parts.Contains("W") &&
            parts.Contains("A") &&
            parts.Contains("S") &&
            parts.Contains("D"))
        {
            return "WASD";
        }

        if (parts.Count == 4 &&
            parts.Contains("Ц") &&
            parts.Contains("Ы") &&
            parts.Contains("Ф") &&
            parts.Contains("В"))
        {
            return "WASD";
        }

        // Arrows
        if (parts.Exists(p => p.Contains("Arrow")))
        {
            return "Arrow Keys";
        }

        return string.Join(" / ", parts);
    }

    private static string GetReadableBindingName(InputAction action, int bindingIndex)
    {
        string path = action.bindings[bindingIndex].effectivePath;

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
            "<Keyboard>/shift" => "Shift",

            "<Keyboard>/leftCtrl" => "Ctrl",
            "<Keyboard>/rightCtrl" => "Ctrl",

            "<Keyboard>/leftAlt" => "Alt",
            "<Keyboard>/rightAlt" => "Alt",

            "<Keyboard>/upArrow" => "↑",
            "<Keyboard>/downArrow" => "↓",
            "<Keyboard>/leftArrow" => "←",
            "<Keyboard>/rightArrow" => "→",

            // Gamepad
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

            _ => InputControlPath.ToHumanReadableString(
                path,
                InputControlPath.HumanReadableStringOptions.OmitDevice |
                InputControlPath.HumanReadableStringOptions.UseShortNames
            )
        };
    }
}
