using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBarUIManager : MonoBehaviour
{
    public static PlayerHealthBarUIManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    [SerializeField] private Image fillImage;

    public void UpdateHealth(float current, float max)
    {
        fillImage.fillAmount = current / max;
    }
}
