using System.Collections;
using UnityEngine;

public class Test_EffectTrigger : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer spriteRenderer;
    [SerializeField]
    float cooldown = 5f;
    bool onCooldown = false;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (onCooldown) return;
        if (collision.CompareTag("Player"))
        {
            spriteRenderer.enabled = false;
            onCooldown = true;
            StartCoroutine(resetCooldown());
        }
    }

    IEnumerator resetCooldown()
    {
        yield return new WaitForSeconds(cooldown);
        spriteRenderer.enabled = true;
        onCooldown = false;
    }
}
