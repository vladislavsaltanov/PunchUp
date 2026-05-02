using UnityEngine;

public class PositionFixer : MonoBehaviour
{
    [SerializeField] Vector3 desired;

    // lol
    void Update() => transform.position = desired;
}
