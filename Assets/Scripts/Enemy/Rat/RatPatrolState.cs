using UnityEngine;

public class RatPatrolState : IEnemyState
{
    readonly EnemyAIRat _rat;
    float _jumpTimer;

    public RatPatrolState(EnemyAIRat rat) => _rat = rat;

    public void Enter() => _jumpTimer = _rat.jumpInterval;

    public void Update()
    {
        if (_rat.IsPanic) { _rat.GoToPanic(); return; }
        if (_rat.CanSeePlayer()) { _rat.GoToAggro(); return; }

        if (_rat.IsGrounded && _rat.IsWallAhead(_rat.direction))
            _rat.direction *= -1;

        _jumpTimer -= Time.deltaTime;
        if (_jumpTimer <= 0f && _rat.IsGrounded)
        {
            _jumpTimer = _rat.jumpInterval;
            TryJump();
        }
    }

    void TryJump()
    {
        if (!_rat.HasGroundAhead(_rat.direction))
        {
            _rat.direction *= -1;
            if (!_rat.HasGroundAhead(_rat.direction)) return; // оба края - стоим
        }
        _rat.Jump(_rat.direction * _rat.jumpForceX, _rat.jumpForceY);
    }

    public void Exit() { }
}
