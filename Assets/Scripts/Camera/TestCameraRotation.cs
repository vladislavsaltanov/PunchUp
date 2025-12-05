using UnityEngine;

public class CameraRotationZone2D : MonoBehaviour
{
    private void OnTriggerExit2D(Collider2D other)
    {
        CameraManager.Instance.RotateCamera(-8f, 0.5f);
        CameraManager.Instance.ChangeZoom(10f, 0.5f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CameraManager.Instance.RotateCamera(8f, 0.05f);
        CameraManager.Instance.ChangeZoom(9f, 0.05f);

    }
}