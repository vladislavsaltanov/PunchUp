using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDash : MonoBehaviour
{
    [Header("Dash Settings")]
    [SerializeField] float dashSpeed = 18f;
    [SerializeField] float dashDuration = 0.18f;

    [Header("References")]
    [SerializeField] PlayerController controller;
    [SerializeField] InputManager inputManager;

    float cooldownTimer;
    bool isDashing;

    void Start()
    {
        inputManager.dashAction.action.performed += OnDash;
    }

    void OnDestroy()
    {
        inputManager.dashAction.action.performed -= OnDash;
    }

    void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
    }

    void OnDash(InputAction.CallbackContext ctx)
    {
        if (isDashing) return;
        if (cooldownTimer > 0f) return;
        if (controller.IsActionLocked) return;

        _ = DashRoutine();
    }

    async Awaitable DashRoutine()
    {
        isDashing = true;
        cooldownTimer = controller.Stats[StatType.DashCooldown];

        controller.ApplyVelocityOverride(
            new Vector2(controller.direction * dashSpeed * controller.Stats[StatType.Speed], 0f),
            dashDuration
        );

        await Awaitable.WaitForSecondsAsync(dashDuration);

        isDashing = false;
    }
}