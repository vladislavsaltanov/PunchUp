using UnityEngine;

public class Test_EffectTrigger : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] EntityEffectData damageOverTimeEffect;
    [SerializeField] float cooldown = 5f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var effectsSystem = collision.GetComponentInParent<EntityEffectsSystem>();

        if (effectsSystem != null && effectsSystem.ActiveEffects.TryGetValue(damageOverTimeEffect, out var activeEffect))
            Debug.Log($"Remaining: {activeEffect.RemainingTime:F1}s");
        else
            Debug.Log("Effect not active");

        effectsSystem.ApplyEffect(damageOverTimeEffect);
    }
}
