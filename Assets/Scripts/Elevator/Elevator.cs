using UnityEngine;

public class Elevator : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    [SerializeField] int sceneBuildIndex;

    [Header("Visuals")]
    [SerializeField] GameObject promptUI;

    private void Start()
    {
        if (promptUI != null)
            promptUI.SetActive(false);

        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    public void Interact(PlayerController player)
    {
        Debug.Log($"[Elevator] Going to scene {sceneBuildIndex}...");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.SwitchScene(sceneBuildIndex);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneBuildIndex);
        }
    }

    public void ShowPrompt(bool show)
    {
        if (promptUI != null)
            promptUI.SetActive(show);
    }
}