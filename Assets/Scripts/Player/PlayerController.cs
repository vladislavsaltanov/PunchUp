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

    [SerializeField] private GameObject shopMenu;
    public bool isShopOpen;
    private bool movementPressed;
    #endregion

    [Header("GODMODE")]
    [SerializeField] public bool GodModeBool;
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
        inputManager.RegisterPlayer(GetComponent<PlayerInput>());

        if (inputManager != null)
        {
            inputManager.GetAction("Attack").performed += OnAttack;
            inputManager.GetAction("SpecialAbility").performed += OnAbility;
            inputManager.GetAction("Interact").performed += OnInteract;
            inputManager.GetAction("Move").performed += OnMovePerformed;
        }

        if (groundedHandler == null) groundedHandler = isGroundedHandler.Instance;
        if (groundedHandler != null) groundedHandler.hasGrounded += hasGroundedEventHandler;
        if (combatHandler == null)   combatHandler = GetComponent<CombatHandler>();

        GodModeBool = PlayerPrefs.GetInt("GODMODE") == 1;
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
            inputManager.GetAction("Attack").performed -= OnAttack;
            inputManager.GetAction("SpecialAbility").performed -= OnAbility;
            inputManager.GetAction("Move").performed -= OnMovePerformed;
        }

        if (groundedHandler != null)
        {
            groundedHandler.hasGrounded -= hasGroundedEventHandler;
        }

        inputManager.GetAction("Interact").performed -= OnInteract;
    }

    void Update()
    {
        currentTime += Time.deltaTime;

        if (HasVelocityOverride) return;

        float inputX = inputManager.GetAction("Move").ReadValue<Vector2>().x;

        animator.SetBool("Running", groundedHandler.IsGrounded && Mathf.Abs(inputX) > 0.1f);

        if (IsActionLocked) return;

        if (isShopOpen)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CloseShop();
                return;
            }

            if (IsMovementInputPressed())
            {
                CloseShop();
                return;
            }
            return;
        }

        UpdateDirection();
    }

    void UpdateDirection()
    {
        if (inputManager == null) return;

        float inputX = inputManager.GetAction("Move").ReadValue<Vector2>().x;

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
    //SHSHOP

    public void OpenShop(ShopKeeper shop, ShopSlotRuntime[] slots)
    {
        isShopOpen = true;
        PlayerAudio.Instance.isMarketBool = isShopOpen;
        AudioManager.Instance.PlayMarketEnter(transform.position);

        shopMenu.SetActive(true);
        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        var shopUI = shopMenu.GetComponent<ShopUI>();
        shopUI.Setup(shop, slots, this);
    }

    public void CloseShop()
    {
        if (!isShopOpen) return;

        isShopOpen = false;
        PlayerAudio.Instance.isMarketBool = isShopOpen;
        AudioManager.Instance.PlayUIHover();

        shopMenu.SetActive(false);
        Time.timeScale = 1f;

        if (!UIManager.Instance.isPaused)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void RefreshShopUI()
    {
        var shopUI = shopMenu.GetComponent<ShopUI>();
        shopUI.Refresh();
    }

    private bool IsMovementInputPressed()
    {
        if (inputManager == null) return false;

        Vector2 moveInput = inputManager.GetAction("Move").ReadValue<Vector2>();
        return moveInput != Vector2.zero;
    }

    void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        if (isShopOpen)
        {
            CloseShop();
        }
    }

}
