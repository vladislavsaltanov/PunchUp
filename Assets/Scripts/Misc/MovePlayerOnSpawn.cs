using UnityEngine;

public class MovePlayerOnSpawn : MonoBehaviour
{
    bool moved;
    [SerializeField] Transform moveTo;

    void Update()
    {
        if (PlayerController.instance != null && !moved)
        {
            PlayerController.instance.transform.position = moveTo.position;
            moved = true;
        }
    }
}
