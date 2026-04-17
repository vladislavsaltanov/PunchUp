using UnityEngine;

public class ShopKeeper : MonoBehaviour, IInteractable
{
    [Header("Shop Items (3 items)")]
    [SerializeField] private ShopItem[] shopItems;

    [Header("Prompt UI")]
    [SerializeField] private GameObject promptUI;

    private ShopUI shopUI;

    private void Awake()
    {
        shopUI = FindObjectOfType<ShopUI>();

        if (promptUI != null)
            promptUI.SetActive(false);
    }

    public void Interact(PlayerController player)
    {
        if (shopUI != null)
        {
            shopUI.OpenShop(this, shopItems, player);
        }
    }

    public void ShowPrompt(bool show)
    {
        if (promptUI != null)
            promptUI.SetActive(show);
    }

    public void TryBuyItem(int index, PlayerController player)
    {
        if (index < 0 || index >= shopItems.Length)
            return;

        var shopItem = shopItems[index];

        if (shopItem.isSold)
            return;

        var inventory = player.GetComponent<Inventory>();

        if (inventory == null)
            return;

        bool success = inventory.AddItem(shopItem.item);

        if (success)
        {
            shopItem.isSold = true;
            shopUI.RefreshUI();

            UIManager.Instance?.ShowItemNotification(shopItem.item);
        }
    }
}