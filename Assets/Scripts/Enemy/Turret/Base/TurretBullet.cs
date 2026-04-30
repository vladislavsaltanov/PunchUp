using UnityEngine;

public class TurretBullet : MonoBehaviour
{
    float speed;
    ushort damage;
    BaseEntity owner;
    int direction;

    [SerializeField] LayerMask targetLayer;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float lifetime = 5f;

    public void Init(int dir, float spd, ushort dmg, BaseEntity own)
    {
        direction = dir;
        speed = spd;
        damage = dmg;
        owner = own;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(Vector2.right * direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & groundLayer.value) != 0)
        {
            Destroy(gameObject);
            return;
        }

        var entity = other.GetComponentInParent<BaseEntity>();
        if (entity == null || entity == owner || !entity._name.Equals("Игрок")) return;

        entity.TakeDamage(damage, owner.transform, "турель");
        Destroy(gameObject);
    }
}
