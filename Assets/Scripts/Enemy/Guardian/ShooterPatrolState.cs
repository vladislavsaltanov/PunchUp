using UnityEngine;

public class ShooterPatrolState : IEnemyState
{
    readonly EnemyAIShooter _shooter;

    public ShooterPatrolState(EnemyAIShooter shooter) => _shooter = shooter;

    public void Enter()
    {
        _shooter.SetAnimation(EnemyAIShooter.ShooterAnimState.Patrol);
    }

    public void Update()
    {
        if (_shooter.CanSeePlayer()) { _shooter.GoToAggro(); return; }

        if (_shooter.IsWallAhead(_shooter.direction) || !_shooter.HasGroundAhead(_shooter.direction))
            _shooter.direction *= -1;

        _shooter.rb.linearVelocityX = _shooter.direction * _shooter.patrolSpeed;
        _shooter.FaceDirection(_shooter.transform.position.x + _shooter.direction);
    }

    public void Exit() => _shooter.rb.linearVelocityX = 0f;
}
