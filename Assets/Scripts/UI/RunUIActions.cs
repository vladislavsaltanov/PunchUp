using UnityEngine;

public class RunUIActions : MonoBehaviour
{
    public void StartRun()
    {
/*        if (PlayerPrefs.GetInt("tutorial_passsed", 0) == 0)
            
*/
        RunManager.Instance.StartRun();
    }

    public void OpenSettings()
    {
        SettingsManager.Instance.Open();
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
