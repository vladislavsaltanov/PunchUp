using UnityEngine;

public class RatAggroState : IEnemyState
{
    readonly EnemyAIRat _rat;
    float _jumpTimer;
    float _searchTimer;
    static readonly Collider2D[] _hitBuffer = new Collider2D[4];

    public RatAggroState(EnemyAIRat rat) => _rat = rat;

    public void Enter()
    {
        _jumpTimer = 0f;
        _searchTimer = _rat.aggroSearchDuration;
    }

    public void Update()
    {
        if (_rat.IsPanic) { _rat.GoToPanic(); return; }

        if (!_rat.CanSeePlayer())
        {
            _searchTimer -= Time.deltaTime;
            if (_searchTimer <= 0f) { _rat.GoToPatrol(); return; }
        }
        else
        {
            _searchTimer = _rat.aggroSearchDuration;
        }

        TryDamagePlayer();

        if (_rat.IsGrounded && _rat.IsWallAhead(_rat.direction))
            _rat.direction *= -1;

        _jumpTimer -= Time.deltaTime;
        if (_jumpTimer <= 0f && _rat.IsGrounded)
        {
            _jumpTimer = _rat.aggroJumpInterval;
            TryJump();
        }
    }

    void TryJump()
    {
        if (_rat.Player != null)
        {
            float dx = _rat.Player.transform.position.x - _rat.transform.position.x;
            _rat.direction = (sbyte)Mathf.Sign(dx);
        }

        if (!_rat.HasGroundAhead(_rat.direction))
        {
            _rat.direction *= -1;
            if (!_rat.HasGroundAhead(_rat.direction)) return;
        }

        _rat.Jump(_rat.direction * _rat.jumpForceX, _rat.jumpForceY);
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
