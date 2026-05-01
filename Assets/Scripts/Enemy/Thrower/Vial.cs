using UnityEngine;

public class Vial : MonoBehaviour
{
    ushort damage;
    BaseEntity owner;

    [SerializeField] LayerMask targetLayer;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float lifetime = 5f;

    [SerializeField] Rigidbody2D rb;

    public void Init(Vector2 velocity, ushort dmg, BaseEntity own)
    {
        damage = dmg;
        owner = own;
        rb.linearVelocity = velocity;
        Destroy(gameObject, lifetime);
        rb.angularVelocity = Random.Range(-300f, 300f);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (((1 << col.gameObject.layer) & groundLayer.value) != 0)
        {
            SpawnDebris(col.contacts[0].point);
            Destroy(gameObject);
            return;
        }

        try
        {
            var entity = col.collider.GetComponentInParent<BaseEntity>();
            if (entity == null || entity == owner) return;

            entity.TakeDamage(damage, owner?.transform, owner._name);
        }
        catch
        {
            SpawnDebris(transform.position);
            Destroy(gameObject);
        }
    }

    void SpawnDebris(Vector3 position)
    {
        VisualEffectsManager.SpawnDebris(owner.spriteRenderer.sprite.texture, position, 5, 0.5f);
    }
}
