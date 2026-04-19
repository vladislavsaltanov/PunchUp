using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    public string itemName;
    public int quantity;
    public Sprite itemSprite;
    public bool isFull;

    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Image itemImage;

    [SerializeField] private Inventory inventory;

    public void AddItem(ItemData item)
    {
        Debug.Log($"ItemSlot({item})");
        this.itemName = item.itemName;
        this.quantity = inventory.GetStackCount(item);
        this.itemSprite = item.icon;
        isFull = true;

        itemImage.enabled = true;
        quantityText.enabled = true;
        quantityText.text = quantity.ToString();
        itemImage.sprite = itemSprite;
        
    }
}
