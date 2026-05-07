using UnityEngine;

public class RemapCanvasCamera : MonoBehaviour
{
    [SerializeField] float planeDistance = 1f;
    bool remapped;

    void OnEnable()
    {
        Remap();
    }

    private void Start()
    {
        Remap();
    }

    void Remap()
    {
        if (remapped) return;

        Canvas canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;

        canvas.worldCamera = Camera.main;
        canvas.planeDistance = planeDistance;

        remapped = true;

    }
}
