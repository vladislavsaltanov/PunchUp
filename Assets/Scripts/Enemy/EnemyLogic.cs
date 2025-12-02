using System;
using System.Collections;
using UnityEngine;

public class EnemyLogic : BaseEntity
{
    #region Modules
    [Header("Modules")]
    [SerializeField] EnemyMovementBaseSO movement;
    [SerializeField] EnemyCombatLogicSO combat;
    //[SerializeField] EnemyAbilitySO abilityModule; // может быть null
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
    PlayerController playerController;
    GameObject player => playerController?.gameObject ?? this.gameObject;
    #endregion

    protected override void Awake()
    {
        base.Awake();

        StartCoroutine(WaitFor(UnityEngine.Random.Range(1f, 3f), () => currentState = EnemyState.Walking));
        direction = (sbyte)(UnityEngine.Random.value > 0.5f ? 1 : -1);

        playerController = PlayerController.instance;
    }

    private void Update()
    {
        // calculating distance to player (only x axis)
        context.playerDistance = Mathf.Abs(player.transform.position.x - gameObject.transform.position.x);

        if (!isWaiting)
            CycleStates();
        else
            return;

        switch (currentState)
        {
            case EnemyState.Walking:
                movement.Movement(this, context);

            break;


            case EnemyState.WalkingTowardsPlayer:
                movement.MovementTowardsPlayer(this, context, detection, direction);
            break;


            case EnemyState.Combat:

            break;


            case EnemyState.UsingAbility:

            break;
        }
    }

    // Checks and cycles through states
    void CycleStates()
    {
        sbyte direction = detection.IsPlayerDetected(this, out context.playerDistance);

        if (direction != 0)
        {
            this.direction = direction;
            currentState = EnemyState.WalkingTowardsPlayer;
        }
        else if (currentState == EnemyState.WalkingTowardsPlayer)
            currentState = EnemyState.Walking;


    }

    public IEnumerator WaitFor(float time, Action action)
    {
        yield return new WaitForSeconds(time);
        action?.Invoke();
    }



    #region Base Entity Implementation
    protected override void OnDamageReceived(ushort amount, Transform atacker = null)
    {
    }

    protected override void OnDeath()
    {
    }
    #endregion

}
public class EnemyContextState
{
    public bool isPlayerDetected = false;
    public float playerDistance = Mathf.Infinity;
    public Coroutine currentCoroutine = null;
}
public class EnemyCombatState
{
}