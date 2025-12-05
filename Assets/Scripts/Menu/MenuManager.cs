using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public void StartGame(int id)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(id);
    }

    public void Exit()
    {
        PlayerPrefs.Save();
        Application.Quit();
    }
}
