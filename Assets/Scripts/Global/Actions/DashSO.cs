using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Actions/Dash")]
public class DashSO : ActionSO
{
    public float dashSpeed = 20f;

    public override IEnumerator Execute(BaseEntity owner, Action onComplete = null)
    {
        if (!string.IsNullOrEmpty(animationTrigger) && owner.animator != null)
            owner.animator.SetTrigger(animationTrigger);

        Vector2 velocity = new Vector2(owner.direction * dashSpeed, 0f);
        owner.ApplyVelocityOverride(velocity, duration);

        yield return new WaitForSeconds(duration);

        onComplete?.Invoke();
    }
}