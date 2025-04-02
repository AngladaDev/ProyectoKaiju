using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Represents a controllable RTS unit that can be selected and moved.
/// The unit type is automatically set from the GameObject tag.
/// </summary>
public class UnitRTS : MonoBehaviour
{
    private bool isSelected;

    private Renderer rend;
    private NavMeshAgent agent;

    private static readonly Color SelectedColor = Color.green;
    private static readonly Color DefaultColor = Color.white;

    /// <summary>
    /// Gets the logical unit type.
    /// </summary>
    public string UnitType => gameObject.tag;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        Deselect();
    }

    private void Update()
    {
        // Right-click to move if selected
        if (isSelected && Input.GetMouseButtonDown(1))
        {
            TrySetDestinationFromMouse();
        }
    }

    /// <summary>
    /// Marks the unit as selected and changes its appearance.
    /// </summary>
    public void Select()
    {
        isSelected = true;
        SetColor(SelectedColor);
    }

    /// <summary>
    /// Unmarks the unit as selected and resets its appearance.
    /// </summary>
    public void Deselect()
    {
        isSelected = false;
        SetColor(DefaultColor);
    }

    /// <summary>
    /// Performs a raycast from the mouse and moves the unit to the clicked position.
    /// </summary>
    private void TrySetDestinationFromMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            agent.SetDestination(hit.point);
        }
    }

    /// <summary>
    /// Changes the material color of the unit.
    /// </summary>
    /// <param name="color">Target color.</param>
    private void SetColor(Color color)
    {
        if (rend != null)
        {
            rend.material.color = color;
        }
    }
}
