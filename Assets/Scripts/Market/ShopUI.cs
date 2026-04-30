using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private Button[] itemButtons;
    [SerializeField] private Image[] itemIcons;
    [SerializeField] private TMP_Text[] itemNames;
    [SerializeField] private TMP_Text[] priceTexts;
    [SerializeField] private Image[] rarityFrames;

    private ShopKeeper currentShop;
    private ShopSlotRuntime[] currentSlots;

    public void Setup(ShopKeeper shop, ShopSlotRuntime[] slots, PlayerController player)
    {
        currentShop = shop;
        currentSlots = slots;

        Refresh();
    }

    public void Refresh()
    {
        int max = Mathf.Min(currentSlots.Length, itemButtons.Length);

        for (int i = 0; i < max; i++)
        {
            var item = currentSlots[i].item;

            itemButtons[i].onClick.RemoveAllListeners();

            if (item == null)
            {
                itemButtons[i].interactable = false;
                continue;
            }

            try
            {
                itemIcons[i].sprite = item.icon;
                rarityFrames[i].color = GetRarityColor(item.rarity);
            }
            catch { }
            itemNames[i].text = item.itemName;
            priceTexts[i].text = item.price.ToString();

            int index = i;
            itemButtons[i].interactable = true;
            itemButtons[i].onClick.AddListener(() =>
            {
                currentShop.TryBuyItem(index);
            });
        }
    }

    Color GetRarityColor(ItemRarity rarity) => rarity switch
    {
        ItemRarity.White => Color.white,
        ItemRarity.Green => Color.green,
        ItemRarity.Red => Color.red,
        ItemRarity.Yellow => Color.yellow,
        _ => Color.white
    };
}
