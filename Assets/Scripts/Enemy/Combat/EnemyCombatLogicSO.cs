using UnityEngine;

public abstract class EnemyCombatLogicSO : ScriptableObject
{
    public abstract void Attack(EnemyLogic logic, EnemyCombatState state);
}
