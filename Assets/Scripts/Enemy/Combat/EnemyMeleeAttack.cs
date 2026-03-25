using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Combat/Melee")]
public class EnemyMeleeAttack : EnemyCombatLogicSO
{
    [Header("Attack Settings")]
    public float attackRange = 1.5f;
    public float attackDuration = 1f;
    public float windupTime = 0.1f;
    public float attackCooldown = 3f;
    public float knockbackForce = 3f;
    public float knockbackDuration = 0.5f;
    public ushort damage = 10;

    [Header("Hitbox")]
    public Vector2 hitboxSize = new Vector2(1f, 1f);
    public Vector2 hitboxOffset = new Vector2(0.8f, 0f);
    public LayerMask targetLayer;

    public override bool IsPlayerCloseEnough(float distanceToPlayer)
    {
        return distanceToPlayer <= attackRange;
    }

    public override void Execute(EnemyLogic enemy, EnemyContextState context, GameObject player)
    {
        if (enemy.currentState != EnemyState.Attacking && enemy.currentState != EnemyState.Waiting)
            _ = AttackRoutine(enemy, context);
    }

    async Awaitable AttackRoutine(EnemyLogic enemy, EnemyContextState context)
    {
        enemy.currentState = EnemyState.Attacking;

        await Awaitable.WaitForSecondsAsync(windupTime);

        Vector2 center = (Vector2)enemy.transform.position +
                         new Vector2(hitboxOffset.x * enemy.direction, hitboxOffset.y);

        var hits = Physics2D.OverlapBoxAll(center, hitboxSize, 0, targetLayer);

        foreach (var hit in hits)
        {
            if (hit.transform == enemy.transform) continue;

            var entity = hit.GetComponentInParent<BaseEntity>();
            if (entity == null) continue;
            if (entity == enemy) continue;

            // Урон
            entity.TakeDamage(damage, enemy.transform, enemy._name);
            float dirX = Mathf.Sign(hit.transform.position.x - enemy.transform.position.x);
            Vector2 knockback = new Vector2(dirX * knockbackForce, knockbackForce * 0.3f);
            entity.ApplyVelocityOverride(knockback, knockbackDuration);
        }

        enemy.currentState = EnemyState.Waiting;

        // Восстановление после удара
        await Awaitable.WaitForSecondsAsync(attackDuration - windupTime);

        // Кулдаун
        await Awaitable.WaitForSecondsAsync(attackCooldown);
        enemy.currentState = EnemyState.Walking;
    }
}