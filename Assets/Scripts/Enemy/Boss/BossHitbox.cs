using UnityEngine;
public class BossHitbox : MonoBehaviour
{
    [SerializeField] LayerMask playerLayer;
    [SerializeField] float damageCooldown = 0.4f;

    public ushort damage;
    float _lastHitTime;

    void OnTriggerStay2D(Collider2D other)
    {
        if (Time.time < _lastHitTime + damageCooldown) return;
        if (((1 << other.gameObject.layer) & playerLayer.value) == 0) return;

        var entity = other.GetComponentInParent<BaseEntity>();
        if (entity == null) return;

        _lastHitTime = Time.time;
        entity.TakeDamage(damage, transform, "Тесей");
    }
}
