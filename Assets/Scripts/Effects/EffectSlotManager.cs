using UnityEngine;
using System.Collections.Generic;

public class EffectSlotManager : MonoBehaviour
{

    [SerializeField] public List<EffectSlot> effectSlots;
    [SerializeField] private EntityEffectsSystem effectsSystem;
    [SerializeField] private GameObject effectSlotPrefab;
    [SerializeField] private Transform slotContaner;

    private List<EntityEffectData> currrentEffects = new List<EntityEffectData>();

    public void AddEffect(EntityEffectData effect)
    {
        if (currrentEffects.Contains(effect)) return;

        currrentEffects.Add(effect);
        EffectSlot slot = Instantiate(effectSlotPrefab, slotContaner).GetComponent<EffectSlot>();
        slot.AddEffect(effect);
        effectSlots.Add(slot);
    }

    public void RemoveEffect(EntityEffectData effect)
    {
        foreach (var slot in effectSlots)
        {
            if (slot.effectName == effect.effectName)
            {
                currrentEffects.Remove(effect);
                effectSlots.Remove(slot);
                Destroy(slot);
                return;
            }
        }
    }
}
