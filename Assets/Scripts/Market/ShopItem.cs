using UnityEngine;

[CreateAssetMenu(menuName = "Items/Shop item")]
public class ShopItem :ScriptableObject
{
    public ItemData item;
    public int price;
    [HideInInspector] public bool isSold;
}