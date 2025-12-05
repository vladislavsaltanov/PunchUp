using System;
using System.Collections;
using UnityEngine;

public abstract class ActionSO : ScriptableObject
{
    [Header("Timing")]
    public float duration = 0.5f;
    public float cooldown = 1f;

    [Header("Animation")]
    public string animationTrigger;

    public abstract IEnumerator Execute(BaseEntity owner, Action onComplete = null);
}