using UnityEngine;

public class ThrowerFleeState : IEnemyState
{
    readonly EnemyAIThrower _thrower;
    float _fleeTimer;

    public ThrowerFleeState(EnemyAIThrower thrower) => _thrower = thrower;

    public void Enter()
    {
        _thrower.SetAnimation(EnemyAIThrower.ThrowerAnimState.Flee);
        _fleeTimer = _thrower.fleeDuration;

        // убегаем от игрока
        if (_thrower.Player != null)
        {
            float dx = _thrower.Player.transform.position.x - _thrower.transform.position.x;
            _thrower.direction = (sbyte)(-Mathf.Sign(dx));
        }
    }

    public void Update()
    {
        _fleeTimer -= Time.deltaTime;
        if (_fleeTimer <= 0f) { _thrower.GoToPatrol(); return; }

        if (_thrower.IsWallAhead(_thrower.direction) || !_thrower.HasGroundAhead(_thrower.direction))
        {
            _thrower.rb.linearVelocityX = 0f;
            return;
        }

        _thrower.rb.linearVelocityX = _thrower.direction * _thrower.fleeSpeed;
    }

    public void Exit() => _thrower.rb.linearVelocityX = 0f;
}
