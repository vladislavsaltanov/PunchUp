using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    bool isPaused;

    private void Start()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex != 0)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        } else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        if (InputManager.Instance == null)
            return;

        InputManager.Instance.pauseAction.action.performed += OnPauseButtonPressed;
    }

    private void OnPauseButtonPressed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        SwitchPause();
    }

    public void SwitchPause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        pauseMenu.SetActive(isPaused);
    }

    private void OnDisable()
    {
        if (InputManager.Instance == null)
            return;

        InputManager.Instance.pauseAction.action.performed -= OnPauseButtonPressed;
    }

    public void SwitchScene(int id)
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(id);
    }

    public void Exit()
    {
        PlayerPrefs.Save();
        Application.Quit();
    }
}
