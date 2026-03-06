using System.Threading;
using UnityEngine;

public class PlayerLedgeClimb : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float headroomRayLength = 0.2f;
    [SerializeField] float headroomRayOffset = 1f;
    [SerializeField] float wallDetectRayLength = 0.2f;
    [SerializeField] float wallDetectRayOffset = 0.8f;
    [SerializeField] Collider2D _collider;

    [SerializeField] PlayerController controller;
    [SerializeField] InputManager inputManager;

    [Space(10)]
    [Header("Climb Settings")]
    [SerializeField] float climbSpeed = 3f;
    [SerializeField] float nudgeDuration = 0.08f;
    [SerializeField] float nudgeSpeed = 3f;
    [SerializeField] float footRayLength = 0.3f;

    #region etc
    bool canHang, isInHangingAction;
    Bounds _bounds;
    RaycastHit2D[] wallRayHits = new RaycastHit2D[1];
    RaycastHit2D[] headroomRayHits = new RaycastHit2D[1];
    RaycastHit2D[] footRayHits = new RaycastHit2D[1];
    int headroomDetectCount = 0;
    int wallDetectCount = 0;
    int footRayDetectCount = 0;

    CancellationTokenSource climbCts;
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

        if (!isInHangingAction && canHang && controller.rb.linearVelocityY != 0f)
        {
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

            // cancel if opposite direction input
            float input = inputManager.moveAction.action.ReadValue<Vector2>().x;
            if (input != 0 && Mathf.Sign(input) != Mathf.Sign(controller.direction))
            {
                CancelClimb();
            }
        }
    }

    void CanHangDetection()
    {
        _bounds = _collider.bounds;

        Debug.DrawRay(new Vector2(controller.direction == -1 ? _bounds.min.x : _bounds.max.x, _bounds.center.y + headroomRayOffset), Vector2.right * controller.direction * headroomRayLength, Color.red);
        Debug.DrawRay(new Vector2(controller.direction == -1 ? _bounds.min.x : _bounds.max.x, _bounds.center.y + wallDetectRayOffset), Vector2.right * controller.direction * wallDetectRayLength, Color.blue);

        headroomDetectCount = Physics2D.RaycastNonAlloc(
            new Vector2(controller.direction == -1 ? _bounds.min.x : _bounds.max.x,
                        _bounds.center.y + headroomRayOffset),
            Vector2.right * controller.direction, headroomRayHits, headroomRayLength, groundLayer);

        wallDetectCount = Physics2D.RaycastNonAlloc(
            new Vector2(controller.direction == -1 ? _bounds.min.x : _bounds.max.x,
                        _bounds.center.y + wallDetectRayOffset),
            Vector2.right * controller.direction, wallRayHits, wallDetectRayLength, groundLayer);

        canHang = headroomDetectCount == 0 && wallDetectCount == 1;
    }

    async void ClimbUp(CancellationToken ct)
    {
        isInHangingAction = true;
        controller.ApplyVelocityOverride(Vector2.up * climbSpeed, float.MaxValue);

        bool earlyJumped = false;
        float climbProgress = 0f;

        // phase 1: climb until foot ray hits the wall
        while (true)
        {
            if (ct.IsCancellationRequested)
                break;

            _bounds = _collider.bounds;
            footRayDetectCount = Physics2D.RaycastNonAlloc(
                new Vector2(controller.direction == -1 ? _bounds.min.x : _bounds.max.x, _bounds.min.y),
                Vector2.right * controller.direction, footRayHits, footRayLength, groundLayer);

            Debug.DrawRay(
                new Vector2(controller.direction == -1 ? _bounds.min.x : _bounds.max.x, _bounds.min.y),
                Vector2.right * controller.direction * footRayLength, Color.yellow);

            if (footRayDetectCount > 0)
                break;

            await Awaitable.NextFrameAsync(ct);
        }

        // phase 2: keep climbing until foot ray clears the ledge
        while (true)
        {
            if (ct.IsCancellationRequested)
                break;

            climbProgress += Time.deltaTime * climbSpeed;
            float normalizedProgress = Mathf.Clamp01(climbProgress);

            // early jump if progress >= 0.8 and player jumps
            if (normalizedProgress >= 0.8f && earlyJumpRequested)
            {
                earlyJumpRequested = false;
                controller.rb.AddForce(new Vector2(controller.direction * climbSpeed, climbSpeed), ForceMode2D.Impulse);
                earlyJumped = true;
                break;
            }

            earlyJumpRequested = false;

            _bounds = _collider.bounds;
            footRayDetectCount = Physics2D.RaycastNonAlloc(
                new Vector2(controller.direction == -1 ? _bounds.min.x : _bounds.max.x, _bounds.min.y),
                Vector2.right * controller.direction, footRayHits, footRayLength, groundLayer);

            Debug.DrawRay(
                new Vector2(controller.direction == -1 ? _bounds.min.x : _bounds.max.x, _bounds.min.y),
                Vector2.right * controller.direction * footRayLength, Color.green);

            if (footRayDetectCount == 0)
                break;

            await Awaitable.NextFrameAsync(ct);
        }

        if (!earlyJumped && !ct.IsCancellationRequested)
        {
            // nudge in direction so player doesn't immediately fall off the ledge
            controller.ApplyVelocityOverride(new Vector2(controller.direction * nudgeSpeed, 0f), nudgeDuration);
        }
        else
        {
            controller.ClearVelocityOverride();
        }

        isInHangingAction = false;
        climbCts = null;
    }

    void CancelClimb()
    {
        if (climbCts != null)
        {
            climbCts.Cancel();
            climbCts.Dispose();
            climbCts = null;
        }

        controller.ClearVelocityOverride();
        isInHangingAction = false;
    }
}