using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Thunder Force")]
public class ThunderForceEffect : EntityEffectData
{
    [Range(0f, 1f)] public float healthThreshold = 0.2f;
    public float radius = 6.5f;
    public ushort damage = 30;
    public float cooldown = 180f;
    public LayerMask enemyLayers;
    public GameObject thunderVfxPrefab;

    public override void Execute(BaseEntity entity, ActiveEffect activeEffect)
    {
        var state = new ThunderEffectState();
        state.onDamage = () => TryActivate(entity, state);
        activeEffect.RuntimeData = state;
        entity.OnDamageEvent += state.onDamage;
    }

    public override void Remove(BaseEntity entity, ActiveEffect activeEffect)
    {
        entity.OnDamageEvent -= ((ThunderEffectState)activeEffect.RuntimeData).onDamage;
    }

    public override void Tick(BaseEntity entity, ActiveEffect activeEffect, float deltaTime)
    {
        var state = (ThunderEffectState)activeEffect.RuntimeData;
        if (state.cooldownTimer > 0f)
            state.cooldownTimer = Mathf.Max(0f, state.cooldownTimer - deltaTime);
    }

    void TryActivate(BaseEntity entity, ThunderEffectState state)
    {
        if (state.cooldownTimer > 0f) return;
        float hpPercent = (float)entity.CurrentHealth / entity.Stats[StatType.MaxHealth];
        if (hpPercent > healthThreshold) return;

        var hits = Physics2D.OverlapCircleAll(entity.transform.position, radius, enemyLayers);
        foreach (var hit in hits)
        {
            var target = hit.GetComponentInParent<BaseEntity>() ?? hit.GetComponent<BaseEntity>();
            if (target != null && target != entity)
                target.TakeDamage(damage, entity.transform, "Thunder Force");
        }

        if (thunderVfxPrefab != null)
            Instantiate(thunderVfxPrefab, entity.transform.position, Quaternion.identity);

        state.cooldownTimer = cooldown;
    }
}
public class ThunderEffectState
{
    public float cooldownTimer;
    public Action onDamage;
}
