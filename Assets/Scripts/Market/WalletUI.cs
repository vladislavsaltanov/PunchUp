using UnityEngine;
using TMPro;

public class WalletUI : MonoBehaviour
{
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private PlayerWallet wallet;

    private void Start()
    {
        if (wallet == null)
            wallet = FindObjectOfType<PlayerWallet>();

        wallet.OnGoldChanged += UpdateGold;
        UpdateGold(wallet.Gold);
    }

    private void UpdateGold(int amount)
    {
        goldText.text = amount.ToString();
    }

    private void OnDestroy()
    {
        if (wallet != null)
            wallet.OnGoldChanged -= UpdateGold;
    }
}