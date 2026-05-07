using UnityEngine;

public class BossIdleState : IEnemyState
{
    readonly EnemyAIBoss _boss;
    float _timer;

    public BossIdleState(EnemyAIBoss boss) => _boss = boss;

    public void Enter()
    {
        _boss.ConsecutiveJumps = 0;
        _boss.CanDealDamage = true;
        
        if (!_boss.PreventVelocityReset)
        {
            _boss.rb.linearVelocity = Vector2.zero;
        }
        _boss.PreventVelocityReset = false;

        if (_boss.hitbox != null) _boss.hitbox.gameObject.SetActive(false);
        _timer = _boss.GetCooldown();
    }

    public void Update()
    {
        FacePlayer();
        _timer -= Time.deltaTime;
        if (_timer <= 0f) PickAttack();
    }

    void FacePlayer()
    {
        if (_boss.Player == null) return;
        float dx = _boss.Player.transform.position.x - _boss.transform.position.x;
        if (Mathf.Abs(dx) > 0.1f)
        {
            _boss.direction = (sbyte)Mathf.Sign(dx);
            _boss.UpdateVisualDirection();
        }
    }

    void PickAttack()
    {
        int pool = _boss.CurrentPhase == 1 ? 3 : 5;
        switch (Random.Range(0, pool))
        {
            case 0: _boss.GoToDash(); break;
            case 1: _boss.GoToJumpSlam(); break;
            case 2: _boss.GoToProjectile(); break;
            case 3: _boss.GoToCombo(); break;
            case 4: _boss.GoToGroundSlam(); break;
        }
    }

    public void Exit()
    {
        _boss.IsVulnerable = false;
    }
}
