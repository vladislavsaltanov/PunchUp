using UnityEngine;

public abstract class PlayerCombatLogicSO : ScriptableObject
{
    public abstract void Execute(PlayerController player);
}
