using UnityEngine;

public class ShopKeeper : MonoBehaviour, IInteractable
{
    [SerializeField] private EntityItemDropConfig dropConfig;
    [SerializeField] private GameObject itemPickupPrefab;
    [SerializeField] private int shopSize = 3;
    [SerializeField] private GameObject promptUI;

    private ShopSlotRuntime[] currentSlots;

    private void Awake()
    {
        if (promptUI != null)
            promptUI.SetActive(false);

        GenerateShop();
    }

    void GenerateShop()
    {
        currentSlots = new ShopSlotRuntime[shopSize];

        for (int i = 0; i < shopSize; i++)
        {
            currentSlots[i] = new ShopSlotRuntime
            {
                item = dropConfig.Roll()
            };
        }
    }

    public void Interact(PlayerController player)
    {
        player.OpenShop(this, currentSlots);
    }

    public void ShowPrompt(bool show)
    {
        if (promptUI != null)
            promptUI.SetActive(show);
    }

    public void TryBuyItem(int index)
    {
        if (index < 0 || index >= currentSlots.Length)
            return;

        var slot = currentSlots[index];
        if (slot.item == null)
            return;

        var wallet = PlayerWallet.Instance;
        if (!wallet.TrySpend(slot.item.price))
            return;

        Vector3 spawnPos = PlayerController.instance.transform.position + Vector3.up * 0.5f;

        var go = Instantiate(itemPickupPrefab, spawnPos, Quaternion.identity);
        go.GetComponent<ItemPickup>().Init(slot.item);

        // Обновляем слот новым предметом
        slot.item = dropConfig.Roll();

        PlayerController.instance.RefreshShopUI();
    }
}
