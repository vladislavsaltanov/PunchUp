using UnityEngine;
using UnityEngine.UI;

public class PrivacyScreenManager : MonoBehaviour
{
    private const string PrivacyPolicyShownKey = "privacy_policy_shown";
    private const string AnalyticsConsentKey = "analytics_consent";

    [Header("UI")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Toggle consentToggle;

    [Header("Links")]
    [SerializeField] private string privacyPolicyUrl = "https://punchup.madebykowie.ru/privacy";
    [SerializeField] private string playtestUrl = "https://punchup.madebykowie.ru/";

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        Hide();
    }

    private void Start()
    {
        var shown = PlayerPrefs.GetInt(PrivacyPolicyShownKey, 0);

        if (shown == 0)
            Show();
        else
            Hide();
    }

    public void Show()
    {
        if (canvasGroup == null) return;

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        if (canvasGroup == null) return;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void OpenPrivacyPolicySite()
    {
        if (string.IsNullOrWhiteSpace(privacyPolicyUrl))
        {
            Debug.LogWarning("[PrivacyScreenManager] Privacy policy URL is empty.");
            return;
        }

        Application.OpenURL(privacyPolicyUrl);
    }

    public void OpenPlaytestSite()
    {
        if (string.IsNullOrWhiteSpace(playtestUrl))
        {
            Debug.LogWarning("[PrivacyScreenManager] Playtest URL is empty.");
            return;
        }

        Application.OpenURL(playtestUrl);
    }

    public void Continue()
    {
        int consent = (consentToggle != null && consentToggle.isOn) ? 1 : 0;

        PlayerPrefs.SetInt(AnalyticsConsentKey, consent);
        PlayerPrefs.SetInt(PrivacyPolicyShownKey, 1);
        PlayerPrefs.Save();

        Hide();
    }

    public static bool IsAnalyticsConsented()
    {
        return PlayerPrefs.GetInt(AnalyticsConsentKey, 0) == 1;
    }

    [ContextMenu("Reset Privacy Policy Shown")]
    public void ResetShown()
    {
        PlayerPrefs.SetInt(PrivacyPolicyShownKey, 0);
        PlayerPrefs.SetInt(AnalyticsConsentKey, 0);
        PlayerPrefs.Save();
        Show();

        if (consentToggle != null)
            consentToggle.isOn = false;
    }
}
