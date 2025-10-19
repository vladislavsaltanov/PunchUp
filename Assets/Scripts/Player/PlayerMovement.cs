using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
        MovementCharacteristics chars;

    [Space(20)]

    [SerializeField]
        Rigidbody2D rb;
    [SerializeField]
        isGroundedHandler isGroundedHandler;
    [SerializeField]
        PlayerController controller;
    [SerializeField]
        InputManager inputManager;

    bool isForcedFalling;
    private void Start()
    {
        inputManager.jumpAction.action.performed += Jump;
    }

    private void OnDestroy()
    {
        inputManager.jumpAction.action.performed -= Jump;
    }

    private void Update()
    {
        isForcedFalling = inputManager.moveAction.action.ReadValue<Vector2>().y < -0.5f && (controller.currentTime - controller.lastGroundedTime > chars.forcedFallCooldown);

        // resetting speed if we stop moving
        if (Mathf.Abs(inputManager.moveAction.action.ReadValue<Vector2>().x) < 0.1f && isGroundedHandler.isGrounded)
            rb.linearVelocityX = Mathf.Lerp(rb.linearVelocityX, 0f, chars.resetSpeedTime * Time.deltaTime);

        // clamping velocity so we wont fly too fast
        rb.linearVelocityX = Mathf.Clamp(rb.linearVelocityX, -chars.maxHorizontalSpeed, chars.maxHorizontalSpeed);
        rb.linearVelocityY = Mathf.Clamp(rb.linearVelocityY, isForcedFalling ? -chars.forcedFallForce : chars.maxVerticalSpeed.x, chars.maxVerticalSpeed.y);

        // horizontal movement
        if (Mathf.Abs(inputManager.moveAction.action.ReadValue<Vector2>().x) > 0)
            rb.linearVelocityX += inputManager.moveAction.action.ReadValue<Vector2>().x * chars.speed;
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

    void Jump(InputAction.CallbackContext callback)
    {
        // TODO: add coyote time
        if (callback.performed && isGroundedHandler.isGrounded)
        {
            rb.linearVelocityY = 2;
            rb.AddForce(Vector2.up * chars.jumpForce, ForceMode2D.Impulse);
        }
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
}