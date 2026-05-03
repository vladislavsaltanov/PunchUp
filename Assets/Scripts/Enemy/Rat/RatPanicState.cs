using UnityEngine;

public class RatPanicState : IEnemyState
{
    readonly EnemyAIRat _rat;
    float _jumpTimer;
    static readonly Collider2D[] _hitBuffer = new Collider2D[4];

    public RatPanicState(EnemyAIRat rat) => _rat = rat;

    public void Enter() => _jumpTimer = 0f;

    public void Update()
    {
        if (_rat.IsGrounded && _rat.IsWallAhead(_rat.direction))
            _rat.direction *= -1;

        _jumpTimer -= Time.deltaTime;
        if (_jumpTimer <= 0f && _rat.IsGrounded)
        {
            _jumpTimer = _rat.jumpInterval / _rat.panicSpeedMultiplier;
            TryJump();
        }

        // урон при контакте в панике тоже наносит
        TryDamagePlayer();
    }

    void TryJump()
    {
        if (!_rat.HasGroundAhead(_rat.direction))
        {
            _rat.direction *= -1;
            if (!_rat.HasGroundAhead(_rat.direction)) return;
        }
        _rat.Jump(_rat.direction * _rat.jumpForceX * _rat.panicSpeedMultiplier, _rat.jumpForceY);
    }

    void TryDamagePlayer()
    {
        if (Time.time < _rat.LastDamageDealtTime + _rat.damageCooldown) return;

        int count = Physics2D.OverlapBoxNonAlloc(
            _rat.entityCollider.bounds.center,
            _rat.entityCollider.bounds.size,
            0f, _hitBuffer, _rat.hitTargetLayer);

        for (int i = 0; i < count; i++)
        {
            var entity = _hitBuffer[i]?.GetComponentInParent<BaseEntity>();
            if (entity == null || entity == _rat) continue;
            _rat.LastDamageDealtTime = Time.time;
            entity.TakeDamage(_rat.contactDamage, _rat.transform, _rat._name);
            break;
        }
    }

    public void Exit() { }
}
