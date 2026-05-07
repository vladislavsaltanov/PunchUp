using UnityEngine;
using TMPro;
using System.Threading;

public class DamageText : MonoBehaviour
{
    private TMP_Text textMesh;
    private float moveSpeed = 2f;
    private float fadeSpeed = 2f;

    void Awake() => textMesh = GetComponent<TMP_Text>();

    public async Awaitable AnimateAndDisable(string amount, Color color, CancellationToken token)
    {
        textMesh.text = amount;
        textMesh.color = color;

        Vector3 direction = new Vector3(Random.Range(-0.7f, 0.7f), 1f, 0).normalized;
        float elapsed = 0f;
        float duration = 1.0f;

        while (elapsed < duration)
        {
            if (token.IsCancellationRequested) return;

            transform.position += direction * moveSpeed * Time.deltaTime;

            Color c = textMesh.color;
            c.a = Mathf.Lerp(1, 0, elapsed / duration);
            textMesh.color = c;

            elapsed += Time.deltaTime;
            await Awaitable.NextFrameAsync(token);
        }

        gameObject.SetActive(false);
    }
}
