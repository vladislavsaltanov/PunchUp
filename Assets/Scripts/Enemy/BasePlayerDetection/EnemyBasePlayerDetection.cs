using UnityEngine;

[CreateAssetMenu(fileName = "EnemyBasePlayerDetection", menuName = "ScriptableObjects/Enemy/EnemyBasePlayerDetection")]
public class EnemyBasePlayerDetection : EnemyPlayerDetectionSO
{
    public override sbyte IsPlayerDetected(EnemyLogic logic, out float distance)
    { 
        Vector2 direction = new Vector2(logic.direction, 0);
        Vector2 origin = (Vector2)logic.transform.position + new Vector2(rayOffset.x * logic.direction, rayOffset.y);

        RaycastHit2D ray = Physics2D.Raycast(origin, direction, rayLength, playerLayer | obstacleLayer);

        distance = 0f;

        if (ray.collider == null)
            return (sbyte)0;

        if (((1 << ray.collider.gameObject.layer) & playerLayer.value) != 0)
        {
            distance = Mathf.Abs(ray.collider.transform.position.x - logic.transform.transform.position.x);

            return ray.collider.transform.position.x < logic.transform.transform.position.x ? (sbyte)-1 : (sbyte)1;
        }
        else
            return (sbyte)0;
    }
}
