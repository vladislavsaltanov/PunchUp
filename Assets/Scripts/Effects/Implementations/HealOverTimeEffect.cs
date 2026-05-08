using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Heal Over Time")]
public class HealOverTimeEffect : EntityEffectData
{
    public ushort healPerTick = 5;
    public float tickInterval = 1f;
    public float noDamageCooldown = 3f;
    [Range(0f, 1f)] public float healPercent = 0f; // 0 = infinite (container with food), >0 = until % of health
    [Range(0f, 1f)] public float healPerTickPercent = 0f;

    public override void Execute(BaseEntity entity, ActiveEffect activeEffect)
    {
        var state = new HealEffectState();
        state.onDamage = () => state.healRemaining = entity.Stats[StatType.MaxHealth] * healPercent;
        entity.OnDamageEvent += state.onDamage;
        activeEffect.RuntimeData = state;
    }

    public override void Remove(BaseEntity entity, ActiveEffect activeEffect)
    {
        var state = (HealEffectState)activeEffect.RuntimeData;
        entity.OnDamageEvent -= state.onDamage;
    }

    public override void Tick(BaseEntity entity, ActiveEffect activeEffect, float deltaTime)
    {
        var state = (HealEffectState)activeEffect.RuntimeData;
        if (state.healRemaining <= 0f) return;
        if (Time.time - entity.LastDamageTime < noDamageCooldown) return;

        state.tickTimer += deltaTime;
        if (state.tickTimer >= tickInterval)
        {
            state.tickTimer -= tickInterval;
            ushort amount = healPerTickPercent > 0f
                ? (ushort)Mathf.Min(entity.Stats[StatType.MaxHealth] * healPerTickPercent, state.healRemaining)
                : (ushort)Mathf.Min(healPerTick, state.healRemaining);
            entity.Heal(amount);
            state.healRemaining -= amount;
        }
    }
}

public class HealEffectState
{
    public float tickTimer;
    public float healRemaining;
    public Action onDamage;
}
