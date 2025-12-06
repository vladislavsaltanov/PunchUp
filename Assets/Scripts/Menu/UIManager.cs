using UnityEngine;
using TMPro;
using System.Collections;
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] GameObject pauseMenu;
    bool isPaused;

    [Space(10)]
    [Header("Notification System")]
    [SerializeField] GameObject notificationPanel;
    [SerializeField] TMP_Text notificationTitle;
    [SerializeField] TMP_Text notificationDesc;
    [SerializeField] float notificationDuration = 3f;
    Coroutine notificationCoroutine;


    private void Start()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex != 0)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        } else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        if (notificationPanel != null)
            notificationPanel.SetActive(false);

        if (InputManager.Instance == null)
            return;

        InputManager.Instance.pauseAction.action.performed += OnPauseButtonPressed;

    }
    public void ShowItemNotification(ItemData item)
    {
        if (notificationPanel == null) return;

        if (notificationCoroutine != null)
            StopCoroutine(notificationCoroutine);

        notificationTitle.text = item.itemName;

        if (string.IsNullOrEmpty(item.description))
        {
            notificationDesc.gameObject.SetActive(false); 
        }
        else
        {
            notificationDesc.text = item.description;
            notificationDesc.gameObject.SetActive(true);
        }

        notificationPanel.SetActive(true);
        notificationCoroutine = StartCoroutine(HideNotificationRoutine());
    }

    private IEnumerator HideNotificationRoutine()
    {
        yield return new WaitForSeconds(notificationDuration);
        notificationPanel.SetActive(false);
        notificationCoroutine = null;
    }

    private void OnPauseButtonPressed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        SwitchPause();
    }

    public void SwitchPause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (pauseMenu != null)
            pauseMenu.SetActive(isPaused);

        if (!isPaused)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
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
        SceneTransitionManager.SwitchScene(id, 1.5f);
    }

    public void Exit()
    {
        PlayerPrefs.Save();
        Application.Quit();
    }
}
