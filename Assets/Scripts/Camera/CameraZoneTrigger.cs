using UnityEngine;
using Unity.Cinemachine;

public class CameraZone : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private CinemachineCamera Camera;
    public Transform Room = null;
    private void OnTriggerEnter2D(Collider2D tr)
    {
        var _rb = tr.GetComponentInParent<PlayerMovement>().rb;
        _rb.AddForce(_rb.linearVelocity * 2f, ForceMode2D.Impulse);
    }

    private void OnTriggerExit2D(Collider2D tr)
    {
        var _rb = tr.GetComponentInParent<PlayerMovement>().rb;
        if (_rb.linearVelocityX > 0)
        {

        }
    }

    public void SetStartRoom(Transform pos)
    {
       Room = pos;
    }
}
