using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Provides tactile, springy AAA click and hover feedback to any UI button or card.
/// Includes:
/// - Smooth scale punch on hover (1.04x) and press down (0.93x)
/// - Elastic bounce-back on release
/// - Procedural mechanical click and soft hover sound effects
/// - Color tint transitions
/// - Selection state outline support
/// </summary>
public class UIButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [Header("Scale Settings")]
    public float normalScale = 1.0f;
    public float hoverScale = 1.04f;
    public float pressedScale = 0.93f;
    public float scaleSpeed = 16f;

    [Header("Color Tint Settings")]
    public Graphic targetGraphic;
    public Color normalColor = Color.white;
    public Color hoverColor = new Color(1.15f, 1.15f, 1.15f, 1f);
    public Color pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);

    [Header("Selection State")]
    public bool isSelected = false;
    public GameObject selectionGlow;
    public Color selectedOutlineColor = new Color(1f, 0.8f, 0.1f, 1f);

    [Header("Audio")]
    public bool playClickSound = true;
    public bool playHoverSound = true;

    private bool isHovered = false;
    private bool isPressed = false;
    private Vector3 currentScale;
    private Vector3 targetScaleVec;

    void Awake()
    {
        if (targetGraphic == null) targetGraphic = GetComponent<Graphic>();
        currentScale = Vector3.one * normalScale;
        targetScaleVec = currentScale;
        transform.localScale = currentScale;
    }

    void OnEnable()
    {
        isHovered = false;
        isPressed = false;
        currentScale = Vector3.one * normalScale;
        transform.localScale = currentScale;
        UpdateVisualsInstant();
    }

    void Update()
    {
        // 1. Calculate Target Scale
        float targetS = normalScale;
        if (isPressed) targetS = pressedScale;
        else if (isHovered) targetS = hoverScale;
        else if (isSelected) targetS = normalScale * 1.02f;

        targetScaleVec = Vector3.one * targetS;
        currentScale = Vector3.Lerp(currentScale, targetScaleVec, Time.unscaledDeltaTime * scaleSpeed);
        transform.localScale = currentScale;

        // 2. Color Transition
        if (targetGraphic != null)
        {
            Color targetC = normalColor;
            if (isPressed) targetC = pressedColor;
            else if (isHovered) targetC = hoverColor;
            targetGraphic.color = Color.Lerp(targetGraphic.color, targetC, Time.unscaledDeltaTime * 12f);
        }

        // 3. Selection Glow
        if (selectionGlow != null)
        {
            selectionGlow.SetActive(isSelected);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        if (playHoverSound)
        {
            ProceduralUISound.Instance.PlayHover();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        isPressed = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        if (playClickSound)
        {
            ProceduralUISound.Instance.PlayClick();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Elastic rebound bounce
        currentScale = Vector3.one * (pressedScale * 0.98f);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (selectionGlow != null) selectionGlow.SetActive(selected);
    }

    private void UpdateVisualsInstant()
    {
        if (targetGraphic != null) targetGraphic.color = normalColor;
        if (selectionGlow != null) selectionGlow.SetActive(isSelected);
    }
}
