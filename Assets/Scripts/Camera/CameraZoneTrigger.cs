using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class CameraZoneTrigger : MonoBehaviour
{

    public float gridWidth = 60f;
    public float gridHeight = 30f;
    private int curCameraID;
    private CinemachineCamera curCamera;
    private float snappedX;
    private float snappedY;
    private Transform player;

    public CameraManager cameraManager;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        snappedX = Mathf.Round(player.position.x / gridWidth);
        snappedY = Mathf.Round(player.position.y / gridHeight);
        cameraManager = GameObject.Find("CameraManager").GetComponent<CameraManager>();
        foreach (var cam in cameraManager.allCameras)
            cam.Target.TrackingTarget = player;

        curCameraID = GetCurrentCamera(snappedX, snappedY);
        curCamera = cameraManager.allCameras[curCameraID];
        cameraManager.ChangeCamera(curCamera);
    }

    private void OnTriggerEnter2D(Collider2D tr)
    {
        if (!tr.CompareTag("Player"))
            return;
        var _rb = tr.GetComponentInParent<BaseEntity>();
        _rb.ApplyVelocityOverride(_rb.GetComponent<Rigidbody2D>().linearVelocity * 2f, 0.1f);
    }

    void OnTriggerExit2D(Collider2D tr)
    {
        if (!tr.CompareTag("Player"))
            return;
        snappedX = Mathf.Clamp(Mathf.Round(tr.transform.position.x / gridWidth), -60, 60);
        snappedY = Mathf.Clamp(Mathf.Round(tr.transform.position.y / gridHeight), -30, 30);
        curCameraID = GetCurrentCamera(snappedX, snappedY);
        curCamera = cameraManager.allCameras[curCameraID];
        cameraManager.ChangeCamera(curCamera);
    }

    private static int GetCurrentCamera(float x, float y)
    {
        return (int)(4 + x + (y * 3));
    }
}
