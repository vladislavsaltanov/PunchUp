using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ToggleWithStates : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Toggle toggle;
    [SerializeField] private Image buttonImage;

    [Header("ВЫКЛ")]
    [SerializeField] private Color offNormal = Color.white;
    [SerializeField] private Color offHighlighted = new Color(0.9f, 0.9f, 0.9f);
    [SerializeField] private Color offPressed = new Color(0.7f, 0.7f, 0.7f);

    [Header("ВКЛ")]
    [SerializeField] private Color onNormal = Color.green;
    [SerializeField] private Color onHighlighted = new Color(0.5f, 1f, 0.5f);
    [SerializeField] private Color onPressed = new Color(0.3f, 0.8f, 0.3f);

    private bool isHovered = false;
    private bool isPressed = false;

    void Start()
    {
        toggle.onValueChanged.AddListener(OnToggleChanged);
        UpdateColor();
    }

    void OnToggleChanged(bool value)
    {
        UpdateColor();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        UpdateColor();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        isPressed = false;
        UpdateColor();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        UpdateColor();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        UpdateColor();
    }

    void UpdateColor()
    {
        Color color;

        if (toggle.isOn)
        {
            if (isPressed) color = onPressed;
            else if (isHovered) color = onHighlighted;
            else color = onNormal;
        }
        else
        {
            if (isPressed) color = offPressed;
            else if (isHovered) color = offHighlighted;
            else color = offNormal;
        }

        buttonImage.color = color;
    }
}
