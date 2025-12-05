using System;
using System.Collections;
using UnityEngine;

public class EnemyLogic : BaseEntity
{
    #region Modules
    [Header("Modules")]
    [SerializeField] EnemyMovementBaseSO movement;
    [SerializeField] EnemyCombatLogicSO combat;
    [SerializeField] ActionSO abilityModule; // может быть null
    [SerializeField] EnemyPlayerDetectionSO detection;
    #endregion
    #region Movement
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
    PlayerController playerController = PlayerController.instance ?? null;
    GameObject player => playerController?.gameObject ?? this.gameObject;
    #endregion

    protected override void Awake()
    {
        base.Awake();

        currentState = EnemyState.Waiting;

        StartCoroutine(WaitFor(UnityEngine.Random.Range(2f, 4f), () => currentState = EnemyState.Walking));
        direction = (sbyte)(UnityEngine.Random.value > 0.5f ? 1 : -1);

        playerController = PlayerController.instance;
    }

    private void Update()
    {
        // checking if we see the player
        context.directionToPlayer = detection.IsPlayerDetected(this, out context.playerDistance);
        context.isPlayerDetected = context.directionToPlayer != 0;

        if (currentState == EnemyState.Waiting)
            return;

        // if we were hit recently, walk towards the player
        if (context.wasHit && !combat.HitTimeout(context.lastHitTime))
        {
            currentState = EnemyState.WalkingTowardsPlayer;
        }
        else if (context.wasHit && combat.HitTimeout(context.lastHitTime) && currentState == EnemyState.WalkingTowardsPlayer)
        {
            context.wasHit = false;
            currentState = EnemyState.Idle;
            StartCoroutine(WaitFor(UnityEngine.Random.Range(0.5f, 2f), () => currentState = EnemyState.Walking));
        }
        else
        // if we see the player, check for transitions to other states
        if (context.isPlayerDetected)
        {
            // if the player is close enough, enter combat
            if (combat.IsPlayerCloseEnough(context.playerDistance))
                currentState = EnemyState.Combat;

            // if the player is not out of range, walk towards them
            else if (!detection.isPlayerOutOfRange(context.playerDistance))
                currentState = EnemyState.WalkingTowardsPlayer;

            // if the player is too far, go back to walking
            else
                currentState = EnemyState.Walking;
        }
        else
        {
            // if we don't see the player, go back to walking
            if (currentState != EnemyState.Waiting)
                currentState = EnemyState.Walking;
        }

        switch (currentState)
        {
            case EnemyState.Walking:
                movement.Movement(this, context);
            break;


            case EnemyState.WalkingTowardsPlayer:
                movement.MovementTowardsPlayer(this, context, detection, direction);
            break;


            case EnemyState.Combat:
                combat.Execute(this, context, player);
                break;


            case EnemyState.UsingAbility:

            break;
        }
    }

    public IEnumerator WaitFor(float time, Action action)
    {
        yield return new WaitForSeconds(time);
        action?.Invoke();
    }

    public void EnterWait(float duration)
    {
        currentState = EnemyState.Waiting;
        movement.Stop(this);
        StartCoroutine(WaitFor(duration, () => currentState = EnemyState.Idle));
    }

    #region Base Entity Implementation
    protected override void OnDamageReceived(ushort amount, Transform attacker = null)
    {
        if (attacker != null && ((1 << attacker.gameObject.layer) & detection.playerLayer.value) != 0)
        {
            context.directionToPlayer = (sbyte)(attacker.position.x - transform.position.x > 0 ? 1 : -1);
            context.lastHitTime = Time.time;
            context.wasHit = true;

            if (currentState != EnemyState.Combat && currentState != EnemyState.UsingAbility)
            {
                currentState = EnemyState.Combat;
            }
        }
    }

    protected override void OnDeath()
    {
    }
    #endregion

}
public class EnemyContextState
{
    public bool isPlayerDetected = false, wasHit = false;
    public float playerDistance = Mathf.Infinity, lastHitTime = 0f;
    public sbyte directionToPlayer = 0;
    public Coroutine currentCoroutine = null;
}
public class EnemyCombatState
{
}