using UnityEngine;
using Unity.Cinemachine;

public class CameraZone : MonoBehaviour
{
    [Header("Confiner Settings")]
    [SerializeField] private BoxCollider2D targetConfiner;

    private void OnTriggerExit2D(Collider2D tr)
    {
        Debug.Log(tr.name);
        targetConfiner.transform.position += new Vector3(20, 0, 0);
    }
}
