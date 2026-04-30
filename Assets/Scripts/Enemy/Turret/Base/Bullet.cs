using UnityEngine;

public class Bullet : MonoBehaviour
{
    float speed;
    ushort damage;
    BaseEntity owner;
    int direction;
    string _name;

    [SerializeField] LayerMask targetLayer;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float lifetime = 5f;

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Init(int dir, float spd, ushort dmg, BaseEntity own)
    {
        direction = dir;
        speed = spd;
        damage = dmg;
        owner = own;
        _name = own._name;

        if (rb != null)
            rb.linearVelocity = Vector2.right * direction * speed;

        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & groundLayer.value) != 0)
        {
            Destroy(gameObject);
            return;
        }

        var entity = other.GetComponentInParent<BaseEntity>();
        if (entity == null || entity == owner) return;
        if (!entity._name.Equals("Игрок")) return;

        entity.TakeDamage(damage, owner?.transform, _name);
        Destroy(gameObject);
    }
}
