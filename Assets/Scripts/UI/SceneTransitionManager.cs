using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject loadingScreenObject;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (loadingScreenObject != null)
            {
                loadingScreenObject.SetActive(false);
                canvasGroup.alpha = 0;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void SwitchScene(int sceneIndex, float duration = 1f)
    {
        if (Instance != null)
        {
            _ = Instance.TransitionRoutine(sceneIndex, duration);
        }
        else
        {
            SceneManager.LoadScene(sceneIndex);
        }
    }

    private async Awaitable TransitionRoutine(int sceneIndex, float duration)
    {
        loadingScreenObject.SetActive(true);
        await Fade(0f, 1f, duration);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);

        while (!operation.isDone)
        {
            await Awaitable.NextFrameAsync();
        }

        await Awaitable.WaitForSecondsAsync(0.5f);

        await Fade(1f, 0f, duration);
        loadingScreenObject.SetActive(false);
    }

    private async Awaitable Fade(float startAlpha, float endAlpha, float duration)
    {
        float time = 0;
        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, time / duration);
            await Awaitable.NextFrameAsync();
        }
        canvasGroup.alpha = endAlpha;
    }

    public void ReloadCurrentScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SwitchScene(currentSceneIndex);
    }
}