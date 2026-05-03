using UnityEngine;

[CreateAssetMenu(fileName = "Entity Item Drop Config", menuName = "ScriptableObjects/Items/Config")]
public class EntityItemDropConfig : ScriptableObject
{
    [Header("Item Pools")]
    public ItemData[] whiteItems;
    public ItemData[] greenItems;
    public ItemData[] redItems;
    public ItemData[] yellowItems;

    [Header("Rarity Weights")]
    public float whiteWeight = 80f;
    public float greenWeight = 20f;
    public float redWeight = 0.5f;
    public float yellowWeight = 0.1f;

    public ItemData Roll(float luckMultiplier = 1f)
    {
        ItemData[] pool = RollPool(luckMultiplier);
        if (pool == null || pool.Length == 0) return null;

        return pool[Random.Range(0, pool.Length)];
    }

    ItemData[] RollPool(float luckMultiplier)
    {
        float wW = whiteWeight;
        float wG = greenWeight;
        float wR = redWeight * luckMultiplier;
        float wY = yellowWeight * luckMultiplier;
        float total = wW + wG + wR + wY;

        float roll = Random.Range(0f, total);

        if (roll < wW) return whiteItems;
        if (roll < wW + wG) return greenItems;
        if (roll < wW + wG + wR) return redItems;
        return yellowItems;
    }
}
