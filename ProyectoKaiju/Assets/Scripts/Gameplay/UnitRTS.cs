using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Represents a controllable RTS unit that can be selected and moved.
/// The unit type is automatically set from the GameObject tag.
/// </summary>
public class UnitRTS : MonoBehaviour
{
    // ─────────────────────────────────────
    // ▶ FIELDS & PROPERTIES
    // ─────────────────────────────────────

    private bool isSelected;
    private Renderer rendererComponent;
    private NavMeshAgent navAgent;

    private static readonly Color SelectedColor = Color.green;
    private static readonly Color DefaultColor = Color.white;

    /// <summary>
    /// Gets the logical unit type from the GameObject tag.
    /// </summary>
    public string UnitType => gameObject.tag;

    // ─────────────────────────────────────
    // ▶ UNITY LIFECYCLE
    // ─────────────────────────────────────

    private void Awake()
    {
        rendererComponent = GetComponent<Renderer>();
        navAgent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        Deselect();
    }

    private void Update()
    {
        HandleMovementInput();
    }

    // ─────────────────────────────────────
    // ▶ SELECTION API
    // ─────────────────────────────────────

    /// <summary>
    /// Marks the unit as selected and changes its appearance.
    /// </summary>
    public void Select()
    {
        isSelected = true;
        UpdateVisualFeedback();
    }

    /// <summary>
    /// Unmarks the unit as selected and resets its appearance.
    /// </summary>
    public void Deselect()
    {
        isSelected = false;
        UpdateVisualFeedback();
    }

    // ─────────────────────────────────────
    // ▶ MOVEMENT
    // ─────────────────────────────────────

    /// <summary>
    /// Handles right-click movement input.
    /// </summary>
    private void HandleMovementInput()
    {
        if (!isSelected) return;

        if (Input.GetMouseButtonDown(1))
        {
            SetDestinationFromMouse();
        }
    }

    /// <summary>
    /// Performs a raycast from the mouse and moves the unit to the clicked position.
    /// </summary>
    private void SetDestinationFromMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            navAgent.SetDestination(hit.point);
        }
    }

    // ─────────────────────────────────────
    // ▶ VISUAL FEEDBACK
    // ─────────────────────────────────────

    /// <summary>
    /// Updates the unit's material color based on its selection state.
    /// </summary>
    private void UpdateVisualFeedback()
    {
        if (rendererComponent != null)
        {
            rendererComponent.material.color = isSelected ? SelectedColor : DefaultColor;
        }
    }
}