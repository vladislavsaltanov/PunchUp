using System.Collections.Generic;
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

    [Header("GODMODE")]
    [SerializeField] public bool GodModeBool;//РћС‡РµРЅСЊ РёРЅРµС‚РµСЂСЃРЅРѕРµ СЂРµС€РµРЅРёРµ СЃРґРµР»Р°РЅРЅРѕРµ Р±РµР· С‚Р·
    public bool IsActionLocked => combatHandler != null && combatHandler.IsBusy;

    #region Cached
    InputManager inputManager;
    #endregion

    #region Interactables
    List<IInteractable> nearbyInteractables = new();
    IInteractable activeInteractable =>
        nearbyInteractables.Count > 0 ? nearbyInteractables[^1] : null;
    #endregion

    void Start()
    {
        Time.timeScale = 1f;
        inputManager = InputManager.Instance;

        if (inputManager != null)
        {
            inputManager.attackAction.action.performed += OnAttack;
            inputManager.specialAbilityAction.action.performed += OnAbility;
            inputManager.interactAction.action.performed += OnInteract;
        }

        if (groundedHandler == null) groundedHandler = isGroundedHandler.Instance;
        if (groundedHandler != null) groundedHandler.hasGrounded += hasGroundedEventHandler;
        if (combatHandler == null)   combatHandler = GetComponent<CombatHandler>();

        GodModeBool = UIManager.Instance.godmode;
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        var interactable = other.GetComponent<IInteractable>();
        if (interactable == null) return;

        nearbyInteractables.Add(interactable);

        // скрыть prompt у предыдущего
        if (nearbyInteractables.Count > 1)
            nearbyInteractables[^2].ShowPrompt(false);

        interactable.ShowPrompt(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var interactable = other.GetComponent<IInteractable>();
        if (interactable == null) return;

        interactable.ShowPrompt(false);
        nearbyInteractables.Remove(interactable);

        // показать prompt у следующего в очереди
        if (nearbyInteractables.Count > 0)
            nearbyInteractables[^1].ShowPrompt(true);
    }

    void OnInteract(InputAction.CallbackContext ctx)
    {
        activeInteractable?.Interact(this);
    }
    void OnDestroy()
    {
        foreach (var i in nearbyInteractables)
            i?.ShowPrompt(false);
        nearbyInteractables.Clear();

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

        inputManager.interactAction.action.performed -= OnInteract;
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

        PlayerAudio.Instance.HandleAttack();
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
        PlayerAudio.Instance.HandleDamage();
    }

    protected override void OnDeath()
    {
        GetComponent<PlayerMovement>().enabled = false;

        rb.simulated = false;
        entityCollider.enabled = false;

        combatHandler?.CancelAll();
        StatisticsHandler.Instance.statisticData.deaths++;

        _ = RunManager.Instance.EndRun(lastDamageCause ?? "unknown");

        // TODO: shader dissolve effect via awaitable

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        base.OnDeath();
    }

    public override void TakeDamage(ushort amount, Transform attacker = null, string cause = null)
    {
        if (GodModeBool) return;
        else base.TakeDamage(amount,attacker,cause);
    }
    #endregion
}
