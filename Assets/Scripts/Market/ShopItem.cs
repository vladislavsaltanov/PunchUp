using System;
using UnityEngine;

[Serializable]
public class ShopItem
{
    public ItemData item;
    public int price;

    [NonSerialized]
    public bool isSold;
}