using UnityEngine;

public class ThrowerPatrolState : IEnemyState
{
    readonly EnemyAIThrower _thrower;

    public ThrowerPatrolState(EnemyAIThrower thrower) => _thrower = thrower;

    public void Enter()
    {
        _thrower.SetAnimation(EnemyAIThrower.ThrowerAnimState.Patrol);
    }

    public void Update()
    {
        if (_thrower.CanSeePlayer())
        {
            if (_thrower.IsPlayerTooClose()) { _thrower.GoToFlee(); return; }
            _thrower.GoToAggro();
            return;
        }

        if (_thrower.IsWallAhead(_thrower.direction) || !_thrower.HasGroundAhead(_thrower.direction))
            _thrower.direction *= -1;

        _thrower.rb.linearVelocityX = _thrower.direction * _thrower.patrolSpeed;
    }

    public void Exit() => _thrower.rb.linearVelocityX = 0f;
}
