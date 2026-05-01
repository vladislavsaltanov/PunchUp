using UnityEngine;

public class Vial : MonoBehaviour
{
    ushort damage;
    BaseEntity owner;

    [SerializeField] LayerMask targetLayer;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float lifetime = 5f;
    [SerializeField] Texture2D debrisTexture;

    Rigidbody2D rb;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    public void Init(Vector2 velocity, ushort dmg, BaseEntity own)
    {
        damage = dmg;
        owner = own;
        rb.linearVelocity = velocity;
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (((1 << col.gameObject.layer) & groundLayer.value) != 0)
        {
            SpawnDebris(col.contacts[0].point);
            Destroy(gameObject);
            return;
        }

        var entity = col.collider.GetComponentInParent<BaseEntity>();
        if (entity == null || entity == owner) return;

        entity.TakeDamage(damage, owner?.transform, "пробирка");
        SpawnDebris(transform.position);
        Destroy(gameObject);
    }

    void SpawnDebris(Vector3 position)
    {
        if (debrisTexture != null)
            VisualEffectsManager.SpawnDebris(debrisTexture, position);
    }
}
