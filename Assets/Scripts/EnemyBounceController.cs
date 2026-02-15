using UnityEngine;

public class EnemyBounce : MonoBehaviour
{
    [SerializeField] private PhysicsMaterial2D _bouncyMaterial;
    [SerializeField] private float _bouncyTime = 0.1f;

    private PhysicsMaterial2D _originalSquareMaterial;
    private GameObject _currentPlayerSquare;
    private float _bouncyTimer = 0f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && IsPlayerOnTop(collision))
        {
            GameObject player = collision.gameObject;
            Transform squareTransform = player.transform.Find("Square");
            _currentPlayerSquare = squareTransform.gameObject;

            if (_currentPlayerSquare != null)
            {
                ApplyBouncyToSquare(_currentPlayerSquare);
                _bouncyTimer = _bouncyTime;
            }
        }
    }

    private void Update()
    {
        if (_bouncyTimer > 0)
        {
            _bouncyTimer -= Time.deltaTime;
            if (_bouncyTimer <= 0 && _currentPlayerSquare != null)
            {
                RestoreSquareMaterial();
            }
        }
    }

    private bool IsPlayerOnTop(Collision2D collision)
    {
        GameObject player = collision.gameObject.transform.Find("Square").gameObject;

        Bounds enemyBounds = GetComponent<Collider2D>().bounds;
        Bounds playerBounds = player.GetComponent<Collider2D>().bounds;

        float enemyTop = enemyBounds.max.y;
        float playerBottom = playerBounds.min.y;
        float playerCenterY = playerBounds.center.y;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.point.y > enemyBounds.center.y)
            {
                if (playerCenterY > enemyBounds.center.y)
                {
                    float verticalDistance = playerBottom - enemyTop;
                    if (verticalDistance > -0.2f && verticalDistance < 0.5f)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private void ApplyBouncyToSquare(GameObject square)
    {
        Collider2D squareCollider = square.GetComponent<Collider2D>();
        if (squareCollider == null) return;

        if (squareCollider.sharedMaterial != null)
        {
            _originalSquareMaterial = squareCollider.sharedMaterial;
        }

        if (_bouncyMaterial != null)
        {
            squareCollider.sharedMaterial = _bouncyMaterial;
        }
    }

    private void RestoreSquareMaterial()
    {
        if (_currentPlayerSquare == null) return;

        Collider2D squareCollider = _currentPlayerSquare.GetComponent<Collider2D>();
        if (squareCollider != null && _originalSquareMaterial != null)
        {
            squareCollider.sharedMaterial = _originalSquareMaterial;
        }

        _currentPlayerSquare = null;
    }
}