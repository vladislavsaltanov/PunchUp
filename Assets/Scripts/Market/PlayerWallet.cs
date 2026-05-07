using UnityEngine;
using System;

public class PlayerWallet : MonoBehaviour
{
    public static PlayerWallet Instance { get; private set; }

    [SerializeField] private int startingGold = 0;

    public int Gold { get; private set; }

    public event Action<int> OnGoldChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

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
        {

            return false;
        }    

        Gold -= amount;
        OnGoldChanged?.Invoke(Gold);
        return true;
    }
}
