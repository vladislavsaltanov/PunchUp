using TMPro;
using UnityEngine;
using System.Threading;

public class DamageText : MonoBehaviour
{
    private TextMeshPro textMesh;

    private float moveSpeed = 2f;

    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }
    void LateUpdate()
    {
        if (Camera.main != null)
            transform.forward = Camera.main.transform.forward;
    }
    public async Awaitable AnimateAndDisable(
        string amount,
        Color color,
        CancellationToken token)
    {
        textMesh.text = amount;

        Vector3 direction =
            new Vector3(Random.Range(-0.7f, 0.7f), 1f, 0);

        float elapsed = 0f;
        float duration = 1f;

        while (elapsed < duration)
        {
            if (token.IsCancellationRequested)
                return;

            transform.position +=
                direction * moveSpeed * Time.deltaTime;

            float t = elapsed / duration;

            Color c = color;

            // fade in
            if (t < 0.2f)
                c.a = Mathf.Lerp(0, 1, t / 0.2f);

            // fade out
            else
                c.a = Mathf.Lerp(1, 0, (t - 0.2f) / 0.8f);

            textMesh.color = c;

            elapsed += Time.deltaTime;

            await Awaitable.NextFrameAsync(token);
        }

        gameObject.SetActive(false);
    }
}
