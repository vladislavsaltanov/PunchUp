using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Melee Attack")]
public class AttackSO : ActionSO
{
    [Header("Damage")]
    public ushort baseDamage = 10;
    public float windupTime = 0.15f;

    [Header("Knockback")]
    public Vector2 knockbackForce = new Vector2(8f, 4f);
    public float knockbackDuration = 0.2f;

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

        yield return new WaitForSeconds(duration - windupTime);

        onComplete?.Invoke();
    }

    void PerformHit(BaseEntity owner)
    {
        Vector2 center = (Vector2)owner.transform.position +
                         new Vector2(hitboxOffset.x * owner.direction, hitboxOffset.y);

        var hits = Physics2D.OverlapBoxAll(center, hitboxSize, 0f, targetLayers);

        foreach (var hit in hits)
        {
            if (hit.transform == owner.transform) continue;
            if (hit.transform.IsChildOf(owner.transform)) continue;

            var target = hit.GetComponentInParent<BaseEntity>();
            if (target == null) continue;
            if (target == owner) continue;

            target.TakeDamage((ushort)(baseDamage * owner.Stats["damage"]), owner.transform);

            Vector2 knockback = new Vector2(owner.direction * knockbackForce.x, knockbackForce.y);
            target.ApplyVelocityOverride(knockback, knockbackDuration);
        }
    }

    public void DrawGizmo(Transform owner, sbyte direction)
    {
        Vector2 center = (Vector2)owner.position +
                         new Vector2(hitboxOffset.x * direction, hitboxOffset.y);
        Gizmos.DrawWireCube(center, hitboxSize);
    }
}