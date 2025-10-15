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
    public void SwitchScenario(int i) =>
        actionScenario = (ActionScenario) i;
    #endregion

    public InputActionReference moveAction, attackAction, specialAbilityAction, interactAction, jumpAction;
}