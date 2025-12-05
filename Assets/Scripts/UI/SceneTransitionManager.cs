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
            Instance.StartCoroutine(Instance.TransitionRoutine(sceneIndex, duration));
        }
        else
        {
            SceneManager.LoadScene(sceneIndex);
        }
    }

    private IEnumerator TransitionRoutine(int sceneIndex, float duration)
    {
        loadingScreenObject.SetActive(true);
        yield return Fade(0f, 1f, duration);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);

        while (!operation.isDone)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        yield return Fade(1f, 0f, duration);
        loadingScreenObject.SetActive(false);
    }

    private IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        float time = 0;
        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, time / duration);
            yield return null;
        }
        canvasGroup.alpha = endAlpha;
    }
}