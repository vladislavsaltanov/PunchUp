using UnityEngine;

public class EntityItemDropController : MonoBehaviour
{
    [SerializeField] EntityItemDropConfig dropConfig;
    [SerializeField] GameObject itemPickupPrefab;

    [Range(0f, 1f)]
    [SerializeField] float dropChance = 0.6f;

    BaseEntity entity;

    void Start()
    {
        entity = GetComponent<BaseEntity>();
        if (entity != null)
            entity.OnDeathEvent += TryDrop;
    }

    void OnDestroy()
    {
        if (entity != null)
            entity.OnDeathEvent -= TryDrop;
    }

    public void TryDrop()
    {
        if (dropConfig == null) return;
        if (Random.value > dropChance) return;

        float luck = entity != null ? entity.Stats[StatType.LuckStat] : 0f;
        float luckMultiplier = 1f + luck / 100f;

        ItemData item = dropConfig.Roll(luckMultiplier);
        if (item == null) return;

        var go = Instantiate(itemPickupPrefab, transform.position, Quaternion.identity);
        go.GetComponent<ItemPickup>().Init(item);
    }
}
