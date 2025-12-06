using UnityEngine;

[CreateAssetMenu(fileName = "EnemyBaseMovement", menuName = "ScriptableObjects/Enemy/EnemyBaseMovement", order = 1)]
public class EnemyBaseMovement : EnemyMovementBaseSO
{
    public float rotationDelay = 2f;
    public float movementTowardsPlayerSpeedMultiplier = 1.5f;

    public override void Movement(EnemyLogic logic, EnemyContextState state)
    {
        if (IsBlocked(logic, logic.direction))
        {
            logic.rb.linearVelocityX = 0;

            logic.direction *= -1;

            logic.EnterWait(rotationDelay);
            return;
        }

        if (logic.currentState != EnemyState.Waiting)
        {
            float speed = logic.Stats["speed"];
            logic.rb.linearVelocityX = logic.direction * speed;
        }
    }

    public override void MovementTowardsPlayer(EnemyLogic logic, EnemyContextState context, EnemyPlayerDetectionSO playerDetection, sbyte direction)
    {
        logic.direction = direction;

        if (IsBlocked(logic, direction))
        {
            logic.rb.linearVelocityX = 0;
            return;
        }

        if (!playerDetection.isPlayerTooClose(context.playerDistance))
        {
            float baseSpeed = logic.Stats["speed"];
            logic.rb.linearVelocityX = direction * baseSpeed * movementTowardsPlayerSpeedMultiplier;
        }
        else
        {
            logic.rb.linearVelocityX = 0;
        }
    }
}