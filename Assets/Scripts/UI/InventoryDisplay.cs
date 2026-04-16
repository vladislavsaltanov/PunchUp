using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class InventoryDisplay : MonoBehaviour
{

    [SerializeField] List<ItemData> items = new();
    [SerializeField] GameObject itemPrefab;
    [SerializeField] Transform container;

    private Inventory inventory;
    private string itemName;
    private string quantity;
    private Sprite icon;


    void Start()
    {
        inventory = FindFirstObjectByType<Inventory>();
        GetItemsFromInventory();
    }

    void Update()
    {

    }

    public void GetItemsFromInventory()
    {
        items.Clear();
        foreach (var item in inventory.items)
        {
            if (!items.Contains(item))
            {
                items.Add(item);
                DisplayItem(item);
            }
        }
    }

    public void DisplayItem(ItemData item)
    {
        itemName = item.itemName;
        quantity = "" + inventory.GetStackCount(item);
        icon = item.icon;

        GameObject newItem = Instantiate(itemPrefab, container);
        SetupItem(newItem, itemName, quantity, icon);
    }

    private void SetupItem(GameObject item, string itemName, string quantity, Sprite icon)
    {
        if (item == null)
        {
            Debug.LogError("Container GameObject is null!");
            return;
        }

        // Находим и заполняем Icon (Image)
        Image _icon = FindChildComponent<Image>(item, "Icon");
        if (_icon != null)
        {
            _icon.sprite = icon;
        }
        else
        {
            Debug.LogWarning($"Image component with name 'Icon' not found in {item.name}");
        }

        // Находим и заполняем Name (Text)
        Text nameText = FindChildComponent<Text>(item, "Name");
        if (nameText != null)
        {
            nameText.text = itemName;
        }
        else
        {
            Debug.LogWarning($"Text component with name 'Name' not found in {item.name}");
        }

        // Находим и заполняем Quantity (Text)
        Text quantityText = FindChildComponent<Text>(item, "Quantity");
        if (quantityText != null)
        {
            quantityText.text = quantity;
        }
        else
        {
            Debug.LogWarning($"Text component with name 'Quantity' not found in {item.name}");
        }
        Debug.Log("Set Up");
    }

    private static T FindChildComponent<T>(GameObject parent, string childName) where T : Component
    {
        Transform childTransform = parent.transform.Find(childName);
        if (childTransform != null)
        {
            return childTransform.GetComponent<T>();
        }

        // Если не нашли прямым поиском, ищем рекурсивно
        foreach (Transform child in parent.transform)
        {
            T component = child.GetComponent<T>();
            if (component != null && child.name == childName)
            {
                return component;
            }

            component = FindChildComponent<T>(child.gameObject, childName);
            if (component != null)
            {
                return component;
            }
        }

        return null;
    }
}
