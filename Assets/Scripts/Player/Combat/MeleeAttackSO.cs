using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Melee Attack")]
public class MeleeAttackSO : AttackSO
{
    [Header("Hitbox")]
    public Vector2 hitboxSize = new Vector2(1f, 1f);
    public Vector2 hitboxOffset = new Vector2(0.8f, 0f);
    public LayerMask targetLayers;

    public override IEnumerator Execute(BaseEntity owner, Action onComplete = null)
    {
        if (!string.IsNullOrEmpty(animationTrigger) && owner.animator != null)
            owner.animator.SetTrigger(animationTrigger);

        yield return new WaitForSeconds(windupTime);

        PerformHit(owner);

        float remainingTime = duration - windupTime;
        if (remainingTime > 0) yield return new WaitForSeconds(remainingTime);

        onComplete?.Invoke();
    }

    void PerformHit(BaseEntity owner)
    {
        Vector2 center = (Vector2)owner.transform.position +
                         new Vector2(hitboxOffset.x * owner.direction, hitboxOffset.y);

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, hitboxSize, 0f, targetLayers);

        foreach (var hit in hits)
        {
            if (hit.transform == owner.transform || hit.transform.IsChildOf(owner.transform) || hit.isTrigger) continue;

            var target = hit.GetComponentInParent<BaseEntity>();
            if (target != null && target != owner)
            {
                ApplyDamageAndKnockback(owner, target);
            }
        }
    }
}