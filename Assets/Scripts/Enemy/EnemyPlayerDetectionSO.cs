using UnityEngine;

public abstract class EnemyPlayerDetectionSO : ScriptableObject
{
    [Header("General Settings")]
    public float minimalDistanceToPlayer = 2f;
    public float maximumDistanceToPlayer = 10f;
    public float rayLength = 5f;
     
    [Header("Detection Settings")]
    public LayerMask playerLayer;
    public LayerMask obstacleLayer;

    [Header("Ray Origin Offset")]
    public Vector2 rayOffset = new Vector2(0, 0.5f);

    public abstract sbyte IsPlayerDetected(EnemyLogic logic, out float distance);
    public virtual bool isPlayerOutOfRange(float distance)
    {
        return distance > maximumDistanceToPlayer;
    }
    public virtual bool isPlayerTooClose(float distance)
    {
        return distance < minimalDistanceToPlayer;
    }
}
