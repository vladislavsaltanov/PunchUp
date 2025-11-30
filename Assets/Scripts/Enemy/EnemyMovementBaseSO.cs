using UnityEngine;

public abstract class EnemyMovementBaseSO : ScriptableObject
{
    public abstract void Movement(EnemyLogic logic, EnemyBaseMovementState state);
}
