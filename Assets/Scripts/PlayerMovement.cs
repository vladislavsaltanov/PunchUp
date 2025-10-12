using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
        MovementCharacteristics chars;

    [SerializeField]
        Rigidbody2D rb;

    private void Start()
    {
        InputManager.Instance.jumpAction.action.performed += Jump;
    }

    private void Update()
    {
        if (Mathf.Abs(InputManager.Instance.moveAction.action.ReadValue<Vector2>().x) < 0.1f)
            rb.linearVelocityX = Mathf.Lerp(rb.linearVelocityX, 0f, chars.resetSpeedTime * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (Mathf.Abs(InputManager.Instance.moveAction.action.ReadValue<Vector2>().x) > 0)
            rb.linearVelocityX = InputManager.Instance.moveAction.action.ReadValue<Vector2>().x * chars.speed;
    }

    void Jump(InputAction.CallbackContext callback)
    {
        if (callback.performed)
        {
            rb.linearVelocityY = 0;
            rb.AddForce(Vector2.up * chars.jumpForce, ForceMode2D.Impulse);
        }
    }
}
[System.Serializable]
public class MovementCharacteristics
{
    public float speed;
    public float jumpForce;
    public float resetSpeedTime;
}