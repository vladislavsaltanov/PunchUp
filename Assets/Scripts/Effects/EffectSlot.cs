using UnityEngine;
using UnityEngine.UI;

public class EffectSlot : MonoBehaviour
{
    public string effectName;
    public string description;
    public Sprite effectSprite;
    public Sprite defaultSprite;
    public bool isFull;
    public float duration;

    [SerializeField] private Image effectImage;
    [SerializeField] private GameObject effectImageObject;

    public void AddEffect(EntityEffectData effect)
    {
        this.effectName = effect.effectName;
        this.description = effect.description;
        this.effectSprite = effect.icon ?? defaultSprite;
        this.isFull = true;
        this.duration = effect.duration;

        effectImageObject.SetActive(true);
        effectImage.sprite = effectSprite;
    }

    public void RemoveEffect()
    {
        this.effectName = null;
        this.description = null;
        this.effectSprite = null;
        this.isFull = false;
        this.duration = 0;

        effectImageObject.SetActive(false);
        effectImage = null;
    }
}
