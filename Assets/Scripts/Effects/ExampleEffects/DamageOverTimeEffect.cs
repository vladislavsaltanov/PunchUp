using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Damage Over Time")]
public class DamageOverTimeEffect: EntityEffectData
{
    public ushort damagePerTick = 5;
    public float tickInterval = 1f;

    public override void Execute(BaseEntity entity, ActiveEffect activeEffect)
    {
        activeEffect.RuntimeData = 0f;
    }

    public override void Remove(BaseEntity entity, ActiveEffect activeEffect)
    {
    }

    public override void Tick(BaseEntity entity, ActiveEffect activeEffect, float deltaTime)
    {
        float timer = (float)activeEffect.RuntimeData;
        timer += deltaTime;

        if (timer >= tickInterval)
        {
            timer -= tickInterval;
            entity.TakeDamage(damagePerTick);
        }

        activeEffect.RuntimeData = timer;
    }
}