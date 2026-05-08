using UnityEngine;

public class DoctorAudio : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
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

    [Header("Idle Voice")]
    [SerializeField] private float minIdleSoundDelay = 5f;
    [SerializeField] private float maxIdleSoundDelay = 10f;

    [Header("Debug")]
    [SerializeField] private bool enableLogs = false;

    private float stepTimer;
    private float idleSoundTimer;
    private float footstepBlockedUntil;
    private float xPosLastFrame;
    private bool wasMovingLastFrame;

    private bool enemyLogicUse = true;

    private void Awake()
    {

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (enemyLogic == null)
        {
            try
            {
                enemyLogic = GetComponent<EnemyLogic>();
            }
            catch
            {
                enemyLogicUse=false;
            }
        }

        xPosLastFrame = transform.position.x;
        ResetIdleSoundTimer();
    }

    private void Update()
    {
        HandleFootsteps();
        HandleIdleSound();
    }

    //Доктор, всё будет хорошо?
    private void HandleFootsteps()
    {
        if (rb == null)
        {
            if (enableLogs) Debug.LogWarning("CharacterFootsteps: Rigidbody2D is null");
            return;
        }

        if (AudioManager.Instance == null)
        {
            if (enableLogs) Debug.LogWarning("CharacterFootsteps: AudioManager.Instance is null");
            return;
        }

        float linearVelocityX = Mathf.Abs(rb.linearVelocityX);

        bool shouldPlay = Mathf.Abs(xPosLastFrame - transform.position.x) > 0;
        //(enemyLogicUse?enemyLogic.currentState == EnemyState.Walking:true)
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
                Debug.Log($"CharacterFootsteps: step | velocityX={linearVelocityX} | interval={currentStepInterval}");
            }
        
            AudioManager.Instance.PlayDoctorFootstep(transform.position);
            xPosLastFrame = transform.position.x;
            stepTimer = currentStepInterval;
        }
    }

    //Кхем-кхем
    private void HandleIdleSound()
    {
        if (AudioManager.Instance == null)
            return;

        idleSoundTimer -= Time.deltaTime;

        if (idleSoundTimer <= 0f)
        {
            AudioManager.Instance.PlayDoctorIdle(transform.position);
            ResetIdleSoundTimer();
        }
    }
    private void ResetIdleSoundTimer()
    {
        idleSoundTimer = Random.Range(minIdleSoundDelay, maxIdleSoundDelay);
    }

    //Ой
    public void HandleDamage()
    {
        AudioManager.Instance.DoctorTakeDamage(transform.position);
    }

    //Скидыщь
    public void HandleAttack()
    {
        AudioManager.Instance.DoctorAttack(transform.position);
    }

    public void HandleAttack2()
    {
        AudioManager.Instance.DoctorAttack2(transform.position);
    }
}
