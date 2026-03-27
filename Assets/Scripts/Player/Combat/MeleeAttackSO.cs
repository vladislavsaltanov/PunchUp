using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Melee Attack")]
public class MeleeAttackSO : AttackSO
{
    [Header("Hitbox")]
    public Vector2 hitboxSize = new Vector2(1f, 1f);
    public Vector2 hitboxOffset = new Vector2(0.8f, 0f);
    public LayerMask targetLayers;

    static readonly List<Collider2D> hitBuffer = new List<Collider2D>(16);
    public override async Awaitable Execute(BaseEntity owner, Action onComplete = null)
    {
        try
        {
            if (!string.IsNullOrEmpty(animationTrigger) && owner.animator != null)
                owner.animator.SetTrigger(animationTrigger);

            await Awaitable.WaitForSecondsAsync(windupTime);

            PerformHit(owner);

            float remainingTime = duration - windupTime;
            if (remainingTime > 0)
                await Awaitable.WaitForSecondsAsync(remainingTime);
        }
        finally
        {
            onComplete?.Invoke();
        }
    }

    void PerformHit(BaseEntity owner)
    {
        Vector2 center = (Vector2)owner.transform.position +
                         new Vector2(hitboxOffset.x * owner.direction, hitboxOffset.y);

        var filter = new ContactFilter2D();
        filter.SetLayerMask(targetLayers);
        filter.useLayerMask = true;
        filter.useTriggers = false;

        hitBuffer.Clear();
        Physics2D.OverlapBox(center, hitboxSize, 0f, filter, hitBuffer);

        for (int i = 0; i < hitBuffer.Count; i++)
        {
            var hit = hitBuffer[i];

            if (hit.transform == owner.transform ||
                hit.transform.IsChildOf(owner.transform))
                continue;

            var target = hit.GetComponentInParent<BaseEntity>();
            target = target ?? hit.GetComponent<BaseEntity>();
            if (target != null && target != owner)
            {
                ApplyDamageAndKnockback(owner, target);
            }
        }
    }
}