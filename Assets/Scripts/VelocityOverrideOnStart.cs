using UnityEngine;

public class VelocityOverrideOnStart : MonoBehaviour
{
    [SerializeField] private BaseEntity _entity;
    [SerializeField] private float duration = 1f;

    void Start()
    {
        _entity.ApplyVelocityOverride(Vector2.zero, duration);
    }
}
