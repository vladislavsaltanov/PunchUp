using UnityEngine;

public class ShopKeeper : MonoBehaviour, IInteractable
{
    [SerializeField] private ShopItem[] shopItems;
    [SerializeField] private GameObject promptUI;

    private void Awake()
    {
        if (promptUI != null)
            promptUI.SetActive(false);
    }

    public void Interact(PlayerController player)
    {
        player.OpenShop(this, shopItems);
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
        var wallet = player.GetComponent<PlayerWallet>();

        if (inventory == null || wallet == null)
            return;

        if (!wallet.TrySpend(shopItem.price))
            return;

        if (inventory.AddItem(shopItem.item))
        {
            shopItem.isSold = true;
            player.RefreshShopUI();
            UIManager.Instance?.ShowItemNotification(shopItem.item);
        }
    }
}