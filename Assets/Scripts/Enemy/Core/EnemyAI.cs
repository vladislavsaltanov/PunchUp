using UnityEngine;
using System;
using System.Threading;

public interface IEnemyState 
{
    void Enter();
    void Update();
    void Exit();
}

public abstract class EnemyAI : BaseEntity 
{
    protected IEnemyState _currentState;

    protected virtual void Update() 
    {
        _currentState?.Update();
    }

    public void ChangeState(IEnemyState newState) 
    {
        if (_currentState == newState) return;
        
        _currentState?.Exit();
        _currentState = newState;
        _currentState?.Enter();
    }
}
