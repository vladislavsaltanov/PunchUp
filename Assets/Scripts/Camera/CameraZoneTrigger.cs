using UnityEngine;
using Unity.Cinemachine;

public class CameraZone : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private CinemachineCamera Camera;
    public Transform Room = null;
    private void OnTriggerExit2D(Collider2D tr)
    {
        Room = FloorManager.Instance.pathwaysPlaceholders[FloorManager.Instance.EnterInd];
        Camera = CameraManager.Instance.GetCamera();
        float Xpos = tr.transform.position.x - Room.position.x;
        float Ypos = tr.transform.position.y - Room.position.y;
        if (Mathf.Abs(Xpos) > 28)
        {
            GetComponent<CameraManager>().MoveCameraTarget(Room.position.x + 40, Room.position.y);
            FloorManager.Instance.currentRoom.position.Set(Room.position.x + 40, Room.position.y, Room.position.z);
        }
        else
        {
            GetComponent<CameraManager>().MoveCameraTarget(Room.position.x, Room.position.y + 20);
            FloorManager.Instance.currentRoom.position.Set(Room.position.x, Room.position.y + 20, Room.position.z);
        }
    }

    public void SetStartRoom(Transform pos)
    {
       Room = pos;
    }
}
