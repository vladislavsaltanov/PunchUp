using UnityEngine;

public class BossProjectileState : IEnemyState
{
    readonly EnemyAIBoss _boss;
    bool _active;

    public BossProjectileState(EnemyAIBoss boss) => _boss = boss;

    public void Enter()
    {
        _boss.IsVulnerable = false;
        if (_boss.hitbox != null) _boss.hitbox.gameObject.SetActive(false);
        _active = true;
        _ = Throw();
    }

    async Awaitable Throw()
    {
        await Awaitable.WaitForSecondsAsync(0.4f);
        if (!_active) return;

        if (_boss.projectilePrefab != null && _boss.Player != null)
        {
            Vector2 dir = (_boss.Player.transform.position - _boss.transform.position).normalized;
            var go = Object.Instantiate(
                _boss.projectilePrefab,
                _boss.transform.position + (Vector3)(dir * 1f),
                Quaternion.identity);

            go.GetComponent<BossProjectile>()?.Init(dir, _boss.projectileSpeed, _boss.projectileDamage);
        }

        await Awaitable.WaitForSecondsAsync(0.3f);
        if (!_active) return;

        _boss.GoToIdle();
    }

    public void Exit() => _active = false;

    public void Update()
    {
    }
}
