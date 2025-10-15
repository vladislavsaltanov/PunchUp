using System;
using UnityEngine;

public class isGroundedHandler : MonoBehaviour
{
    public static isGroundedHandler Instance { get; private set; }
    private void Awake() =>
        Instance = this;

    public bool isGrounded = false;
    public event Action<bool, float> hasGrounded;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isGrounded && collision.CompareTag("Ground"))
        {
            hasGrounded?.Invoke(true, PlayerController.instance.currentTime);
            isGrounded = true;
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (isGrounded && collision.CompareTag("Ground"))
        {
            hasGrounded?.Invoke(false, PlayerController.instance.currentTime);
            isGrounded = false;
        }
    }
}
