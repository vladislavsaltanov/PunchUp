using UnityEngine;

public class RunUIActions : MonoBehaviour
{
    public void StartRun()
    {
        RunManager.Instance.StartRun();
    }

    public void RestartRun()
    {
        RunManager.Instance.RestartRun();
    }

    public void Surrender()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.Surrender();
    }
}