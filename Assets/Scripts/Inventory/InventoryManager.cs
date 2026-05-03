using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryManager : MonoBehaviour
{
    private bool isInventoryOpen;
    public ItemSlot[] itemSlot;
    [SerializeField] private Inventory inventory;
    [SerializeField] Sprite defaultSprite;
    [SerializeField] private Sprite noItemsSprite;
    [SerializeField] [TextArea] private string emptyInventoryText = "У вас пока нет предметов.";
    [SerializeField] private string emptyInventoryTitle = "Пусто";

    void Start()
    {
        UpdateInventory();
    }

    public void UpdateInventory()
    {
        DeselectAllSlots();
        ClearAllSlots();
        foreach (var item in inventory.items)
        {
            AddItem(item);
        }
    }

    private void OnEnable()
    {
        // Даем небольшую задержку, чтобы контент успел обновиться перед выделением
        // Если UpdateInventory не вызывается при открытии, добавьте его сюда
        SelectFirstItem();
    }

    public void SelectFirstItem()
    {
        // Проверяем, что слоты существуют и базовый слот не пустой
        if (itemSlot != null && itemSlot.Length > 0 && itemSlot[0].isFull)
        {
            // Устанавливает фокус EventSystem на первый предмет
            itemSlot[0].Select();

            // Запускает логику обновления UI (название, описание, шейдер)
            itemSlot[0].OnLeftClick();
        }
        else
        {
            // Если инвентарь пуст, просто очищаем фокус и описание
            DeselectAllSlots();
            ShowEmptyInventoryHelper();
        }
    }
    public void ShowEmptyInventoryHelper()
    {
        if (itemSlot != null && itemSlot.Length > 0)
        {
            // Используем ссылки из нулевого слота для изменения описания в UI
            itemSlot[0].ItemNameText.text = emptyInventoryTitle;
            itemSlot[0].ItemDescriptionText.text = emptyInventoryText;

            if (noItemsSprite != null)
            {
                itemSlot[0].itemDescriptionImage.sprite = noItemsSprite;
            }
        }
    }
    public void AddItem(ItemData item)
    {
        if (item == null) return;

        string itemName = item.itemName;
        string itemDescription = item.description;
        Sprite itemSprite = item.icon ?? defaultSprite;

        //Debug.Log("ItemName = " + itemName + "Description = " + itemDescription + "itemSprite = " + itemSprite);

        for (int i = 0; i < itemSlot.Length; i++)
        {
            if (itemSlot[i].itemName == item.itemName)
            {
                itemSlot[i].AddCount(1);
                return;
            }

            if (!itemSlot[i].isFull)
            {
                itemSlot[i].AddItem(item);
                return;
            }
        }
    }
    public void RemoveItem(ItemData item)
    {
        if (item == null) return;

        bool slotCleared = false;

        for (int i = 0; i < itemSlot.Length; i++)
        {
            if (itemSlot[i].itemName == item.itemName)
            {
                itemSlot[i].AddCount(-1);

                if (itemSlot[i].quantity <= 0)
                {
                    itemSlot[i].ClearSlot();
                    slotCleared = true;
                }
                break; // Прерываем цикл, так как предмет найден
            }
        }

        // Если слот полностью освободился, сдвигаем предметы, чтобы избежать пропусков
        if (slotCleared)
        {
            ReorganizeSlots();
        }
    }

    public void ReorganizeSlots()
    {
        // Собираем все текущие предметы и их количество
        var activeItems = new System.Collections.Generic.List<(ItemData itemData, int quantity)>();

        for (int i = 0; i < itemSlot.Length; i++)
        {
            if (itemSlot[i].isFull)
            {
                // Находим оригинальный ItemData из основного инвентаря
                ItemData data = inventory.items.Find(x => x.itemName == itemSlot[i].itemName);
                if (data != null)
                {
                    activeItems.Add((data, itemSlot[i].quantity));
                }
            }
        }

        ClearAllSlots();

        // Заполняем слоты заново подряд, перенося количество
        int newIndex = 0;
        foreach (var activeItem in activeItems)
        {
            if (newIndex < itemSlot.Length)
            {
                itemSlot[newIndex].AddItem(activeItem.itemData);

                // AddItem обычно устанавливает quantity на 1, поэтому добавляем остаток
                if (activeItem.quantity > 1)
                {
                    itemSlot[newIndex].AddCount(activeItem.quantity - 1);
                }
                newIndex++;
            }
        }
    }
    public void DeselectAllSlots()
    {
        for (int i = 0; i < itemSlot.Length; i++)
        {
            itemSlot[i].selectedShader.SetActive(false);
            itemSlot[i].thisItemSelected = false;
        }
    }

    public void ClearAllSlots()
    {
        for (int i = 0; i < itemSlot.Length; i++)
        {
            itemSlot[i].ClearSlot();
        }
    }
}
