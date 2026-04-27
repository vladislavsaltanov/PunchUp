using UnityEngine;

public class ItemPickup : MonoBehaviour, IInteractable
{
    [Header("Data")]
    [SerializeField] ItemData itemData;

    [Header("Visuals")]
    [SerializeField] GameObject promptUI;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] ParticleSystem commonGlow;
    [SerializeField] ParticleSystem rareGlow;

    [Header("Animation")]
    [SerializeField] float translationTime = 1f;
    [SerializeField] float translationSpeed = 4f;
    [SerializeField] float translationSpeedRandom = 0.5f;
    [SerializeField] float translationDistance = 1.4f;
    [SerializeField] float translationOffset = 0f;

    bool isDropping = true;
    float dropTarget;

    void Update()
    {
        if (isDropping)
        {
            transform.position = Vector3.MoveTowards(transform.position,
                new Vector3(transform.position.x, dropTarget, transform.position.z),
                5f * Time.deltaTime);

            if (Mathf.Abs(transform.position.y - dropTarget) < 0.05f)
            {
                transform.position = new Vector3(transform.position.x, dropTarget, transform.position.z);
                isDropping = false;
            }
            return;
        }

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

        var hit = Physics2D.Raycast(transform.position, Vector2.down, 50f, LayerMask.GetMask("Ground"));
        if (hit.collider != null)
            dropTarget = hit.point.y + 1f;
        else
            dropTarget = transform.position.y - Random.Range(1f, 2f);
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
        bool isRare = data.rarity == ItemRarity.Red || data.rarity == ItemRarity.Yellow;

        if (commonGlow != null) commonGlow.gameObject.SetActive(!isRare);
        if (rareGlow != null) rareGlow.gameObject.SetActive(isRare);

        var glow = isRare ? rareGlow : commonGlow;
        if (glow != null)
        {
            var main = glow.main;
            main.startColor = RarityColor(data.rarity);
        }

        if (data.icon != null)
            spriteRenderer.sprite = data.icon;
    }
    public void ShowPrompt(bool show)
    {
        if (promptUI != null)
            promptUI.SetActive(show);
    }

    Color RarityColor(ItemRarity rarity) => rarity switch
    {
        ItemRarity.White => Color.whiteSmoke,
        ItemRarity.Green => Color.limeGreen,
        ItemRarity.Red => Color.softRed,
        ItemRarity.Yellow => Color.lightGoldenRod,
        _ => Color.white
    };
}
