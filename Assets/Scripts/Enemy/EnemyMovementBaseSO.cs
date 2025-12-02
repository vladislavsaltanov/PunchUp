using UnityEngine;

public abstract class EnemyMovementBaseSO : ScriptableObject
{
    [Space(10)]
    [Header("Detection")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] LayerMask obstacleLayer;

    public abstract void Movement(EnemyLogic logic, EnemyContextState state);
    public abstract void MovementTowardsPlayer(EnemyLogic logic, EnemyContextState state, EnemyPlayerDetectionSO playerDetection, float playerDirection);

    public bool IsBlocked(EnemyLogic logic, sbyte direction) =>
               (direction < 0 && (logic.leftSideTrigger.IsTouchingLayers(obstacleLayer) || !logic.leftSideTrigger.IsTouchingLayers(groundLayer))) ||
               (direction > 0 && (logic.rightSideTrigger.IsTouchingLayers(obstacleLayer) || !logic.rightSideTrigger.IsTouchingLayers(groundLayer)));

    public virtual void Stop(EnemyLogic enemy)
    {
        enemy.rb.linearVelocityX = 0;
    }
}
