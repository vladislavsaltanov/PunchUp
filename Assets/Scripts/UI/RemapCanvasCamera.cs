using UnityEngine;

public class RemapCanvasCamera : MonoBehaviour
{
    [SerializeField] float planeDistance = 1f;

    void OnEnable()
    {
        Canvas canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;

        canvas.worldCamera = Camera.main;
        canvas.planeDistance = planeDistance;
    }
}
