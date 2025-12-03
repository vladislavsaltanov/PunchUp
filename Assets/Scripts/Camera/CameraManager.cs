using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    [Header("Camera References")]
    [SerializeField] private CinemachineCamera defaultCamera;
    [SerializeField] private List<CinemachineCamera> allCameras = new List<CinemachineCamera>();

    [Header("Camera Target Settings")]
    [SerializeField] private Transform cameraTarget;

    public static CameraManager Instance { get; private set; }

    private CinemachineCamera currentCamera;
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

    public void MoveCameraTarget(float x, float y)
    {
        if (cameraTarget == null) return;

        Vector3 newPosition = cameraTarget.position;
        newPosition.x = x;
        newPosition.y = y;
        cameraTarget.position = newPosition;
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
}