using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSlot : Selectable, ISubmitHandler, IPointerClickHandler
{
    public string itemName;
    public int quantity;
    public Sprite itemSprite;
    public bool isFull;
    public string description;

    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Image itemImage;

    [SerializeField] private GameObject quantityTextObject;
    [SerializeField] private GameObject itemImageObject;

    [SerializeField] private Inventory inventory;

    public Image itemDescriptionImage;
    public TMP_Text ItemNameText;
    public TMP_Text ItemDescriptionText;

    public GameObject selectedShader;
    public bool thisItemSelected;

    [SerializeField] InventoryManager inventoryManager;

    public void AddItem(ItemData item)
    {
        this.itemName = item.itemName;
        this.quantity = 1;
        this.itemSprite = item.icon;
        this.description = item.description;
        isFull = true;

        quantityTextObject.SetActive(true);
        itemImageObject.SetActive(true);
        quantityText.text = quantity.ToString();
        itemImage.sprite = itemSprite;

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (string.IsNullOrEmpty(itemName)) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftClick();
        }
    }
    public void OnSubmit(BaseEventData eventData)
    {
        if (string.IsNullOrEmpty(itemName)) return;

        OnLeftClick();
    }
    public void OnLeftClick()
    {
        inventoryManager.DeselectAllSlots();
        selectedShader.SetActive(true);
        thisItemSelected = true;

        ItemNameText.text = itemName;
        ItemDescriptionText.text = description;
        itemDescriptionImage.sprite = itemSprite;
    }

    public void ClearSlot()
    {
        itemName = null;
        description = null;
        quantityTextObject.SetActive(false);
        itemImageObject.SetActive(false);
        isFull = false;
    }

    public void AddCount(int n)
    {
        this.quantity += n;
        quantityText.text = quantity.ToString();
    }
}
