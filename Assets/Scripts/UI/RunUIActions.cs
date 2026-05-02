using UnityEngine;
using UnityEngine.InputSystem;

public class RunUIActions : MonoBehaviour
{
    public void StartRun()
    {
/*        if (PlayerPrefs.GetInt("tutorial_passsed", 0) == 0)
            
*/
        RunManager.Instance.StartRun();
    }
    public void Exit()
    {
        PlayerPrefs.Save();
        Application.Quit();
    }
    public void SwitchScene(int id)
    {
        Time.timeScale = 1f;
        SceneTransitionManager.SwitchScene(id, 1.5f);
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
