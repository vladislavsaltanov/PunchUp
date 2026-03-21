using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    public static PlayerAudio Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private isGroundedHandler isGroundedHandler;
    [SerializeField] private InputManager inputManager;

    [Header("Movement Threshold")]
    [SerializeField] private float minVelocityToPlay = 0.1f;

    [Header("Speed Mapping")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 6f;

    [Header("Step Interval")]
    [SerializeField] private float maxStepInterval = 0.5f;
    [SerializeField] private float minStepInterval = 0.25f;

    [Header("Debug")]
    [SerializeField] private bool enableLogs = false;

    private float stepTimer;

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

        bool shouldPlay = isGrounded && linearVelocityX > minVelocityToPlay;

        if (!shouldPlay)
        {
            stepTimer = 0f;
            return;
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
            stepTimer = currentStepInterval;
        }
    }

    //Прыг
    public void HandleJump()
    {
        AudioManager.Instance.PlayJumpLand(transform.position, AudioManager.JumpLandAction.Jump);
    }

    //Скок
    private void HandleLand(bool hasGrounded, float num)
    {
        if (hasGrounded)
        {
            AudioManager.Instance.PlayJumpLand(transform.position,AudioManager.JumpLandAction.Land);
        }
    }
}