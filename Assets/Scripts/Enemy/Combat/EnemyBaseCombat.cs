// EnemyBaseCombat.cs
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Combat/Base")]
public class EnemyBaseCombat : EnemyCombatLogicSO
{
    [Header("Attack Settings")]
    public float attackRange = 1.5f;
    public float attackDuration = 1f;
    public float attackCooldown = 3f;
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
            enemy.StartCoroutine(AttackRoutine(enemy, context));
    }

    IEnumerator AttackRoutine(EnemyLogic enemy, EnemyContextState context)
    {
        enemy.currentState = EnemyState.Attacking;

        // Удар
        Vector2 center = (Vector2)enemy.transform.position +
                         new Vector2(hitboxOffset.x * enemy.direction, hitboxOffset.y);

        var hits = Physics2D.OverlapBoxAll(center, hitboxSize, 0, targetLayer);

        foreach (var hit in hits)
        {
            var hitted = hit.GetComponentInParent<IHealth>();
            
            if (hitted != null)
                hitted.TakeDamage(damage, enemy.transform);
        }

        // Кулдаун и возврат к оценке
        //enemy.EnterWait(attackCooldown);
        enemy.currentState = EnemyState.Waiting;
        // Восстановление
        yield return new WaitForSeconds(attackCooldown);
        enemy.currentState = EnemyState.Idle;

    }
}