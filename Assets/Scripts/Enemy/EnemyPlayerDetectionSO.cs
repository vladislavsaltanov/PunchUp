using UnityEngine;

public abstract class EnemyPlayerDetectionSO : ScriptableObject
{
    [Header("General Settings")]
    public float minimalDistanceToPlayer = 2f;
    public float rayLength = 5f;
     
    [Header("Detection Settings")]
    public LayerMask playerLayer;
    public LayerMask obstacleLayer;

    [Header("Ray Origin Offset")]
    public Vector2 rayOffset = new Vector2(0, 0.5f);

    public abstract sbyte IsPlayerDetected(EnemyLogic logic, out float distance);
}
