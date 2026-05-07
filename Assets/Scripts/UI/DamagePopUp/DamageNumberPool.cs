using System.Collections.Generic;
using UnityEngine;

public class DamageNumberPool : MonoBehaviour
{
    [SerializeField] private GameObject textPrefab;
    [SerializeField] private int initialPoolSize = 30;

    private static DamageNumberPool Instance;
    private Queue<DamageText> pool = new Queue<DamageText>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewObject();
        }

        DontDestroyOnLoad(gameObject);
    }

    private DamageText CreateNewObject()
    {
        GameObject obj = Instantiate(textPrefab, transform);
        obj.SetActive(false);
        DamageText dt = obj.GetComponentInChildren<DamageText>();
        pool.Enqueue(dt);
        return dt;
    }
    public static void ShowDamage(Vector3 position, string amount, Color color)
    {
        if (Instance == null)
        {
            Debug.LogError("DamageNumberPool не найден на сцене!");
            return;
        }
        Instance.InternalShow(position, amount, color);
    }

    private async void InternalShow(Vector3 position, string amount, Color color)
    {
        DamageText dt = pool.Count > 0 ? pool.Dequeue() : CreateNewObject();

        dt.transform.position = position;
        dt.gameObject.SetActive(true);

        await dt.AnimateAndDisable(amount, color, destroyCancellationToken);

        pool.Enqueue(dt);
    }
}
