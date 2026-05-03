using Lean.Transition;
using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    public static PlayerAudio Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private isGroundedHandler isGroundedHandler;
    [SerializeField] private InputManager inputManager;

    [Header("Movement Threshold")]
    [SerializeField] private float minVelocityToPlay = 1.0f;

    [Header("Footstep Delays")]
    [SerializeField] private float footstepDelayAfterJump = 0.5f;
    [SerializeField] private float footstepDelayAfterLand = 0.5f;
    [SerializeField] private float footstepStartDelay = 0.2f;

    [Header("Jump Threshold")]
    [SerializeField] private float minTimeToPlay = 0.5f;

    [Header("Speed Mapping")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 6f;

    [Header("Step Interval")]
    [SerializeField] private float maxStepInterval = 0.5f;
    [SerializeField] private float minStepInterval = 0.25f;

    [Header("Debug")]
    [SerializeField] private bool enableLogs = false;

    private float stepTimer;
    private float jumpTime;
    private float footstepBlockedUntil;
    private float xPosLastFrame;
    private bool wasMovingLastFrame;
    private bool isPausedBool = false;
    public bool isMarketBool = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Больше одного PlayerAudio o_0");
        }
        Instance = this;

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (isGroundedHandler == null)
            isGroundedHandler = GetComponent<isGroundedHandler>();
        
        if (inputManager == null)
            inputManager = GetComponent<InputManager>();

        isGroundedHandler.hasGrounded += HandleLand;
        xPosLastFrame = transform.position.x;
    }

    private void Update()
    {
        HandleFootsteps();
    }

    //О песочек горячо горяченьки - ай ой ай бж бжж ай шшж
    private void HandleFootsteps()
    {
        if (rb == null)
        {
            if (enableLogs) Debug.LogWarning("CharacterFootsteps: Rigidbody2D is null");
            return;
        }

        if (isGroundedHandler == null)
        {
            if (enableLogs) Debug.LogWarning("CharacterFootsteps: IsGroundedHandler is null");
            return;
        }

        if (AudioManager.Instance == null)
        {
            if (enableLogs) Debug.LogWarning("CharacterFootsteps: AudioManager.Instance is null");
            return;
        }

        float linearVelocityX = Mathf.Abs(rb.linearVelocityX);
        bool isGrounded = isGroundedHandler.IsGrounded;

        bool shouldPlay = isGrounded && (linearVelocityX > minVelocityToPlay) && Mathf.Abs(xPosLastFrame - transform.position.x) > 0;

        if (Time.time < footstepBlockedUntil)
        {
            stepTimer = 0f;
            return;
        }

        if (!shouldPlay)
        {
            stepTimer = 0f;
            wasMovingLastFrame = false;
            return;
        }

        if (!wasMovingLastFrame)
        {
            stepTimer = footstepStartDelay;
            wasMovingLastFrame = true;
        }

        float normalizedSpeed = Mathf.InverseLerp(walkSpeed, runSpeed, linearVelocityX);
        float currentStepInterval = Mathf.Lerp(maxStepInterval, minStepInterval, normalizedSpeed);

        stepTimer -= Time.deltaTime;

        if (stepTimer <= 0f)
        {
            if (enableLogs)
            {
                Debug.Log($"CharacterFootsteps: step | grounded={isGrounded} | velocityX={linearVelocityX} | interval={currentStepInterval}");
            }

            AudioManager.Instance.PlayFootstep(transform.position);
            xPosLastFrame = transform.position.x;
            stepTimer = currentStepInterval;
        }
    }

    //Прыг
    public void HandleJump()
    {
        footstepBlockedUntil = Time.time + footstepDelayAfterJump;
        jumpTime = PlayerController.instance.currentTime;
        isPausedBool = UIManager.Instance.isPaused;
        if (!isPausedBool && !isMarketBool)
        {
            AudioManager.Instance.PlayJumpLand(transform.position, AudioManager.JumpLandAction.Jump);
        }
        else return;
    }

    //Скок
    private void HandleLand(bool hasGrounded, float time)
    {
        if (hasGrounded)
        {
            footstepBlockedUntil = Time.time + footstepDelayAfterJump;
            if ((time - jumpTime) > minTimeToPlay)
            {
                AudioManager.Instance.PlayJumpLand(transform.position, AudioManager.JumpLandAction.Land);
            } else
            {
                AudioManager.Instance.PlayJumpLand(transform.position, AudioManager.JumpLandAction.softLand);
            }
        }
    }

    //Деш
    public void HandleDash()
    {
        isPausedBool = UIManager.Instance.isPaused;
        if (!isPausedBool && !isMarketBool)
        {
            AudioManager.Instance.PlayDashSound(transform.position);
        }
        else return;
    }
    
    //Ай
    public void HandleDamage()
    {
        AudioManager.Instance.PlayerTakeDamage(transform.position);
    }

    //Бамс
    public void HandleAttack()
    {
        isPausedBool = UIManager.Instance.isPaused;
        if (!isPausedBool && !isMarketBool)
        {
            AudioManager.Instance.PlayerAttack(transform.position);
        }
        else return;
    }
}
