using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    #region Singleton
    static public InputManager Instance { get; private set; }
    private void Awake() =>
        Instance = this;
    #endregion

    #region Action Scenario
    enum ActionScenario { Game, UI }
    [SerializeField] ActionScenario actionScenario;
    PlayerInput _playerInput;

    public void SwitchScenario(int i)
    {
        actionScenario = (ActionScenario)i;

        if (_playerInput == null) _playerInput = FindFirstObjectByType<PlayerInput>();

        if (_playerInput != null)
        {
            string mapName = (actionScenario == ActionScenario.UI) ? "UI" : "Game";
            _playerInput.SwitchCurrentActionMap(mapName);
        }
    }
    #endregion

    public InputActionReference moveAction, attackAction, dashAction, specialAbilityAction, interactAction, jumpAction, pauseAction, inventoryAction;
}
