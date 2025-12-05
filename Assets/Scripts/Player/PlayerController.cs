using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : BaseEntity
{
    #region Singleton
        public static PlayerController instance {  get; private set; }
    protected override void Awake()
    {
        base.Awake();
        instance = this;
    }
    #endregion

    [HideInInspector]
        public float currentTime, lastGroundedTime;

    #region Modules
    [Header("Modules")]
    [SerializeField] CombatHandler combatHandler;
    #endregion
    public bool IsActionLocked => combatHandler != null && combatHandler.IsBusy;

    #region Cached
    InputManager inputManager;
    #endregion

    void Start()
    {
        isGroundedHandler.Instance.hasGrounded += hasGroundedEventHandler;
        inputManager = InputManager.Instance;

        inputManager.attackAction.action.performed += OnAttack;
        inputManager.specialAbilityAction.action.performed += OnAbility;
    }

    void UpdateDirection()
    {
        if (inputManager == null) return;

        float input = inputManager.moveAction.action.ReadValue<Vector2>().x;

        if (Mathf.Abs(input) > 0.1f)
        {
            direction = (sbyte)(input > 0 ? 1 : -1);
            UpdateVisualDirection();
        }
    }
    void OnDisable()
    {
        isGroundedHandler.Instance.hasGrounded -= hasGroundedEventHandler;
        inputManager.attackAction.action.performed -= OnAttack;
        inputManager.specialAbilityAction.action.performed -= OnAbility;
    }
    void OnAttack(InputAction.CallbackContext ctx)
    {
        if (combatHandler == null)
        {
            Debug.LogError("[PlayerController] CombatHandler not assigned!");
            return;
        }

        combatHandler.TryAttack();
    }

    void OnAbility(InputAction.CallbackContext ctx)
    {
        if (combatHandler == null) return;

        combatHandler.TryUseAbility();
    }
    private void hasGroundedEventHandler(bool hasGrounded, float time)
    {
        lastGroundedTime = time;
    }

    void Update()
    {
        currentTime += Time.deltaTime;

        if (!IsActionLocked)
            UpdateDirection();
    }

    protected override void OnDamageReceived(ushort amount, Transform attacker = null)
    {
        Debug.Log($"[Player] Received {amount} damage");
    }

    protected override void OnDeath()
    {
        combatHandler?.CancelAll();
        Debug.Log("[Player] Died");
    }
}