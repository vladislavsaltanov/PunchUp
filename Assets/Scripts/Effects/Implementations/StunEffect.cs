using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Stun Effect")]
public class StunEffect : EntityEffectData
{
    public override void Execute(BaseEntity entity, ActiveEffect activeEffect)
    {
        activeEffect.RuntimeData = 0f;
    }

    public override void Remove(BaseEntity entity, ActiveEffect activeEffect)
    {
    }

    public override void Tick(BaseEntity entity, ActiveEffect activeEffect, float deltaTime)
    {
        entity.ApplyVelocityOverride(Vector2.zero, 1f);
    }
}