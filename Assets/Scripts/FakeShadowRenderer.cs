using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FakeShadowRenderer : MonoBehaviour
{
    [Header("Shadow Settings")]
    [SerializeField] private Color _shadowColor = new Color(0f, 0f, 0f, 0.3f);
    [SerializeField] private float _maxShadowAlpha = 0.75f;
    [SerializeField] private float _minShadowAlpha = 0.1f;
    [SerializeField] private float _groundCheckDistance = 8f;
    [SerializeField] private LayerMask _groundLayer = 1000000;
    [SerializeField] private float _yOffset = -0.15f;
    [SerializeField] private bool _updateShadowSize = true;
    [SerializeField] private float _shadowSizeMultiplier = 0.75f;

    [Header("References")]
    [SerializeField] private Transform _entityToFollow;
    [SerializeField] private SpriteRenderer _entitySpriteRenderer;

    private SpriteRenderer shadowRenderer;
    private Transform shadowTransform;

    private void Awake()
    {
        InitializeShadow();

        if (_entityToFollow == null)
            _entityToFollow = transform;

        if (_entitySpriteRenderer == null && _entityToFollow != null)
            _entitySpriteRenderer = _entityToFollow.GetComponent<SpriteRenderer>();

        UpdateShadowSize();
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
        int textureWidth = 128;
        int textureHeight = 64;
        Texture2D texture = new Texture2D(textureWidth, textureHeight);

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
                    float alpha = Mathf.Clamp01(1 - distanceSquared);
                    alpha = Mathf.SmoothStep(0, 1, alpha);
                    texture.SetPixel(x, y, new Color(1, 1, 1, alpha));
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        texture.Apply();

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, textureWidth, textureHeight),
            new Vector2(0.5f, 0.5f),
            Mathf.Max(textureWidth, textureHeight)
        );

        return sprite;
    }

    private void Update()
    {
        if (_entityToFollow == null || shadowRenderer == null)
            return;
        RaycastHit2D hit = GetRaycastHit();
        UpdateShadowPosition(hit);
        UpdateShadowAlpha(hit);
        if (_updateShadowSize)
            UpdateShadowSize();
    }

    private void UpdateShadowPosition(RaycastHit2D hit)
    {
        if (_entityToFollow == null) return;

        if (hit.collider != null)
        {
            shadowTransform.position = new Vector3(
                hit.point.x,
                hit.point.y + _yOffset,
                0
            );
        }
        else
        {
            Vector2 entityPosition = _entityToFollow.position;
            shadowTransform.position = new Vector3(
                entityPosition.x,
                entityPosition.y - _groundCheckDistance,
                0
            );
        }
    }

    private void UpdateShadowSize()
    {
        if (_entitySpriteRenderer == null) return;

        float entityWidth = _entitySpriteRenderer.bounds.size.x;
        float entityHeight = _entitySpriteRenderer.bounds.size.y;

        float shadowWidth = entityWidth * _shadowSizeMultiplier;
        float shadowHeight = entityHeight * _shadowSizeMultiplier * 0.6f;

        shadowTransform.localScale = new Vector3(
            shadowWidth,
            shadowHeight,
            1f
        );
    }

    private void UpdateShadowAlpha(RaycastHit2D hit)
    {

        if (hit.collider != null)
        {
            float distanceToGround = hit.distance;
            float normalizedDistance = Mathf.Clamp01(distanceToGround / _groundCheckDistance);
            float alpha = Mathf.Lerp(_maxShadowAlpha, _minShadowAlpha, normalizedDistance);

            Color currentColor = shadowRenderer.color;
            shadowRenderer.color = new Color(
                currentColor.r,
                currentColor.g,
                currentColor.b,
                alpha
            );
        }
        else
        {
            Color currentColor = shadowRenderer.color;
            shadowRenderer.color = new Color(
                currentColor.r,
                currentColor.g,
                currentColor.b,
                _minShadowAlpha
            );
        }
    }

    private RaycastHit2D GetRaycastHit()
    {
        Vector2 entityPosition = _entityToFollow.position;
        RaycastHit2D hit = Physics2D.Raycast(
            entityPosition,
            Vector2.down,
            _groundCheckDistance,
            _groundLayer
        );
        return hit;
    }

    private void OnDestroy()
    {
        if (shadowTransform != null && shadowTransform.gameObject != null)
        {
            Destroy(shadowTransform.gameObject);
        }
    }
}
