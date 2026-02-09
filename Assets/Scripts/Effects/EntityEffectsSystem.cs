using System;
using System.Collections.Generic;
using UnityEngine;

public class EntityEffectsSystem : MonoBehaviour
{
    [SerializeField] BaseEntity entity;

    Dictionary<EntityEffectData, ActiveEffect> activeEffects = new();
    List<EntityEffectData> expiredBuffer = new();

    public event Action<ActiveEffect> OnEffectAdded;
    public event Action<ActiveEffect> OnEffectRefreshed;
    public event Action<ActiveEffect> OnEffectRemoved;

    public IReadOnlyDictionary<EntityEffectData, ActiveEffect> ActiveEffects => activeEffects;

    void Awake()
    {
        if (entity == null)
            entity = GetComponent<BaseEntity>();
    }

    void Update()
    {
        TickEffects(Time.deltaTime);
    }

    public void ApplyEffect(EntityEffectData effectData)
    {
        if (effectData == null) return;

        if (activeEffects.TryGetValue(effectData, out var existing))
        {
            existing.Refresh();
            OnEffectRefreshed?.Invoke(existing);
            return;
        }

        var newEffect = new ActiveEffect(effectData, entity);
        activeEffects[effectData] = newEffect;

        newEffect.Execute();
        OnEffectAdded?.Invoke(newEffect);
    }

    public void RemoveEffect(EntityEffectData effectData)
    {
        if (!activeEffects.TryGetValue(effectData, out var effect))
            return;

        effect.Remove();
        activeEffects.Remove(effectData);

        OnEffectRemoved?.Invoke(effect);
    }

    public void RemoveAllEffects()
    {
        foreach (var effect in activeEffects.Values)
        {
            effect.Remove();
            OnEffectRemoved?.Invoke(effect);
        }
        activeEffects.Clear();
    }

    void TickEffects(float deltaTime)
    {
        expiredBuffer.Clear();

        foreach (var kvp in activeEffects)
        {
            kvp.Value.Tick(deltaTime);

            if (kvp.Value.IsExpired)
                expiredBuffer.Add(kvp.Key);
        }

        foreach (var key in expiredBuffer)
            RemoveEffect(key);
    }

    public bool HasEffect(EntityEffectData effectData) => activeEffects.ContainsKey(effectData);
}