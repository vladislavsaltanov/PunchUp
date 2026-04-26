using UnityEngine;

public class ItemPickup : MonoBehaviour, IInteractable
{
    [Header("Data")]
    [SerializeField] ItemData itemData;

    [Header("Visuals")]
    [SerializeField] GameObject promptUI;
    [SerializeField] SpriteRenderer spriteRenderer;

    [Header("Animation")]
    [SerializeField] float translationTime = 1f;
    [SerializeField] float translationSpeed = 4f;
    [SerializeField] float translationSpeedRandom = 0.5f;
    [SerializeField] float translationDistance = 1.4f;
    [SerializeField] float translationOffset = 0f;

    void Update()
    {
        float translation = Mathf.Sin(Time.unscaledTime * (translationSpeed + translationSpeedRandom) + translationOffset) * (translationDistance / 1000);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + translation, transform.localPosition.z);
    }

    private void Start()
    {
        translationOffset = Random.Range(0f, 50f);
        translationSpeedRandom = Random.Range(-0.5f, 0.5f);

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
    public void Init(ItemData data)
    {
        itemData = data;

        if (data.icon != null)
            spriteRenderer.sprite = data.icon;
    }
    public void ShowPrompt(bool show)
    {
        if (promptUI != null)
            promptUI.SetActive(show);
    }

    Color returnColor(ItemRarity rarity) => rarity switch
    {
        ItemRarity.White => Color.whiteSmoke,
        ItemRarity.Green => Color.limeGreen,
        ItemRarity.Red => Color.softRed,
        ItemRarity.Yellow => Color.lightGoldenRod,
        _ => Color.white
    };
}