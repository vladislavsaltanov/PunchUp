using UnityEngine;

public class VisualEffectsManager : MonoBehaviour
{
    private const string PrefabPath = "Debris";
    private static GameObject _cachedPrefab;
    private static readonly int TexturePropID = Shader.PropertyToID("_MainTex");

    public static void SpawnDebris(Texture2D texture, Vector3 position, short amount = 20)
    {
        if (_cachedPrefab == null)
        {
            _cachedPrefab = Resources.Load<GameObject>(PrefabPath);

            if (_cachedPrefab == null)
            {
                Debug.LogError($"Prefab was not found: Resources/{PrefabPath}");
                return;
            }
        }

        GameObject effect = Object.Instantiate(_cachedPrefab, position, Quaternion.identity);
        var ps = effect.GetComponent<ParticleSystem>();
        ParticleSystemRenderer psRenderer = effect.GetComponent<ParticleSystemRenderer>();

        var emission = ps.emission;
        var burst = new ParticleSystem.Burst(0f, amount);
        emission.SetBursts(new ParticleSystem.Burst[] { burst });

        MaterialPropertyBlock block = new MaterialPropertyBlock();
        block.SetTexture(TexturePropID, texture);
        psRenderer.SetPropertyBlock(block);

        
        Object.Destroy(effect, ps.main.duration + ps.main.startLifetime.constantMax);
    }
}
