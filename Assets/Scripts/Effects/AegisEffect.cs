using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Aegis")]
public class AegisEffect : EntityEffectData
{
    [Range(0f, 1f)] public float healthThreshold = 0.1f;
    public float shieldDuration = 15f;
    public float cooldown = 120f;

    public override void Execute(BaseEntity entity, ActiveEffect activeEffect)
    {
        var state = new AegisEffectState();
        state.onDamage = () => TryActivate(entity, state);
        activeEffect.RuntimeData = state;
        entity.OnDamageEvent += state.onDamage;
    }

    public override void Remove(BaseEntity entity, ActiveEffect activeEffect)
    {
        var state = (AegisEffectState)activeEffect.RuntimeData;
        entity.OnDamageEvent -= state.onDamage;
        if (state.isActive)
            entity.Stats.RemoveModifier(StatType.DamageBlockChance, state.blockMod);
    }

    public override void Tick(BaseEntity entity, ActiveEffect activeEffect, float deltaTime)
    {
        var state = (AegisEffectState)activeEffect.RuntimeData;

        if (state.cooldownTimer > 0f)
            state.cooldownTimer = Mathf.Max(0f, state.cooldownTimer - deltaTime);

        if (state.isActive)
        {
            state.shieldTimer -= deltaTime;
            if (state.shieldTimer <= 0f)
                Deactivate(entity, state);
        }
    }

    void TryActivate(BaseEntity entity, AegisEffectState state)
    {
        if (state.isActive || state.cooldownTimer > 0f) return;
        float hpPercent = (float)entity.CurrentHealth / entity.Stats[StatType.MaxHealth];
        if (hpPercent > healthThreshold) return;

        state.blockMod = entity.Stats.AddModifier(StatType.DamageBlockChance, percent: 100f, source: this);
        state.isActive = true;
        state.shieldTimer = shieldDuration;
        state.cooldownTimer = cooldown;

        UIManager.Instance.ShowItemNotification(null); // или отдельный поп-ап
    }

    void Deactivate(BaseEntity entity, AegisEffectState state)
    {
        entity.Stats.RemoveModifier(StatType.DamageBlockChance, state.blockMod);
        state.isActive = false;
    }
}

public class AegisEffectState
{
    public float cooldownTimer;
    public float shieldTimer;
    public bool isActive;
    public StatModifier blockMod;
    public Action onDamage;
}
