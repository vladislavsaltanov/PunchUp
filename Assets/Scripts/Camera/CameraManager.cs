using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    [Header("Camera References")]
    [SerializeField] private CinemachineCamera defaultCamera;
    [SerializeField] private List<CinemachineCamera> allCameras = new List<CinemachineCamera>();

    [Header("Zoom Settings")]
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 15f;

    public static CameraManager Instance { get; private set; }

    private CinemachineCamera currentCamera;
    private CinemachinePositionComposer composer;

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
    }

    private void ChangeZoom(float zoomValue)
    {
        float clampedZoom = Mathf.Clamp(zoomValue, minZoom, maxZoom);

        if (composer != null)
        {
            composer.CameraDistance = clampedZoom;
        }
    }

    public void SwitchToCamera(CinemachineCamera targetCamera)
    {

        if (!allCameras.Contains(targetCamera))
        {
            allCameras.Add(targetCamera);
        }

        foreach (var camera in allCameras)
        {
            camera.Priority = 0;
        }

        targetCamera.Priority = 100;
        currentCamera = targetCamera;
    }

    public void SwitchToCamera(string cameraName)
    {
        CinemachineCamera targetCamera = allCameras.Find(cam => cam.name == cameraName);

        if (targetCamera != null)
        {
            SwitchToCamera(targetCamera);
        }
    }
}