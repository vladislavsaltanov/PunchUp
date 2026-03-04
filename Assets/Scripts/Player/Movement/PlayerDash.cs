using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDash : MonoBehaviour
{
    [Header("Dash Settings")]
    [SerializeField] float dashSpeed = 18f;
    [SerializeField] float dashDuration = 0.18f;
    [SerializeField] float dashCooldown = 0.6f;

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
        cooldownTimer = dashCooldown;

        float dashDir = controller.direction;

        controller.ApplyVelocityOverride(
            new Vector2(dashDir * dashSpeed, 0f),
            dashDuration
        );

        await Awaitable.WaitForSecondsAsync(dashDuration);

        isDashing = false;
    }
}