using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryManager : MonoBehaviour
{
    public GameObject inventoryCanvas;
    private bool isInventoryOpen;
    public ItemSlot[] itemSlot;
    private InputAction toggleAction;
    [SerializeField] private Inventory inventory;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        // Создаём действие прямо в коде
        toggleAction = new InputAction(
            "ToggleInventory",
            binding: "<Keyboard>/Tab",
            interactions: "Press"
        );
    }

    private void OnDisable()
    {
        toggleAction.Disable();
        toggleAction.performed -= OnToggle;
    }

    private void OnToggle(InputAction.CallbackContext context)
    {
        ToggleInventory();
    }

    private void ToggleInventory()
    {
        Debug.Log("ToggleInventory");
        isInventoryOpen = !isInventoryOpen;
        inventoryCanvas.SetActive(isInventoryOpen);

        Cursor.visible = isInventoryOpen;
        Cursor.lockState = isInventoryOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Time.timeScale = isInventoryOpen ? 0f : 1f;
        UpdateInventory();
    }

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

    public void AddItem(ItemData item)
    {
        if (item == null) return;

        string itemName = item.itemName;
        string itemDescription = item.description;
        Sprite itemSprite = item.icon;

        //Debug.Log("ItemName = " + itemName + "Description = " + itemDescription + "itemSprite = " + itemSprite);
        
        for (int i = 0; i < itemSlot.Length; i++)
        {
            if (!itemSlot[i].isFull)
            {
                itemSlot[i].AddItem(item);
                return;
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
