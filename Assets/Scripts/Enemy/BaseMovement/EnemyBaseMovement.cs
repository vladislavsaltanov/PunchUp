using UnityEngine;

[CreateAssetMenu(fileName = "EnemyBaseMovement", menuName = "ScriptableObjects/Enemy/EnemyBaseMovement", order = 1)]
public class EnemyBaseMovement : EnemyMovementBaseSO
{
    public float movementSpeed = 3f;
    public float rotationDelay = 2f;
    public float movementTowardsPlayerSpeedMultiplier = 1.5f;

    public override void Movement(EnemyLogic logic, EnemyContextState state)
    {
        if (IsBlocked(logic, logic.direction) && logic.currentState != EnemyState.Waiting)
        {
            logic.direction *= -1;
            logic.isWaiting = true;
            logic.rb.linearVelocityX = 0;
            logic.currentState = EnemyState.Waiting;

            if (state.currentCoroutine != null)
                logic.StopCoroutine(state.currentCoroutine);

            state.currentCoroutine = logic.StartCoroutine(logic.WaitFor(rotationDelay, () =>
            {
                logic.currentState = EnemyState.Idle;
                state.currentCoroutine = null;
                logic.isWaiting = false;
            }
            ));
        }

        if (logic.currentState != EnemyState.Waiting)
            logic.rb.linearVelocityX = logic.direction * movementSpeed;
    }

    // Movement towards player
    public override void MovementTowardsPlayer(EnemyLogic logic, EnemyContextState context, EnemyPlayerDetectionSO playerDetection, float playerDirection)
    {
        if (IsBlocked(logic, context.directionToPlayer))
        {
            logic.rb.linearVelocityX = 0;
            return;
        }

        if (!playerDetection.isPlayerTooClose(context.playerDistance))
            logic.rb.linearVelocityX = context.directionToPlayer * movementSpeed * movementTowardsPlayerSpeedMultiplier;
    }
}