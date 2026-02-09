using System.Threading;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraManager : MonoBehaviour
{
    [Header("Camera References")]
    [SerializeField] private CinemachineCamera defaultCamera;

    public static CameraManager Instance { get; private set; }

    private CinemachineCamera currentCamera;
    private Quaternion startRotation;
    private Quaternion targetRotation;
    private float rotationProgress;
    private float currentRotationDuration;
    private bool isrotating;

    private float targetZoom;
    private float startZoom;
    private float zoomProgress;
    private float currentZoomDuration;
    public GameObject player;

    private CancellationTokenSource rotationCts;
    private CancellationTokenSource zoomCts;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (currentCamera == null)
        {
            currentCamera = defaultCamera;
        }
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentCamera = FindFirstObjectByType<CinemachineCamera>();
    }

    public void SetTrackingTarget(Transform target)
    {
        if (currentCamera == null)
        {
            currentCamera = FindFirstObjectByType<CinemachineCamera>();
        }

        currentCamera.Follow = target;
    }

    public void ResetCameraRotation()
    {
        if (currentCamera == null) return;

        targetRotation = Quaternion.identity;
        currentCamera.transform.rotation = targetRotation;
        isrotating = false;
    }

    public void RotateCamera(float angle, float duration = -1f)
    {
        if (currentCamera == null || isrotating) return;

        if (duration <= 0f)
        {
            currentCamera.transform.rotation *= Quaternion.Euler(0, 0, angle);
        }
        else
        {
            isrotating = true;
            _ = Rotation(currentCamera.transform.rotation * Quaternion.Euler(0, 0, angle), duration, rotationCts.Token);
        }
    }

    private async Awaitable Rotation(Quaternion target, float duration, CancellationToken token)
    {
        startRotation = currentCamera.transform.rotation;
        targetRotation = target;
        rotationProgress = 0f;
        currentRotationDuration = duration;

        while (rotationProgress < 1f)
        {
            if (currentCamera == null || token.IsCancellationRequested) return;

            rotationProgress += Time.deltaTime / duration;
            currentCamera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, rotationProgress);
            await Awaitable.NextFrameAsync();
        }

        if (currentCamera != null)
        {
            currentCamera.transform.rotation = targetRotation;
        }
        isrotating = false;
    }

    public void ChangeZoom(float zoomValue, float time = -1f)
    {
        if (currentCamera == null) return;

        StopZoomChange();

        if (time <= 0f)
        {
            currentCamera.Lens.OrthographicSize = zoomValue;
        }
        else
        {
            StartSmoothZoomChange(zoomValue, time);
        }
    }

    private void StartSmoothZoomChange(float targetZoomValue, float duration)
    {
        StopZoomChange();
        zoomCts = new CancellationTokenSource();
        _ = SmoothZoom(targetZoomValue, duration, zoomCts.Token);
    }

    private async Awaitable SmoothZoom(float targetZoomValue, float duration, CancellationToken token)
    {
        startZoom = currentCamera.Lens.OrthographicSize;
        targetZoom = targetZoomValue;
        zoomProgress = 0f;
        currentZoomDuration = duration;

        while (zoomProgress < 1f)
        {
            if (currentCamera == null || token.IsCancellationRequested) return;

            zoomProgress += Time.deltaTime / duration;
            float currentZoom = Mathf.Lerp(startZoom, targetZoom, zoomProgress);
            currentCamera.Lens.OrthographicSize = currentZoom;
            await Awaitable.NextFrameAsync();
        }

        if (currentCamera != null)
        {
            currentCamera.Lens.OrthographicSize = targetZoom;
        }
    }

    public void StopCameraRotation()
    {
        rotationCts?.Cancel();
        rotationCts?.Dispose();
        rotationCts = null;
    }

    public void StopZoomChange()
    {
        zoomCts?.Cancel();
        zoomCts?.Dispose();
        zoomCts = null;
    }

    public CinemachineCamera GetCamera()
    {
        return currentCamera;
    }
}