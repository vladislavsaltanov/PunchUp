using UnityEngine;

public class ThrowerStunState : IEnemyState
{
    readonly EnemyAIThrower _thrower;
    float _duration;
    float _timer;

    public ThrowerStunState(EnemyAIThrower thrower) => _thrower = thrower;

    public void SetDuration(float duration) => _duration = duration;

    public void Enter()
    {
        _timer = _duration;
        _thrower.rb.linearVelocity = Vector2.zero;
        _thrower.SetAnimation(EnemyAIThrower.ThrowerAnimState.Stun);
    }

    public void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0f) _thrower.GoToPatrol();
    }

    public void Exit() { }
}
