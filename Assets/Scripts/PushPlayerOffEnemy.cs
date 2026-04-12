using UnityEngine;

public class PushPlayerOffEnemy : MonoBehaviour
{
    [SerializeField] private BaseEntity _player;
    [SerializeField] private float _pushStrengthX = 12;
    [SerializeField] private float _pushStrengthY = 8;
    [SerializeField] private float _duration = 0.1f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && IsPlayerOnTop(collision))
        {
            GameObject enemy = collision.gameObject;
            GameObject player = this.gameObject;
            BaseEntity entity = enemy.GetComponent<EnemyLogic>();
            Vector2 direction = GetPushDirection(player, enemy);

            ApplyVelocity(_player, direction, _duration);
            ApplyVelocity(entity, -direction, _duration);
        }
    }

    private Vector2 GetPushDirection(GameObject player, GameObject enemy)
    {
        Vector2 playerPosition = player.transform.position;
        Vector2 enemyPosition = enemy.transform.position;

        float xDifference = enemyPosition.x - playerPosition.x;
        Vector2 pushDirection;

        if (xDifference > 0)
        {
            pushDirection = new Vector2(-_pushStrengthX, _pushStrengthY);
        }
        else
        {
            pushDirection = new Vector2(_pushStrengthX, _pushStrengthY);
        }
        return pushDirection;
    }

    private void ApplyVelocity(BaseEntity entity, Vector2 direction, float duration)
    {
        entity.ApplyVelocityOverride(direction, duration);
    }

    private bool IsPlayerOnTop(Collision2D collision)
    {
        GameObject enemy = collision.gameObject;
        GameObject player = this.gameObject;

        Collider2D enemyCollider = enemy.transform.Find("Square").GetComponent<Collider2D>();
        Collider2D playerCollider = player.transform.Find("Square").GetComponent<Collider2D>();

        Bounds playerBounds = playerCollider.bounds;
        Bounds enemyBounds = enemyCollider.bounds;

        float enemyTop = enemyBounds.max.y;
        float playerBottom = playerBounds.min.y;
        float playerCenterY = playerBounds.center.y;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.point.y > enemyBounds.center.y)
            {
                if (playerCenterY > enemyBounds.center.y)
                {
                    float verticalDistance = playerBottom - enemyTop;
                    if (verticalDistance > -0.2f && verticalDistance < 0.5f)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}