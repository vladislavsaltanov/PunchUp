using UnityEngine;

public class ItemPickup : MonoBehaviour, IInteractable
{
    [Header("Data")]
    [SerializeField] ItemData itemData;

    [Header("Visuals")]
    [SerializeField] GameObject promptUI;

    private void Start()
    {
        if (promptUI != null)
            promptUI.SetActive(false);

        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    public void Interact(PlayerController player)
    {
        var inventory = player.GetComponent<Inventory>();

        if (inventory != null)
        {
            if (inventory.AddItem(itemData))
            {
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowItemNotification(itemData);
                }
                Destroy(gameObject);
            }
        }
    }

    public void ShowPrompt(bool show)
    {
        if (promptUI != null)
            promptUI.SetActive(show);
    }
}