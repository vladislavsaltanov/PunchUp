using UnityEngine;

[CreateAssetMenu(menuName = "Items/Item")]
public class ItemSO : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public StatModifier[] modifiers;
}