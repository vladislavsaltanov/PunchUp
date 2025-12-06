using UnityEngine;

public abstract class ItemData : ScriptableObject
{
    public string itemName;
    [TextArea(3, 5)]
    public string description;
    public Sprite icon;
}