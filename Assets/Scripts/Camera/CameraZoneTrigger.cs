using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class CameraZoneTrigger : MonoBehaviour
{
    /*
    Transform cameraBounds;

    public float gridWidth = 40f;
    public float gridHeight = 20f;

    private void Start()
    {
        cameraBounds = GameObject.FindGameObjectWithTag("CameraBounds").transform;
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

        float snappedX = Mathf.Round(tr.transform.position.x / gridWidth) * gridWidth;
        float snappedY = Mathf.Round(tr.transform.position.y / gridHeight) * gridHeight;
        snappedX = Mathf.Clamp(snappedX, -40f, 40f);
        snappedY = Mathf.Clamp(snappedY, -20f, 20f);

        cameraBounds.position = new Vector3(snappedX, snappedY, cameraBounds.position.z); 
    }*/

    public float gridWidth = 40f;
    public float gridHeight = 20f;
    public int curCameraID;
    public CinemachineCamera curCamera;
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
        Debug.Log(curCameraID);
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
        snappedX = Mathf.Clamp(Mathf.Round(tr.transform.position.x / gridWidth), -40, 40);
        snappedY = Mathf.Clamp(Mathf.Round(tr.transform.position.y / gridHeight), -20, 20);
        curCameraID = GetCurrentCamera(snappedX, snappedY);
        curCamera = cameraManager.allCameras[curCameraID];
        cameraManager.ChangeCamera(curCamera);
    }

    private static int GetCurrentCamera(float x, float y)
    {
        return (int)(4 + x + (y * 3));
    }
}
