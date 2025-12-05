using UnityEngine;

public abstract class EnemyMovementBaseSO : ScriptableObject
{
    [Space(10)]
    [Header("Detection")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] LayerMask obstacleLayer;

    [Header("Wall Detection")]
    [SerializeField] float wallCheckDistance = 0.15f; 
    [SerializeField] float wallCheckHeight = 0.15f;

    public abstract void Movement(EnemyLogic logic, EnemyContextState state);
    public abstract void MovementTowardsPlayer(EnemyLogic logic, EnemyContextState context, EnemyPlayerDetectionSO playerDetection, sbyte direction);

    public bool IsBlocked(EnemyLogic logic, sbyte direction)
    {
        Collider2D groundSensor = direction < 0 ? logic.leftSideTrigger : logic.rightSideTrigger;

        if (groundSensor != null)
            if (!groundSensor.IsTouchingLayers(groundLayer | obstacleLayer))
                return true;

        Collider2D bodyCollider = logic.GetComponentInChildren<Collider2D>();
        Bounds b = bodyCollider.bounds;

        float frontX = direction > 0 ? b.max.x : b.min.x;

        frontX += direction * 0.05f;

        Vector2 origin = new Vector2(frontX, logic.transform.position.y + wallCheckHeight);
        Vector2 dir = Vector2.right * direction;


        RaycastHit2D hit = Physics2D.Raycast(origin, dir, wallCheckDistance, obstacleLayer | groundLayer);

        if (hit.collider != null)
            return true;

        return false;
    }

    public virtual void Stop(EnemyLogic enemy)
    {
        enemy.rb.linearVelocityX = 0;
    }
}
