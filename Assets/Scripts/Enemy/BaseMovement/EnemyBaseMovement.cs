using UnityEngine;

[CreateAssetMenu(fileName = "EnemyBaseMovement", menuName = "ScriptableObjects/Enemy/EnemyBaseMovement", order = 1)]
public class EnemyBaseMovement : EnemyMovementBaseSO
{
    public float movementSpeed = 3f;
    public float rotationDelay = 2f;
    public float movementTowardsPlayerSpeedMultiplier = 1.5f;

    public override void Movement(EnemyLogic logic, EnemyContextState state)
    {
/*        if (logic.nearLeftWallOrEdge && logic.direction == -1 && !logic.isWaiting)
        {
            logic.direction = 1;
            logic.isWaiting = true;
            logic.rb.linearVelocityX = 0;

            if (state.currentCoroutine != null)
                logic.StopCoroutine(state.currentCoroutine);

            state.currentCoroutine = logic.StartCoroutine(logic.WaitFor(rotationDelay, () => logic.isWaiting = false));
        }
        else if (logic.nearRightWallOrEdge && logic.direction == 1 && !logic.isWaiting)
        {
            logic.direction = -1;
            logic.isWaiting = true;
            logic.rb.linearVelocityX = 0;

            if (state.currentCoroutine != null)
                logic.StopCoroutine(state.currentCoroutine);

            state.currentCoroutine = logic.StartCoroutine(logic.WaitFor(rotationDelay, () => logic.isWaiting = false));
        }
*/
        if (!logic.isWaiting)
            logic.rb.linearVelocityX = logic.direction * movementSpeed;
    }

    // Movement towards player
    public override void MovementTowardsPlayer(EnemyLogic logic, EnemyContextState state, EnemyPlayerDetectionSO playerDetection, float playerDirection)
    {
        logic.direction = (sbyte)playerDirection;

        if (logic.currentState == EnemyState.WalkingTowardsPlayer && Mathf.Abs(logic.transform.position.x - PlayerController.instance.transform.position.x) > playerDetection.minimalDistanceToPlayer)
            logic.rb.linearVelocityX = logic.direction * movementSpeed * movementTowardsPlayerSpeedMultiplier;
    }
}