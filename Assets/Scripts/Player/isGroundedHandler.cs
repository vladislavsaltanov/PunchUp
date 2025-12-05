using System;
using UnityEngine;

public class isGroundedHandler : MonoBehaviour
{
    public static isGroundedHandler Instance { get; private set; }
    private void Awake() =>
        Instance = this;

    public event Action<bool, float> hasGrounded;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Collider2D _collider;
    public bool IsGrounded { get; private set; }
    [SerializeField] float checkDistance = 0.05f;

    private void Update()
    {
        bool currentGrounded = Physics2D.BoxCast
        (
            _collider.bounds.center,
            _collider.bounds.size,
            0f,
            Vector2.down,
            checkDistance,
            groundLayer
        );

        if (currentGrounded != IsGrounded)
        {
            IsGrounded = currentGrounded;
            hasGrounded?.Invoke(IsGrounded, PlayerController.instance.currentTime);
        }
    }
}
