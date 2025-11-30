using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    [Header("Camera References")]
    [SerializeField] private CinemachineCamera defaultCamera;
    [SerializeField] private List<CinemachineCamera> allCameras = new List<CinemachineCamera>();

    public static CameraManager Instance { get; private set; }

    private CinemachineCamera currentCamera;
    private CinemachinePositionComposer composer;
    private Quaternion startRotation;
    private Quaternion targetRotation;
    private float rotationProgress;
    private float currentRotationDuration;
    private bool isrotating;
    private Coroutine rotationCoroutine;

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
            SwitchToCamera(defaultCamera);
        }
        //GlobalEventHandler.Instance.GetActionByName()
    }

    public void ChangeZoom(float zoomValue)
    {
        if (currentCamera != null)
        {
            currentCamera.Lens.OrthographicSize = zoomValue;
        }
    }

    public void SwitchToCamera(CinemachineCamera targetCamera)
    {
        if (targetCamera == null) return;

        foreach (var camera in allCameras)
        {
            if (camera != null)
            {
                camera.gameObject.SetActive(false);
                camera.Priority = 0;
            }
        }

        targetCamera.gameObject.SetActive(true);
        targetCamera.Priority = 100;
        currentCamera = targetCamera;

        composer = targetCamera.GetComponent<CinemachinePositionComposer>();
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
            StopCameraRotation();
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
        rotationCoroutine = null;
    }

    public void StopCameraRotation()
    {
        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
            rotationCoroutine = null;
        }
    }
}