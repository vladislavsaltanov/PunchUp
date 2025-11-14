using UnityEngine;
using Unity.Cinemachine;

public class CameraZone : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private CinemachineCamera leftExitCamera;
    [SerializeField] private CinemachineCamera rightExitCamera;

    private void OnTriggerExit2D(Collider2D other)
    {
        
        float exitDirection = other.transform.position.x - transform.position.x;
        CinemachineCamera targetCamera = exitDirection > 0 ? rightExitCamera : leftExitCamera;

        if (targetCamera != null)
        {
            CameraManager.Instance.SwitchToCamera(targetCamera);
        }
    }
}
