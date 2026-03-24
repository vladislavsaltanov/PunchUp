using UnityEngine;

public class DoctorAudio : MonoBehaviour
{
    //public static DoctorAudio Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private isGroundedHandler isGroundedHandler;
    [SerializeField] private EnemyLogic enemyLogic;

    [Header("Movement Threshold")]
    [SerializeField] private float minVelocityToPlay = 1.0f;

    [Header("Footstep Delays")]
    [SerializeField] private float footstepDelayAfterJump = 0.5f;
    [SerializeField] private float footstepDelayAfterLand = 0.5f;
    [SerializeField] private float footstepStartDelay = 0.2f;

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

    private void Awake()
    {
        //if (Instance != null)
        //{
        //    Debug.LogError("Больше одного PlayerAudio o_0");
        //}
        //Instance = this;

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (isGroundedHandler == null)
            isGroundedHandler = GetComponent<isGroundedHandler>();

        if (enemyLogic == null)
            enemyLogic = GetComponent<EnemyLogic>();

        xPosLastFrame = transform.position.x;
    }

    private void Update()
    {
        HandleFootsteps();
    }

    //Доктор, всё будет хорошо?
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

        /*float linearVelocityX = Mathf.Abs(rb.linearVelocityX);
        bool isGrounded = isGroundedHandler.IsGrounded;

        bool shouldPlay = enemyLogic.currentState == EnemyState.Walking && Mathf.Abs(xPosLastFrame - transform.position.x) > 0;

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
        */
            AudioManager.Instance.PlayDoctorFootstep(transform.position);
            //xPosLastFrame = transform.position.x;
            //stepTimer = currentStepInterval;
        //}
    }
}
