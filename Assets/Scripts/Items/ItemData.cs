using UnityEngine;

public abstract class ItemData : ScriptableObject
{
    public string itemName;
    [TextArea(3, 5)]
    public string description;
    public Sprite icon;
}

[CreateAssetMenu(menuName = "Items/Stat Item")]
public class StatItemData : ItemData
{
    public StatModifier[] modifiers;
}

[CreateAssetMenu(menuName = "Items/Ability Item")]
public class AbilityItemData : ItemData
{
    public ActionSO ability;
}