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

    [Tooltip("Дистанция, с которой можно применить эту атаку")]
    public float range = 1.5f;
    public abstract IEnumerator Execute(BaseEntity owner, Action onComplete = null);
}