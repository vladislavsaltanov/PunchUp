using UnityEngine;

public class BatAudio : MonoBehaviour
{
    //public static DoctorAudio Instance { get; private set; }

    [Header("References")]
    [SerializeField] private EnemyLogic enemyLogic;

    [Header("Movement Threshold")]
    [SerializeField] private float minVelocityToPlay = 1.0f;


    [Header("Speed Mapping")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 6f;

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

    private void Awake()
    {

        if (enemyLogic == null)
            enemyLogic = GetComponent<EnemyLogic>();

        xPosLastFrame = transform.position.x;
        ResetIdleSoundTimer();
    }

    private void Update()
    {
        HandleIdleSound();
    }

    //Доктор, всё будет хорошо?
    public void HandleFly()
    {
        if (AudioManager.Instance == null)
        {
            if (enableLogs) Debug.LogWarning("CharacterFootsteps: AudioManager.Instance is null");
            return;
        }
        if (Time.time < footstepBlockedUntil)
        {
            stepTimer = 0f;
            return;
        }

        stepTimer -= Time.deltaTime;

        if (stepTimer <= 0f)
        {
            AudioManager.Instance.PlayDoctorFootstep(transform.position);
            xPosLastFrame = transform.position.x;
        }
    }

    //Вау-Вау
    private void HandleIdleSound()
    {
        if (AudioManager.Instance == null)
            return;

        idleSoundTimer -= Time.deltaTime;

        if (idleSoundTimer <= 0f)
        {
            AudioManager.Instance.PlayBatIdle(transform.position);
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
        AudioManager.Instance.BatTakeDamage(transform.position);
    }

    //Скидыщь
    public void HandleAttack()
    {
        AudioManager.Instance.BatAttack(transform.position);
    }
}
