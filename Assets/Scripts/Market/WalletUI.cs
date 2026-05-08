using UnityEngine;
using TMPro;

public class WalletUI : MonoBehaviour
{
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private PlayerWallet wallet;
    [Header("Shake Settings")]
    [SerializeField] private float shakeDuration = 1f;
    [SerializeField] private float shakeMagnitude = 10f;

    private bool isShaking = false;

    private void Start()
    {
        if (wallet == null)
            wallet = FindObjectOfType<PlayerWallet>();

        wallet.OnGoldChanged += UpdateGold;
        wallet.OnSpendFailed += TriggerShake;
        UpdateGold(wallet.Gold);
    }

    private void UpdateGold(int amount)
    {
        goldText.text = amount.ToString();
    }

    private async void TriggerShake()
    {
        if (isShaking) return;
        await ShakeRoutine();
    }

    private async Awaitable ShakeRoutine()
    {
        isShaking = true;
        float elapsed = 0f;
        Vector3 originalPosition = goldText.rectTransform.anchoredPosition;

        try
        {
            while (elapsed < shakeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                Vector3 randomOffset = UnityEngine.Random.insideUnitCircle * shakeMagnitude;
                goldText.rectTransform.anchoredPosition = originalPosition + randomOffset;

                await Awaitable.NextFrameAsync(destroyCancellationToken);
            }
        }
        finally
        {
            if (goldText != null)
            {
                goldText.rectTransform.anchoredPosition = originalPosition;
            }
            isShaking = false;
        }
    }

    private void OnDestroy()
    {
        if (wallet != null)
        {
            wallet.OnGoldChanged -= UpdateGold;
            wallet.OnSpendFailed -= TriggerShake;
        }
    }
}
