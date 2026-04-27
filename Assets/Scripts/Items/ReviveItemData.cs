using UnityEngine;

[CreateAssetMenu(menuName = "Items/Revive Item")]
public class ReviveItemData : ItemData
{
    [Range(0f, 1f)] public float healPercent = 0.5f;
    public GameObject reviveParticlePrefab;

    public void Revive(BaseEntity entity, Inventory inventory)
    {
        entity.SetHealth((ushort)(entity.Stats[StatType.MaxHealth] * healPercent));

        if (reviveParticlePrefab != null)
            Object.Instantiate(reviveParticlePrefab, entity.transform.position, Quaternion.identity);

        UIManager.Instance.ShowItemNotification(this);

        inventory.RemoveItem(this);
    }
}
