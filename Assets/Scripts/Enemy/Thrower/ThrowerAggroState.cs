using UnityEngine;

public class ThrowerAggroState : IEnemyState
{
    readonly EnemyAIThrower _thrower;
    bool _jumpedBack;

    public ThrowerAggroState(EnemyAIThrower thrower) => _thrower = thrower;

    public void Enter()
    {
        _thrower.SetAnimation(EnemyAIThrower.ThrowerAnimState.Aggro);
        _thrower.rb.linearVelocityX = 0f;
        _jumpedBack = false;

        // прыгаем назад перед броском
        if (_thrower.IsGrounded)
        {
            _thrower.JumpBack();
            _jumpedBack = true;
        }
    }

    public void Update()
    {
        if (!_thrower.CanSeePlayer()) { _thrower.GoToPatrol(); return; }
        if (_thrower.IsPlayerTooClose()) { _thrower.GoToFlee(); return; }

        // смотрим на игрока
        if (_thrower.Player != null)
        {
            float dx = _thrower.Player.transform.position.x - _thrower.transform.position.x;
            _thrower.direction = (sbyte)Mathf.Sign(dx);
        }

        // бросаем если приземлились и кулдаун прошёл
        if (_thrower.IsGrounded && Time.time >= _thrower.LastThrowTime + _thrower.throwCooldown)
            _thrower.Throw();
    }

    public void Exit() { }
}
