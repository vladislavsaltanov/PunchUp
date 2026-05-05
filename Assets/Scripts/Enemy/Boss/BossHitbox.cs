using System.Collections.Generic;
using UnityEngine;

public class BossHitbox : MonoBehaviour
{
    [SerializeField] LayerMask playerBodyLayer;
    [SerializeField] LayerMask playerAttackLayer;

    public ushort damage;

    EnemyAIBoss _boss;

    readonly HashSet<Collider2D> _overlappingBodies = new HashSet<Collider2D>();

    public void Init(EnemyAIBoss boss) => _boss = boss;

    void OnDisable()
    {
        _overlappingBodies.Clear();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!_boss.CanDealDamage) return;

        int layer = 1 << other.gameObject.layer;

        if ((layer & playerAttackLayer.value) != 0)
        {
            _boss.GoToVulnerable();
            return;
        }

        if ((layer & playerBodyLayer.value) != 0)
        {
            _overlappingBodies.Add(other);
            _ = HandleCollisionWithGracePeriod(other);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        _overlappingBodies.Remove(other);
    }

    async Awaitable HandleCollisionWithGracePeriod(Collider2D other)
    {
        float graceTime = _boss.GetCurrentGracePeriod();
        float elapsed = 0f;

        while (elapsed < graceTime && _boss.CanDealDamage && _overlappingBodies.Contains(other))
        {
            elapsed += Time.deltaTime;
            await Awaitable.NextFrameAsync();
        }

        if (_boss != null && _boss.CanDealDamage && _overlappingBodies.Contains(other))
        {
            var entity = other.GetComponentInParent<BaseEntity>();
            if (entity != null && entity.CurrentHealth > 0)
            {
                entity.TakeDamage(damage, transform, "Тесей");
                _boss.OnPlayerHitByAttack();
            }
        }
    }
}
