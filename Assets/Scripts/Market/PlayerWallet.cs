using UnityEngine;
using System;

public class PlayerWallet : MonoBehaviour
{
    [SerializeField] private int startingGold = 0;

    public int Gold { get; private set; }

    public event Action<int> OnGoldChanged;

    private void Awake()
    {
        Gold = startingGold;
        OnGoldChanged?.Invoke(Gold);
    }

    public void AddGold(int amount)
    {
        if (amount <= 0) return;

        Gold += amount;
        OnGoldChanged?.Invoke(Gold);
    }

    public bool TrySpend(int amount)
    {
        if (amount <= 0) return true;

        if (Gold < amount)
            return false;

        Gold -= amount;
        OnGoldChanged?.Invoke(Gold);
        return true;
    }

    public void SetGold(int amount)
    {
        Gold = Mathf.Max(0, amount);
        OnGoldChanged?.Invoke(Gold);
    }
}
