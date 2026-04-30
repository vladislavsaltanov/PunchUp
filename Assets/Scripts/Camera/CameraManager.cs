using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraManager : MonoBehaviour
{
    [Header("Camera References")]
    [SerializeField] private CinemachineCamera defaultCamera;
    [SerializeField] public List<CinemachineCamera> allCameras = new List<CinemachineCamera>();

    public static CameraManager Instance { get; private set; }

    public CinemachineCamera currentCamera;
    private CinemachinePositionComposer composer;
    private Quaternion startRotation;
    private Quaternion targetRotation;
    private float rotationProgress;
    private float currentRotationDuration;
    private bool isrotating;
    private Coroutine rotationCoroutine;

    private float targetZoom;
    private float startZoom;
    private float zoomProgress;
    private float currentZoomDuration;
    private Coroutine zoomCoroutine;
    public GameObject player;

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
        UpdateAllCameras();
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
            StartCoroutine(RotationCoroutine(currentCamera.transform.rotation * Quaternion.Euler(0, 0, angle), duration));
        }
    }

    private IEnumerator RotationCoroutine(Quaternion target, float duration)
    {
        startRotation = currentCamera.transform.rotation;
        targetRotation = target;
        rotationProgress = 0f;
        currentRotationDuration = duration;

        while (rotationProgress < 1f)
        {
            if (currentCamera == null) yield break;

            rotationProgress += Time.deltaTime / duration;
            currentCamera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, rotationProgress);
            yield return null;
        }

        if (currentCamera != null)
        {
            currentCamera.transform.rotation = targetRotation;
        }
        isrotating = false;
        rotationCoroutine = null;
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
        if (zoomCoroutine != null)
        {
            StopCoroutine(zoomCoroutine);
        }

        zoomCoroutine = StartCoroutine(SmoothZoomCoroutine(targetZoomValue, duration));
    }

    private IEnumerator SmoothZoomCoroutine(float targetZoomValue, float duration)
    {
        startZoom = currentCamera.Lens.OrthographicSize;
        targetZoom = targetZoomValue;
        zoomProgress = 0f;
        currentZoomDuration = duration;

        while (zoomProgress < 1f)
        {
            if (currentCamera == null) yield break;

            zoomProgress += Time.deltaTime / duration;
            float currentZoom = Mathf.Lerp(startZoom, targetZoom, zoomProgress);
            currentCamera.Lens.OrthographicSize = currentZoom;
            yield return null;
        }

        if (currentCamera != null)
        {
            currentCamera.Lens.OrthographicSize = targetZoom;
        }

        zoomCoroutine = null;
    }

    public void StopCameraRotation()
    {
        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
            rotationCoroutine = null;
        }
    }

    public void StopZoomChange()
    {
        if (zoomCoroutine != null)
        {
            StopCoroutine(zoomCoroutine);
            zoomCoroutine = null;
        }
    }

    public void AddCamera(CinemachineCamera camera)
    {
        if (!allCameras.Contains(camera))
            allCameras.Add(camera);
    }

    public void RemoveCamera(CinemachineCamera camera)
    {
        allCameras.Remove(camera);
    }

    public void ChangeCamera(CinemachineCamera camera)
    {
        foreach (var cam in allCameras)
        {
            cam.Priority = 0;
        }
        camera.Priority = 100;
        currentCamera = camera;
    }

    public void UpdateAllCameras()
    {
        CinemachineCamera[] cameras = GameObject.Find("Cameras").GetComponentsInChildren<CinemachineCamera>();
        allCameras.Clear();
        foreach (var cam in cameras)
        {
            AddCamera(cam);
        }
    }

    //������ ������
    public CinemachineCamera GetCamera()
    {
        return currentCamera;
    }
}