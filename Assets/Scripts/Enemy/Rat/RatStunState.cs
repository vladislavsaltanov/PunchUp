using UnityEngine;

public class RatStunState : IEnemyState
{
    readonly EnemyAIRat _rat;
    float _duration;
    float _timer;

    public RatStunState(EnemyAIRat rat) => _rat = rat;

    public void SetDuration(float duration) => _duration = duration;

    public void Enter()
    {
        _timer = _duration;
        _rat.rb.linearVelocity = Vector2.zero;
    }

    public void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            if (_rat.IsPanic) _rat.GoToPanic();
            else _rat.GoToAggro();
        }
    }

    public void Exit() { }
}
