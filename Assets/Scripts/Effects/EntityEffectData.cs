using UnityEngine;

public abstract class EntityEffectData : ScriptableObject
{
    [Header("Base Info")]
    public string effectName;
    [TextArea] public string description;
    public Sprite icon;
    public float duration = 5f;

    public abstract void Execute(BaseEntity entity, ActiveEffect activeEffect);
    public abstract void Remove(BaseEntity entity, ActiveEffect activeEffect);
    public virtual void Tick(BaseEntity entity, ActiveEffect activeEffect, float deltaTime) { }
}