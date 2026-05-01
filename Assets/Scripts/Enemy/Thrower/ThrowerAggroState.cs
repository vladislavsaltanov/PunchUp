using UnityEngine;

public class ThrowerAggroState : IEnemyState
{
    readonly EnemyAIThrower _thrower;
    bool _isJumping;

    public ThrowerAggroState(EnemyAIThrower thrower) => _thrower = thrower;

    public void Enter()
    {
        _thrower.SetAnimation(EnemyAIThrower.ThrowerAnimState.Aggro);
        _thrower.rb.linearVelocityX = 0f;
        _isJumping = false;
        if (Time.time - _thrower.LastThrowTime > _thrower.throwCooldown)
            _thrower.LastThrowTime = Time.time - _thrower.throwCooldown;
    }

    public void Update()
    {
        if (!_thrower.CanSeePlayer()) { _thrower.GoToPatrol(); return; }

        FacePlayer();

        if (_isJumping)
        {
            if (_thrower.IsGrounded && _thrower.rb.linearVelocityY <= 0f)
                _isJumping = false;
            return;
        }

        if (!_thrower.IsGrounded) return;

        if (Time.time >= _thrower.LastThrowTime + _thrower.throwCooldown)
        {
            _thrower.Jump();
            _isJumping = true;
            _thrower.Throw();
        }
    }

    void FacePlayer()
    {
        if (_thrower.Player == null) return;
        float dx = _thrower.Player.transform.position.x - _thrower.transform.position.x;
        _thrower.direction = (sbyte)Mathf.Sign(dx);
    }

    public void Exit() { }
}
