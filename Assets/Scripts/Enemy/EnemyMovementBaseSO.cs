using UnityEngine;

public abstract class EnemyMovementBaseSO : ScriptableObject
{
    [Space(10)]
    [Header("Detection")]
    public LayerMask groundLayer;
    public LayerMask obstacleLayer;

    [Header("Wall Detection")]
    public float wallCheckDistance = 0.5f; // Длина луча от "лица" врага

    [Header("Ledge Detection")]
    public float ledgeLookAhead = 0.5f; // Насколько далеко ОТ КРАЯ коллайдера проверять пол
    public float ledgeRayLength = 1.0f; // Длина луча вниз

    public abstract void Movement(EnemyLogic logic, EnemyContextState state);
    public abstract void MovementTowardsPlayer(EnemyLogic logic, EnemyContextState context, EnemyPlayerDetectionSO playerDetection, sbyte direction);
    public abstract bool IsBlocked(EnemyLogic logic, sbyte direction);

    public virtual void Stop(EnemyLogic enemy)
    {
        enemy.rb.linearVelocityX = 0;
    }
}
