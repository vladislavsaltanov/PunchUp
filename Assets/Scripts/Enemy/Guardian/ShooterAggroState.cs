using UnityEngine;

public class ShooterAggroState : IEnemyState
{
    readonly EnemyAIShooter _shooter;
    float _fireTimer;
    float _lostTargetTimer;
    bool _playerLost;

    public ShooterAggroState(EnemyAIShooter shooter) => _shooter = shooter;

    public void Enter()
    {
        _fireTimer = 0f;
        _lostTargetTimer = 0f;
        _playerLost = false;
        _shooter.SetAnimation(EnemyAIShooter.ShooterAnimState.Aggro);
    }

    public void Update()
    {
        bool sees = _shooter.CanSeePlayer();

        if (!sees)
        {
            if (!_playerLost)
            {
                _playerLost = true;
                _lostTargetTimer = _shooter.lostTargetFireDuration;
            }
            _lostTargetTimer -= Time.deltaTime;
            if (_lostTargetTimer <= 0f) { _shooter.GoToPatrol(); return; }
        }
        else
        {
            _playerLost = false;
            FacePlayer();
            HandleRetreat();
        }

        _fireTimer -= Time.deltaTime;
        if (_fireTimer <= 0f)
        {
            _shooter.Shoot();
            _fireTimer = 1f / _shooter.fireRate;
        }
    }

    void FacePlayer()
    {
        if (_shooter.Player == null) return;
        float dx = _shooter.Player.transform.position.x - _shooter.transform.position.x;
        _shooter.direction = (sbyte)Mathf.Sign(dx);
        _shooter.UpdateVisualDirection();
    }

    void HandleRetreat()
    {
        if (_shooter.Player == null) return;
        float dist = Vector2.Distance(_shooter.transform.position, _shooter.Player.transform.position);

        if (dist < _shooter.retreatDistance)
        {
            sbyte retreatDir = (sbyte)(-_shooter.direction);

            if (_shooter.IsWallAhead(retreatDir) || !_shooter.HasGroundAhead(retreatDir))
            {
                _shooter.rb.linearVelocityX = 0f;
                return;
            }

            _shooter.rb.linearVelocityX = retreatDir * _shooter.retreatSpeed;
        }
        else
        {
            _shooter.rb.linearVelocityX = 0f;
        }
    }

    public void Exit() => _shooter.rb.linearVelocityX = 0f;
}
