using UnityEngine;

public class PlayerLedgeClimb : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float headroomRayLength = 0.2f;
    [SerializeField] float headroomRayOffset = 1f;
    [SerializeField] float wallDetectRayLength = 0.2f;
    [SerializeField] float wallDetectRayOffset = 0.8f;
    [SerializeField] Collider2D _collider;

    [SerializeField] PlayerController playerController;

    #region etc
    public bool canHang;
    Bounds _bounds;
    RaycastHit2D[] wallRayHits = new RaycastHit2D[1];
    RaycastHit2D[] headroomRayHits = new RaycastHit2D[1];
    int headroomDetectCount = 0;
    int wallDetectCount = 0;
    #endregion

    private void Update()
    {
        _bounds = _collider.bounds;

        Debug.DrawRay(new Vector2(playerController.direction == -1 ? _bounds.min.x : _bounds.max.x, _bounds.center.y + headroomRayOffset), Vector2.right * playerController.direction * headroomRayLength, Color.red);
        Debug.DrawRay(new Vector2(playerController.direction == -1 ? _bounds.min.x : _bounds.max.x, _bounds.center.y + wallDetectRayOffset), Vector2.right * playerController.direction * wallDetectRayLength, Color.blue);

        headroomDetectCount = Physics2D.RaycastNonAlloc(
            new Vector2(playerController.direction == -1 ? _bounds.min.x : _bounds.max.x,
                        _bounds.center.y + headroomRayOffset), 
            Vector2.right * playerController.direction, headroomRayHits, headroomRayLength, groundLayer);

        wallDetectCount = Physics2D.RaycastNonAlloc(
            new Vector2(playerController.direction == -1 ? _bounds.min.x : _bounds.max.x, 
                        _bounds.center.y + wallDetectRayOffset), 
            Vector2.right * playerController.direction, wallRayHits, wallDetectRayLength, groundLayer);
        
        canHang = headroomDetectCount == 0 && wallDetectCount == 1;
        
        if (canHang)
            Debug.Log("Can hang!");
    }
}
// TODO
// climb up while
// wait until bounds.min.y >= last ray hit y at jump key press event