using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Vampirism")]
public class VampirismEffect : EntityEffectData
{
    [Range(0f, 1f)] public float chance = 0.3f;
    [Range(0f, 1f)] public float healPercent = 0.05f;

    public override void Execute(BaseEntity entity, ActiveEffect activeEffect)
    {
        Action<BaseEntity> handler = target =>
        {
            if (UnityEngine.Random.value <= chance)
                entity.Heal((ushort)(entity.Stats[StatType.MaxHealth] * healPercent));
        };
        activeEffect.RuntimeData = handler;
        entity.OnHitEnemy += handler;
    }

    public override void Remove(BaseEntity entity, ActiveEffect activeEffect)
    {
        entity.OnHitEnemy -= (Action<BaseEntity>)activeEffect.RuntimeData;
    }
}
