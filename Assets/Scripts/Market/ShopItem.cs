using UnityEngine;

[System.Serializable]
public class ShopItem
{
    public ItemData item;
    public int price;
    [HideInInspector] public bool isSold;
}