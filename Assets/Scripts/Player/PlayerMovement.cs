using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    MovementCharacteristics chars;

    [Space(20)]

    Rigidbody2D rb;

    [SerializeField]
        isGroundedHandler isGroundedHandler;
    [SerializeField]
        PlayerController controller;
    [SerializeField]
        InputManager inputManager;

    bool isForcedFalling;
    byte jumpsRemaining;
    float coyoteTimeTimerCurrent, bufferedJumpTimerCurrent, jumpCooldown = -0.1f;

    [HideInInspector]
        float movementDirection;

    

    private void Start()
    {
        rb = PlayerController.instance.rb;

        inputManager.jumpAction.action.performed += JumpAction;
        coyoteTimeTimerCurrent = chars.coyoteTime;
    }

    private void OnDestroy()
    {
        inputManager.jumpAction.action.performed -= JumpAction;
    }

    private void Update()
    {
        if (controller.HasVelocityOverride)
        {
            Vector2 overrideVel = new Vector2(
                controller.overrideVelocityX ?? rb.linearVelocityX,
                controller.overrideVelocityY ?? rb.linearVelocityY
            );

            rb.linearVelocity = overrideVel;
            // need to still handle jump timers while velocity is overridden
            HandleJumpTimers();
            return;
        }

        Vector2 inputVector = inputManager.moveAction.action.ReadValue<Vector2>();

        float inputX = inputVector.x;
        movementDirection = inputX == 0f ? 0 : inputX > 0.15f ? 1 : -1;

        float targetSpeed = movementDirection * controller.Stats["speed"];

        rb.linearVelocityX = Mathf.Lerp(rb.linearVelocityX, targetSpeed, chars.resetSpeedTime * Time.deltaTime);

        isForcedFalling = inputVector.y < -0.5f && (controller.currentTime - controller.lastGroundedTime > chars.forcedFallCooldown);

        // resetting speed if we stop moving
        if (Mathf.Abs(inputVector.x) < 0.1f && isGroundedHandler.IsGrounded)
            rb.linearVelocityX = Mathf.Lerp(rb.linearVelocityX, 0f, chars.resetSpeedTime * Time.deltaTime);

        if (!controller.overrideVelocityY.HasValue)
            rb.linearVelocityY = Mathf.Clamp(rb.linearVelocityY, isForcedFalling ? -chars.forcedFallForce : chars.maxVerticalSpeed.x, chars.maxVerticalSpeed.y);

        HandleJumpTimers();
        if ((coyoteTimeTimerCurrent > 0f && bufferedJumpTimerCurrent > 0f) || (jumpsRemaining > 0 && jumpCooldown > 0 && bufferedJumpTimerCurrent > 0f))
            Jump();
    }
    void HandleJumpTimers()
    {
        if (isGroundedHandler.IsGrounded)
        {
            jumpsRemaining = chars.maxJumps;
            coyoteTimeTimerCurrent = chars.coyoteTime;
        }
        else
            coyoteTimeTimerCurrent -= Time.deltaTime;

        if (bufferedJumpTimerCurrent > 0f)
            bufferedJumpTimerCurrent -= Time.deltaTime;

        jumpCooldown -= Time.deltaTime;
    }
    private void FixedUpdate()
    {
        // holding down 's' or stick down makes player slam down (after cooldown)
        if (isForcedFalling)
            rb.AddForce(Vector2.down * chars.forcedFallForce, ForceMode2D.Force);

        // additional gravity if player is falling down
        if (rb.linearVelocityY < 0f)
            rb.AddForce(Vector2.down * chars.additionalGravity, ForceMode2D.Force);
    }

    void JumpAction(InputAction.CallbackContext callback)
    {
        if (callback.performed && jumpsRemaining > 0)
        {
            bufferedJumpTimerCurrent = chars.bufferedJumpTimer;
            jumpCooldown = chars.jumpCooldown;
            jumpsRemaining--;
        }
    }

    void Jump()
    {
        coyoteTimeTimerCurrent = 0f;
        bufferedJumpTimerCurrent = 0f;

        rb.linearVelocityY = 0;
        rb.AddForce(Vector2.up * chars.jumpForce, ForceMode2D.Impulse);
    }
}
[System.Serializable]
public class MovementCharacteristics
{
    public float speed;
    public float jumpForce;
    public float resetSpeedTime;
    public float maxHorizontalSpeed;
    public Vector2 maxVerticalSpeed;
    public float additionalGravity;
    public float forcedFallCooldown, forcedFallForce;
    public float coyoteTime = 0.25f, bufferedJumpTimer = 0.2f, jumpCooldown = 0.25f;
    public byte maxJumps = 1;
}