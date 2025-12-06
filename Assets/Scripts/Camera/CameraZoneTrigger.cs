using UnityEngine;
using Unity.Cinemachine;

public class CameraZoneTrigger : MonoBehaviour
{
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
    }
}
