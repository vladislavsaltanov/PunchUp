using UnityEngine;

public abstract class EnemyCombatLogicSO : ScriptableObject
{
    // Timeout after entity cancels combat state due to hit
    public float hitTimeout = 5f;

    public abstract void Execute(EnemyLogic logic, EnemyContextState context, GameObject player);
    public abstract bool IsPlayerCloseEnough(float distanceToPlayer);
    public virtual bool HitTimeout(float lastHitTime) =>
        Time.time > lastHitTime + hitTimeout;
}
