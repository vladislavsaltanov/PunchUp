using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : BaseEntity
{
    #region Singleton
    public static PlayerController instance { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    #endregion

    [HideInInspector]
    public float currentTime, lastGroundedTime;

    #region Modules
    [Header("Modules")]
    [SerializeField] CombatHandler combatHandler;
    [SerializeField] isGroundedHandler groundedHandler; 
    #endregion
    public bool IsActionLocked => combatHandler != null && combatHandler.IsBusy;

    #region Cached
    InputManager inputManager;
    #endregion

    void Start()
    {
        inputManager = InputManager.Instance;

        if (inputManager != null)
        {
            inputManager.attackAction.action.performed += OnAttack;
            inputManager.specialAbilityAction.action.performed += OnAbility;
        }

        if (groundedHandler == null) groundedHandler = isGroundedHandler.Instance;
        if (groundedHandler != null)
        {
            groundedHandler.hasGrounded += hasGroundedEventHandler;
        }

        if (combatHandler == null) combatHandler = GetComponent<CombatHandler>();
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;

        if (inputManager != null)
        {
            inputManager.attackAction.action.performed -= OnAttack;
            inputManager.specialAbilityAction.action.performed -= OnAbility;
        }

        if (groundedHandler != null)
        {
            groundedHandler.hasGrounded -= hasGroundedEventHandler;
        }
    }

    void Update()
    {
        currentTime += Time.deltaTime;

        if (HasVelocityOverride) return;

        if (IsActionLocked) return;

        UpdateDirection();
    }

    void UpdateDirection()
    {
        if (inputManager == null) return;

        float inputX = inputManager.moveAction.action.ReadValue<Vector2>().x;

        if (Mathf.Abs(inputX) > 0.1f)
        {
            direction = (sbyte)(inputX > 0 ? 1 : -1);

            UpdateVisualDirection();
        }
    }

    #region Input Handlers
    void OnAttack(InputAction.CallbackContext ctx)
    {
        if (combatHandler == null) return;

        combatHandler.TryPrimaryAttack();
    }

    void OnAbility(InputAction.CallbackContext ctx)
    {
        if (combatHandler == null) return;

        combatHandler.TrySpecialAbility();
    }
    #endregion

    private void hasGroundedEventHandler(bool hasGrounded, float time)
    {
        lastGroundedTime = time;
    }

    #region BaseEntity Implementation
    protected override void OnDamageReceived(ushort amount, Transform attacker = null)
    {
    }

    protected override void OnDeath()
    {
        combatHandler?.CancelAll();
        Debug.Log("[Player] Died. Game Over logic here.");

        this.enabled = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }
    #endregion
}