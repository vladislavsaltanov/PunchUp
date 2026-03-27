using System.Threading;
using UnityEngine;

public class PlayerLedgeClimb : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float wallBoxWidth = 0.1f;
    [SerializeField] float wallBoxHeightScale = 0.9f; // fraction of collider height covered
    [SerializeField] float headroomBoxWidth = 0.1f;
    [SerializeField] float headroomBoxHeight = 0.1f;
    [SerializeField] Collider2D _collider;

    [SerializeField] PlayerController controller;
    [SerializeField] InputManager inputManager;

    [Space(10)]
    [Header("Climb Settings")]
    [SerializeField] float climbSpeed = 3f;
    [SerializeField] float nudgeDuration = 0.15f;
    [SerializeField] float nudgeSpeedX = 4f;
    [SerializeField] float nudgeSpeedY = 2f;
    [SerializeField] float footBoxWidth = 0.05f;
    [SerializeField] float climbTimeout = 2f;
    [SerializeField] float cancelInputDuration = 0.3f;
    [SerializeField] float climbCooldown = 0.3f;

    #region etc
    bool canHang, isInHangingAction;
    Bounds _bounds;
    bool headroomHit, wallHit;

    CancellationTokenSource climbCts;
    float oppositeInputTimer;
    float climbCooldownTimer;
    float lastPositionY;
    int stalledFrames;
    const int stallFrameThreshold = 4;
    #endregion

    bool earlyJumpRequested;

    private void Start()
    {
        inputManager.jumpAction.action.performed += OnJump;
    }

    void OnDestroy()
    {
        inputManager.jumpAction.action.performed -= OnJump;
    }

    // early jump out of climb if progress >= 0.8
    private void OnJump(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (isInHangingAction)
            earlyJumpRequested = true;
    }

    private void Update()
    {
        CanHangDetection();

        if (climbCooldownTimer > 0f)
            climbCooldownTimer -= Time.deltaTime;

        if (!isInHangingAction && canHang && climbCooldownTimer <= 0f && controller.rb.linearVelocityY != 0f)
        {
            float input = inputManager.moveAction.action.ReadValue<Vector2>().x;
            bool pressingTowardWall = input != 0 && Mathf.Sign(input) == Mathf.Sign(controller.direction);
            if (!pressingTowardWall)
                return;

            climbCts = new CancellationTokenSource();
            ClimbUp(climbCts.Token);
            return;
        }

        if (isInHangingAction)
        {
            // cancel if player touches floor
            if (isGroundedHandler.Instance.IsGrounded)
            {
                CancelClimb();
                return;
            }

            // cancel only after holding opposite direction long enough
            float input = inputManager.moveAction.action.ReadValue<Vector2>().x;
            if (input != 0 && Mathf.Sign(input) != Mathf.Sign(controller.direction))
            {
                oppositeInputTimer += Time.deltaTime;
                if (oppositeInputTimer >= cancelInputDuration)
                {
                    oppositeInputTimer = 0f;
                    CancelClimb();
                }
            }
            else
            {
                oppositeInputTimer = 0f;
            }
        }
    }

    void CanHangDetection()
    {
        _bounds = _collider.bounds;
        float sideX = controller.direction == -1 ? _bounds.min.x : _bounds.max.x;

        // tall box covering almost full height of the player to the side, lifted off the ground
        float wallBoxHeight = _bounds.size.y * wallBoxHeightScale;
        float groundOffset = _bounds.size.y * 0.15f;
        Vector2 wallBoxCenter = new Vector2(
            sideX + controller.direction * wallBoxWidth * 0.5f,
            _bounds.min.y + groundOffset + wallBoxHeight * 0.5f);

        wallHit = Physics2D.OverlapBox(wallBoxCenter, new Vector2(wallBoxWidth, wallBoxHeight), 0f, groundLayer);

        // small box above the player's head on the side facing the wall
        Vector2 headBoxCenter = new Vector2(
            sideX + controller.direction * headroomBoxWidth * 0.5f,
            _bounds.max.y + headroomBoxHeight * 0.5f);

        headroomHit = Physics2D.OverlapBox(headBoxCenter, new Vector2(headroomBoxWidth, headroomBoxHeight), 0f, groundLayer);

        DebugDrawBox(wallBoxCenter, new Vector2(wallBoxWidth, wallBoxHeight), wallHit ? Color.green : Color.blue);
        DebugDrawBox(headBoxCenter, new Vector2(headroomBoxWidth, headroomBoxHeight), headroomHit ? Color.red : Color.cyan);

        canHang = wallHit && !headroomHit;
    }

    void DebugDrawBox(Vector2 center, Vector2 size, Color color)
    {
        Vector2 half = size * 0.5f;
        Debug.DrawLine(center + new Vector2(-half.x, -half.y), center + new Vector2(half.x, -half.y), color);
        Debug.DrawLine(center + new Vector2(half.x, -half.y), center + new Vector2(half.x, half.y), color);
        Debug.DrawLine(center + new Vector2(half.x, half.y), center + new Vector2(-half.x, half.y), color);
        Debug.DrawLine(center + new Vector2(-half.x, half.y), center + new Vector2(-half.x, -half.y), color);
    }

    async void ClimbUp(CancellationToken ct)
    {
        isInHangingAction = true;
        controller.ApplyVelocityOverride(Vector2.up * climbSpeed, float.MaxValue);

        bool earlyJumped = false;
        float climbProgress = 0f;
        float timeoutTimer = 0f;
        float gracePeriod = 0.1f;
        lastPositionY = controller.rb.position.y;
        stalledFrames = 0;

        // phase 1: climb until foot box hits the wall
        while (true)
        {
            if (ct.IsCancellationRequested)
                break;

            timeoutTimer += Time.deltaTime;
            gracePeriod -= Time.deltaTime;

            if (gracePeriod <= 0f)
            {
                if (Mathf.Approximately(controller.rb.position.y, lastPositionY))
                    stalledFrames++;
                else
                    stalledFrames = 0;

                lastPositionY = controller.rb.position.y;
            }

            if (timeoutTimer >= climbTimeout || IsClimbBlocked() || stalledFrames >= stallFrameThreshold)
            {
                CancelClimb();
                return;
            }

            if (earlyJumpRequested)
            {
                earlyJumpRequested = false;
                controller.rb.AddForce(new Vector2(controller.direction * climbSpeed, climbSpeed), ForceMode2D.Impulse);
                earlyJumped = true;
                break;
            }

            if (FootBoxHit())
                break;

            await Awaitable.NextFrameAsync(ct);
        }

        // phase 2: keep climbing until foot box clears the ledge
        while (true)
        {
            if (ct.IsCancellationRequested)
                break;

            if (IsClimbBlocked())
            {
                CancelClimb();
                return;
            }

            if (Mathf.Approximately(controller.rb.position.y, lastPositionY))
                stalledFrames++;
            else
                stalledFrames = 0;

            lastPositionY = controller.rb.position.y;

            if (stalledFrames >= stallFrameThreshold)
            {
                CancelClimb();
                return;
            }

            climbProgress += Time.deltaTime * climbSpeed;

            // jump out of climb at any point in phase 2
            if (earlyJumpRequested)
            {
                earlyJumpRequested = false;
                controller.rb.AddForce(new Vector2(controller.direction * climbSpeed, climbSpeed), ForceMode2D.Impulse);
                earlyJumped = true;
                break;
            }

            earlyJumpRequested = false;

            if (!FootBoxHit())
                break;

            await Awaitable.NextFrameAsync(ct);
        }

        if (!earlyJumped && !ct.IsCancellationRequested)
        {
            // push player diagonally onto the ledge so they land properly
            controller.ApplyVelocityOverride(
                new Vector2(controller.direction * nudgeSpeedX, nudgeSpeedY),
                nudgeDuration);
            climbCooldownTimer = nudgeDuration + climbCooldown;
        }
        else
        {
            controller.ClearVelocityOverride();
            climbCooldownTimer = climbCooldown;
        }
        isInHangingAction = false;
        climbCts = null;
    }

    // thin vertical box cast at foot level to the side of the player
    bool FootBoxHit()
    {
        _bounds = _collider.bounds;
        float sideX = controller.direction == -1 ? _bounds.min.x : _bounds.max.x;
        Vector2 boxCenter = new Vector2(sideX + controller.direction * footBoxWidth * 0.5f, _bounds.min.y + _bounds.size.y * 0.25f);
        Vector2 boxSize = new Vector2(footBoxWidth, _bounds.size.y * 0.5f);

        return Physics2D.OverlapBox(boxCenter, boxSize, 0f, groundLayer);
    }

    bool IsClimbBlocked() => headroomHit;

    void CancelClimb()
    {
        if (climbCts != null)
        {
            climbCts.Cancel();
            climbCts.Dispose();
            climbCts = null;
        }

        controller.ClearVelocityOverride();
        climbCooldownTimer = climbCooldown;
        isInHangingAction = false;
    }
}