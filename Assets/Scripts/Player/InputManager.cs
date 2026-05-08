using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    #region Singleton
    static public InputManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }
    #endregion

    #region Action Scenario
    public enum ActionScenario { Game, UI }
    public ActionScenario CurrentScenario { get; private set; } = ActionScenario.Game;

    private PlayerInput _playerInput;

    public void RegisterPlayer(PlayerInput input)
    {
        _playerInput = input;
    }

    public void UnregisterPlayer()
    {
        _playerInput = null;
    }

    public void SwitchScenario(ActionScenario scenario)
    {
        CurrentScenario = scenario;
        
        if (_playerInput != null)
        {
            string mapName = (scenario == ActionScenario.UI) ? "UI" : "Game";
            _playerInput.SwitchCurrentActionMap(mapName);
        }
    }
    public string GetCurrentDeviceLayout()
    {
        if (_playerInput == null || _playerInput.devices.Count == 0)
            return "Keyboard";
        return _playerInput.devices[0].layout;
    }
    public InputAction GetAction(string actionName)
    {
        if (_playerInput == null) return null;
        return _playerInput.actions.FindAction(actionName);
    }
    #endregion
}
