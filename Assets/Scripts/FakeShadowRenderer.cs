using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FakeShadowRenderer : MonoBehaviour
{
    [Header("Shadow Settings")]
    [SerializeField] private Color _shadowColor = new Color(0f, 0f, 0f, 0.3f);
    [SerializeField] private float _maxShadowAlpha = 0.75f;
    [SerializeField] private float _minShadowAlpha = 0f;

    [SerializeField, Min(0.01f)] private float _groundCheckDistance = 8f;
    [SerializeField] private LayerMask _groundLayer = 1000000;
    [SerializeField] private float _yOffset = -0.15f;
    [SerializeField] private bool _updateShadowSize = true;
    [SerializeField] private float _shadowSizeMultiplier = 0.75f;

    [Header("Smoothing")]
    [SerializeField, Min(0.001f)] private float _alphaSmoothTime = 0.08f;

    [Header("Step/Snap Fix")]
    [SerializeField, Min(0.001f)] private float _groundSnapThreshold = 0.15f;
    [SerializeField, Min(0f)] private float _snapAlphaZeroSeconds = 0.05f;

    [Header("References")]
    [SerializeField] private Transform _entityToFollow;
    [SerializeField] private SpriteRenderer _entitySpriteRenderer;

    private SpriteRenderer shadowRenderer;
    private Transform shadowTransform;

    float currentAlpha;
    float alphaVelocity;

    bool hadGroundLastFrame;
    bool hasLastGroundPoint;
    Vector2 lastGroundPoint;

    float forceZeroUntilTime;

    private void Awake()
    {
        InitializeShadow();

        if (_entityToFollow == null)
            _entityToFollow = transform;

        if (_entitySpriteRenderer == null && _entityToFollow != null)
            _entitySpriteRenderer = _entityToFollow.GetComponent<SpriteRenderer>();

        UpdateShadowSize();

        currentAlpha = 0f;
        ApplyAlpha(currentAlpha);
    }

    private void InitializeShadow()
    {
        GameObject shadowObject = new GameObject("FakeShadow");
        shadowTransform = shadowObject.transform;

        shadowRenderer = shadowObject.AddComponent<SpriteRenderer>();
        shadowRenderer.sprite = CreateCircleSprite();
        shadowRenderer.color = _shadowColor;
        shadowRenderer.sortingOrder = 100;

        shadowTransform.SetParent(transform);
        shadowTransform.localPosition = Vector3.zero;
        shadowTransform.localRotation = Quaternion.identity;
    }

    private Sprite CreateCircleSprite()
    {
        const int textureWidth = 128;
        const int textureHeight = 64;

        Texture2D texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        float ellipseWidth = textureWidth * 0.9f;
        float ellipseHeight = textureHeight * 0.9f;

        Vector2 center = new Vector2(textureWidth / 2f, textureHeight / 2f);

        for (int y = 0; y < textureHeight; y++)
        {
            for (int x = 0; x < textureWidth; x++)
            {
                float normalizedX = (x - center.x) / (ellipseWidth / 2f);
                float normalizedY = (y - center.y) / (ellipseHeight / 2f);
                float distanceSquared = normalizedX * normalizedX + normalizedY * normalizedY;

                if (distanceSquared <= 1f)
                {
                    float a = Mathf.Clamp01(1f - distanceSquared);
                    a = Mathf.SmoothStep(0f, 1f, a);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }

        texture.Apply(false, false);

        return Sprite.Create(
            texture,
            new Rect(0, 0, textureWidth, textureHeight),
            new Vector2(0.5f, 0.5f),
            Mathf.Max(textureWidth, textureHeight)
        );
    }

    private void Update()
    {
        if (_entityToFollow == null || shadowRenderer == null || shadowTransform == null)
            return;

        RaycastHit2D hit = GetRaycastHit();
        bool hasGroundNow = hit.collider != null;

        // Snap detection: ground height jumps OR raycast reacquires ground after missing it.
        if (hasGroundNow)
        {
            Vector2 groundPoint = hit.point;
    
            if (hasLastGroundPoint)
            {
                float dy = Mathf.Abs(groundPoint.y - lastGroundPoint.y);
                if (dy >= _groundSnapThreshold)
                    ForceAlphaZeroMoment();
            }

            if (!hadGroundLastFrame)
            {
                // Reacquired ground this frame -> treat like a snap (typical ledge jitter case)
                ForceAlphaZeroMoment();
            }

            lastGroundPoint = groundPoint;
            hasLastGroundPoint = true;
        }

        hadGroundLastFrame = hasGroundNow;

        UpdateShadowPosition(hit, hasGroundNow);
        UpdateShadowAlpha(hit, hasGroundNow);

        if (_updateShadowSize)
            UpdateShadowSize();
    }

    void ForceAlphaZeroMoment()
    {
        forceZeroUntilTime = Time.time + _snapAlphaZeroSeconds;
        alphaVelocity = 0f;
        currentAlpha = 0f;
        ApplyAlpha(0f);
    }

    private void UpdateShadowPosition(RaycastHit2D hit, bool hasGroundNow)
    {
        // Requirement: ALWAYS on ground.
        // If no ground hit, stay on last known ground point (still ground).
        Vector2 p;

        if (hasGroundNow)
        {
            p = hit.point;
        }
        else if (hasLastGroundPoint)
        {
            p = lastGroundPoint;
        }
        else
        {
            // No ground ever found yet (first frames) -> keep at entity X, but don't fake Y.
            // This still follows "always ground" best-effort until the first hit appears.
            p = _entityToFollow.position;
        }

        shadowTransform.position = new Vector3(p.x, p.y + _yOffset, 0f);
    }

    private void UpdateShadowSize()
    {
        if (_entitySpriteRenderer == null) return;

        float entityWidth = _entitySpriteRenderer.bounds.size.x;
        float entityHeight = _entitySpriteRenderer.bounds.size.y;

        float shadowWidth = entityWidth * _shadowSizeMultiplier;
        float shadowHeight = entityHeight * _shadowSizeMultiplier * 0.6f;

        shadowTransform.localScale = new Vector3(shadowWidth, shadowHeight, 1f);
    }

    private void UpdateShadowAlpha(RaycastHit2D hit, bool hasGroundNow)
    {
        float targetAlpha;

        // During snap window: force 0, then recover smoothly.
        if (Time.time < forceZeroUntilTime)
        {
            targetAlpha = 0f;
        }
        else if (!hasGroundNow)
        {
            // No ground in ray distance -> shadow should fade out, but position stays on last ground
            targetAlpha = 0f;
        }
        else
        {
            float t = Mathf.Clamp01(hit.distance / _groundCheckDistance);
            targetAlpha = Mathf.Clamp01(Mathf.Lerp(_maxShadowAlpha, _minShadowAlpha, t));
        }

        currentAlpha = Mathf.SmoothDamp(currentAlpha, targetAlpha, ref alphaVelocity, _alphaSmoothTime);
        ApplyAlpha(currentAlpha);
    }

    private void ApplyAlpha(float alpha)
    {
        Color c = shadowRenderer.color;
        shadowRenderer.color = new Color(c.r, c.g, c.b, Mathf.Clamp01(alpha));
    }

    private RaycastHit2D GetRaycastHit()
    {
        Vector2 origin = _entityToFollow.position;
        return Physics2D.Raycast(origin, Vector2.down, _groundCheckDistance, _groundLayer);
    }

    private void OnDestroy()
    {
        if (shadowTransform != null && shadowTransform.gameObject != null)
            Destroy(shadowTransform.gameObject);
    }
}
