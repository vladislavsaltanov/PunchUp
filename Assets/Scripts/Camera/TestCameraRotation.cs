using UnityEngine;

public class CameraRotationZone2D : MonoBehaviour
{
    private void OnTriggerExit2D(Collider2D other)
    {
        CameraManager.Instance.ResetCameraRotation();
        CameraManager.Instance.ChangeZoom(10f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CameraManager.Instance.RotateCamera(60f, 6f);
        CameraManager.Instance.ChangeZoom(5f);
    }
}