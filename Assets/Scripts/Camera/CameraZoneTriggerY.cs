using UnityEngine;
using Unity.Cinemachine;

public class CameraZoneY : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private CinemachineCamera downExitCamera;
    [SerializeField] private CinemachineCamera upExitCamera;

    private void OnTriggerExit2D(Collider2D other)
    {

        float exitDirection = other.transform.position.y - transform.position.y;
        CinemachineCamera targetCamera = exitDirection > 0 ? upExitCamera : downExitCamera;

        if (targetCamera != null)
        {
            CameraManager.Instance.SwitchToCamera(targetCamera);
        }
    }
}
