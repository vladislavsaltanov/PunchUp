using System;
using UnityEngine;

public class AnimationTrigger : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] string triggerName;
    [SerializeField] string colliderTag;
    [SerializeField] bool triggerOnce;
    bool wasTriggered;
    public Action animationTriggered;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(colliderTag) && (!triggerOnce || !wasTriggered))
        {
            animator.SetTrigger(triggerName);
            wasTriggered = true;
            animationTriggered?.Invoke();
        }
    }
}
