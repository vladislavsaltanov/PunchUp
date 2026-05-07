using UnityEngine;

public class VisualEffectsManager : MonoBehaviour
{
    private const string debrisPath = "Debris";
    private const string explosionPath = "Explosion";
    private static GameObject _cachedPrefab;
    private static readonly int TexturePropID = Shader.PropertyToID("_MainTex");

    public static void SpawnDebris(Texture2D texture, Vector3 position, short amount = 20, float scale = 1f)
    {
        if (_cachedPrefab == null)
        {
            _cachedPrefab = Resources.Load<GameObject>(debrisPath);

            if (_cachedPrefab == null)
            {
                Debug.LogError($"Prefab was not found: Resources/{debrisPath}");
                return;
            }
        }

        GameObject effect = Object.Instantiate(_cachedPrefab, position, Quaternion.identity);
        var ps = effect.GetComponent<ParticleSystem>();
        ParticleSystemRenderer psRenderer = effect.GetComponent<ParticleSystemRenderer>();

        var emission = ps.emission;
        var burst = new ParticleSystem.Burst(0f, amount);
        emission.SetBursts(new ParticleSystem.Burst[] { burst });

        var main = ps.main;
        var startSize = main.startSize;
        startSize.constantMin *= scale;
        startSize.constantMax *= scale;
        main.startSize = startSize;

        MaterialPropertyBlock block = new MaterialPropertyBlock();
        block.SetTexture(TexturePropID, texture);
        psRenderer.SetPropertyBlock(block);

        
        Object.Destroy(effect, ps.main.duration + ps.main.startLifetime.constantMax);
    }
    public static void SpawnDebris(Texture2D texture, Vector3 position, Color color, short amount = 20, float scale = 1f)
    {
        if (_cachedPrefab == null)
        {
            _cachedPrefab = Resources.Load<GameObject>(debrisPath);

            if (_cachedPrefab == null)
            {
                Debug.LogError($"Prefab was not found: Resources/{debrisPath}");
                return;
            }
        }

        GameObject effect = Object.Instantiate(_cachedPrefab, position, Quaternion.identity);
        var ps = effect.GetComponent<ParticleSystem>();
        ParticleSystemRenderer psRenderer = effect.GetComponent<ParticleSystemRenderer>();

        var emission = ps.emission;
        var burst = new ParticleSystem.Burst(0f, amount);
        emission.SetBursts(new ParticleSystem.Burst[] { burst });

        var main = ps.main;
        var startSize = main.startSize;
        startSize.constantMin *= scale;
        startSize.constantMax *= scale;
        main.startSize = startSize;
        main.startColor = color;

        MaterialPropertyBlock block = new MaterialPropertyBlock();
        block.SetTexture(TexturePropID, texture);
        psRenderer.SetPropertyBlock(block);


        Object.Destroy(effect, ps.main.duration + ps.main.startLifetime.constantMax);
    }

    public static void SpawnExplosion(Texture2D texture, Vector3 position, Color color, short amount = 45, float scale = 1f)
    {
        if (_cachedPrefab == null)
        {
            _cachedPrefab = Resources.Load<GameObject>(explosionPath);

            if (_cachedPrefab == null)
            {
                Debug.LogError($"Prefab was not found: Resources/{explosionPath}");
                return;
            }
        }

        GameObject effect = Object.Instantiate(_cachedPrefab, position, Quaternion.identity);
        var ps = effect.GetComponent<ParticleSystem>();
        ParticleSystemRenderer psRenderer = effect.GetComponent<ParticleSystemRenderer>();

        var emission = ps.emission;
        var burst = new ParticleSystem.Burst(0f, amount);
        emission.SetBursts(new ParticleSystem.Burst[] { burst });

        var main = ps.main;
        var startSize = main.startSize;
        startSize.constantMin *= scale;
        startSize.constantMax *= scale;
        main.startSize = startSize;
        main.startColor = color;

        MaterialPropertyBlock block = new MaterialPropertyBlock();
        block.SetTexture(TexturePropID, texture);
        psRenderer.SetPropertyBlock(block);


        Object.Destroy(effect, ps.main.duration + ps.main.startLifetime.constantMax);
    }
}
