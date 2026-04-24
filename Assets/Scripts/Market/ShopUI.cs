using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private Button[] itemButtons;
    [SerializeField] private Image[] itemIcons;
    [SerializeField] private Text[] itemTexts;
    [SerializeField] private Text[] priceTexts;

    private ShopKeeper currentShop;
    private ShopItem[] currentItems;
    private PlayerController currentPlayer;

    public void Setup(ShopKeeper shop, ShopItem[] items, PlayerController player)
    {
        currentShop = shop;
        currentItems = items;
        currentPlayer = player;

        Refresh();
    }

    public void Refresh()
    {
        for (int i = 0; i < itemButtons.Length; i++)
        {
            if (i >= currentItems.Length)
                continue;

            var shopItem = currentItems[i];

            itemIcons[i].sprite = shopItem.item.icon;
            priceTexts[i].text = shopItem.price.ToString();
            itemButtons[i].onClick.RemoveAllListeners();
            if (shopItem.isSold)
            {
                itemTexts[i].text = shopItem.item.itemName + " (Sold)";
                itemButtons[i].interactable = false;
            }
            else
            {
                int index = i;
                itemTexts[i].text = shopItem.item.itemName;
                itemButtons[i].interactable = true;

                itemButtons[i].onClick.AddListener(() =>
                {
                    currentShop.TryBuyItem(index, currentPlayer);
                });
            }
        }
    }
}