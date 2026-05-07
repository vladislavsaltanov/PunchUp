using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossProjectile : MonoBehaviour
{
    [SerializeField] LayerMask playerLayer;
    [SerializeField] float lifetime = 5f;

    ushort _damage;

    public void Init(Vector2 dir, float speed, ushort damage)
    {
        var rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearVelocity = dir * speed;
        _damage = damage;
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & playerLayer.value) != 0)
            other.GetComponentInParent<BaseEntity>()?.TakeDamage(_damage, transform, "снаряд Тесея");

        // ignore Default layer 
        if (other.gameObject.layer != LayerMask.NameToLayer("Default"))
            Destroy(gameObject);
    }
}
