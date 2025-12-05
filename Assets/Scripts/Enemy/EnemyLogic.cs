using System;
using System.Collections;
using UnityEngine;

public class EnemyLogic : BaseEntity
{
    #region Modules
    [Header("Modules")]
    [SerializeField] EnemyMovementBaseSO movement;
    [SerializeField] EnemyPlayerDetectionSO detection;
    [SerializeField] CombatHandler combatHandler;
    #endregion

    #region AI Settings
    [Header("AI Settings")]
    [SerializeField] float attackRange = 1.5f;
    [SerializeField] float abilityChance = 0.3f;
    [SerializeField] float agroTimeout = 5f;
    [SerializeField] float searchDuration = 2f; 
    #endregion

    #region Movement Triggers
    [Space(20)]
    [Header("Movement")]
    public Collider2D leftSideTrigger;
    public Collider2D rightSideTrigger;
    #endregion

    #region State
    [Space(10)]
    [Header("State")]
    public EnemyState currentState;
    public EnemyContextState context = new EnemyContextState();
    #endregion

    #region Cached
    PlayerController playerController;
    GameObject player => playerController?.gameObject ?? this.gameObject;
    #endregion

    protected override void Awake()
    {
        base.Awake();

        if (combatHandler == null) combatHandler = GetComponent<CombatHandler>();
        playerController = PlayerController.instance;

        direction = (sbyte)(UnityEngine.Random.value > 0.5f ? 1 : -1);
        EnterWait(UnityEngine.Random.Range(1f, 3f));
    }

    private void Update()
    {
        if (CurrentHealth <= 0) return;
        if (HasVelocityOverride) return;

        UpdateDetection();

        if (IsStateLocked()) return;

        EvaluateSituation();

        ExecuteState();
    }

    void UpdateDetection()
    {
        context.directionToPlayer = detection.IsPlayerDetected(this, out context.playerDistance);
        context.isPlayerDetected = context.directionToPlayer != 0;

        if (context.isPlayerDetected)
        {
            context.lastTimeSeenPlayer = Time.time;
            context.lastKnownDirection = context.directionToPlayer;
        }
    }

    bool IsStateLocked()
    {
        return currentState == EnemyState.Waiting ||
               currentState == EnemyState.Attacking ||
               currentState == EnemyState.UsingAbility;
    }

    void EvaluateSituation()
    {
        if (context.wasHit)
        {
            if (Time.time > context.lastHitTime + agroTimeout)
            {
                context.wasHit = false;
            }
            else
            {
                if (!context.isPlayerDetected)
                {
                    direction = context.directionToPlayer; 
                }
                else
                {
                    direction = context.directionToPlayer; 
                }

                ChooseCombatOrChase();
                return;
            }
        }

        if (context.isPlayerDetected)
        {
            direction = context.directionToPlayer;
            ChooseCombatOrChase();
            return;
        }

        if (Time.time < context.lastTimeSeenPlayer + searchDuration)
        {
            direction = context.lastKnownDirection;
            currentState = EnemyState.WalkingTowardsPlayer;
            return;
        }
        
        currentState = EnemyState.Walking;

        if (currentState == EnemyState.Walking && direction == 0)
        {
            // Пнем его в случайную сторону
            direction = (sbyte)(UnityEngine.Random.value > 0.5f ? 1 : -1);
        }
    }

    // Вспомогательный метод для выбора между ударом и бегом
    void ChooseCombatOrChase()
    {
        if (context.isPlayerDetected && context.playerDistance <= attackRange)
        {
            currentState = EnemyState.Combat;
        }
        else
        {
            currentState = EnemyState.WalkingTowardsPlayer;
        }
    }

    void ExecuteState()
    {
        switch (currentState)
        {
            case EnemyState.Walking:
                movement.Movement(this, context);
                break;

            case EnemyState.WalkingTowardsPlayer:
                movement.MovementTowardsPlayer(this, context, detection, direction);
                break;

            case EnemyState.Combat:
                HandleCombat();
                break;
        }
    }

    void HandleCombat()
    {
        movement.Stop(this);

        if (specialAbility != null && UnityEngine.Random.value < abilityChance)
        {
            if (combatHandler.TrySpecialAbility())
            {
                StartCoroutine(PerformActionRoutine(EnemyState.UsingAbility, specialAbility));
                return;
            }
        }

        if (combatHandler.TryPrimaryAttack())
        {
            StartCoroutine(PerformActionRoutine(EnemyState.Attacking, primaryAttack));
        }
    }

    IEnumerator PerformActionRoutine(EnemyState actionState, ActionSO action)
    {
        currentState = actionState;
        yield return new WaitForSeconds(action.duration);

        currentState = EnemyState.Waiting;
        yield return new WaitForSeconds(0.5f);

        currentState = EnemyState.Walking;
    }

    public void EnterWait(float duration)
    {
        currentState = EnemyState.Waiting;
        movement.Stop(this);
        StartCoroutine(WaitFor(duration, () => currentState = EnemyState.Walking));
    }

    public IEnumerator WaitFor(float time, Action action)
    {
        yield return new WaitForSeconds(time);
        action?.Invoke();
    }

    #region Base Entity Implementation
    protected override void OnDamageReceived(ushort amount, Transform attacker = null)
    {
        if (attacker != null && ((1 << attacker.gameObject.layer) & detection.playerLayer.value) != 0)
        {
            context.directionToPlayer = (sbyte)(attacker.position.x - transform.position.x > 0 ? 1 : -1);
            context.lastHitTime = Time.time;
            context.wasHit = true;

            if (currentState == EnemyState.Walking || currentState == EnemyState.Waiting)
            {
                StopAllCoroutines();
                currentState = EnemyState.Walking;
            }
        }
    }
    #endregion
}

[System.Serializable]
public class EnemyContextState
{
    public bool isPlayerDetected = false;
    public bool wasHit = false;
    public float playerDistance = Mathf.Infinity;
    public float lastHitTime = -100f;

    // Для логики "потерял из виду"
    public float lastTimeSeenPlayer = -100f;
    public sbyte lastKnownDirection = 0;

    public sbyte directionToPlayer = 0;
}