using System.Threading;
using TMPro;
using UnityEngine;

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

    CancellationTokenSource notificationCts;

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

        notificationCts?.Cancel();
        notificationCts?.Dispose();

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
        notificationCts = new CancellationTokenSource();
        _ = HideNotificationRoutine(notificationCts.Token);
    }

    private async Awaitable HideNotificationRoutine(CancellationToken token)
    {
        await Awaitable.WaitForSecondsAsync(notificationDuration, token);
        if (token.IsCancellationRequested) return;

        notificationPanel.SetActive(false);
        notificationCts = null;
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
        notificationCts?.Cancel();
        notificationCts?.Dispose();
        notificationCts = null;

        if (InputManager.Instance == null)
            return;

        InputManager.Instance.pauseAction.action.performed -= OnPauseButtonPressed;
    }

    public void SwitchScene(int id)
    {
        Time.timeScale = 1f;
        SceneTransitionManager.SwitchScene(id, 1.5f);
    }

    public async void Surrender()
    {
        // Unpause first so timers/awaitables/etc. don't get stuck on Time.timeScale = 0.
        if (isPaused)
            SwitchPause();

        var player = PlayerController.instance;
        if (player == null)
        {
            SwitchScene(0);
            return;
        }

        // Убиваем игрока "легальным" путем, чтобы отработал OnDeath().
        // cause важно: PlayerController.OnDeath() прочитает lastDamageCause = "surrender".
        player.TakeDamage(ushort.MaxValue, null, "surrender");

        // На всякий случай даем кадр, чтобы OnDeath успел стартовать EndRun.
        await Awaitable.NextFrameAsync();
    }

    public void Exit()
    {
        PlayerPrefs.Save();
        Application.Quit();
    }
}
