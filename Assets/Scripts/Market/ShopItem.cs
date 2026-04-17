using UnityEngine;

[System.Serializable]
public class ShopItem
{
    public ItemData item;
    [HideInInspector] public bool isSold;
}