using UnityEngine;

public class EffectSlotManager : MonoBehaviour
{

    [SerializeField] public EffectSlot[] effectSlots;

    private EntityEffectsSystem effectsSystem;
    public void AddEffect(EntityEffectData effect)
    {
        foreach (var slot in effectSlots)
        {
            if (!slot.isFull)
            {
                slot.AddEffect(effect);
                return;
            }
        }
    }

    public void RemoveEffect(EntityEffectData effect)
    {
        foreach (var slot in effectSlots)
        {
            if (slot.effectName == effect.effectName)
            {
                slot.RemoveEffect();
                return;
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        effectsSystem = GameObject.Find("Player").GetComponent<EntityEffectsSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
