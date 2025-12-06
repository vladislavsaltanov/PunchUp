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
        SceneTransitionManager.SwitchScene(sceneBuildIndex);
    }

    public void ShowPrompt(bool show)
    {
        if (promptUI != null)
            promptUI.SetActive(show);
    }
}
