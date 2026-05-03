using UnityEngine;

public class ShooterStunState : IEnemyState
{
    readonly EnemyAIShooter _shooter;
    float _duration;
    float _timer;

    public ShooterStunState(EnemyAIShooter shooter) => _shooter = shooter;

    public void SetDuration(float duration) => _duration = duration;

    public void Enter()
    {
        _timer = _duration;
        _shooter.rb.linearVelocity = Vector2.zero;
        _shooter.SetAnimation(EnemyAIShooter.ShooterAnimState.Stun);
    }

    public void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
            _shooter.GoToAggro();
    }

    public void Exit() { }
}
