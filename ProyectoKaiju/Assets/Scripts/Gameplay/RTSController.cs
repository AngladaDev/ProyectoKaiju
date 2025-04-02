using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages unit selection (click, drag-box, double-click) in RTS gameplay.
/// Attach to an input controller GameObject.
/// </summary>
public class RTSController : MonoBehaviour
{
    /// <summary> UI representation of the selection box. </summary>
    public RectTransform selectionBoxUI;

    private Vector2 startMousePos;
    private Vector2 endMousePos;
    private bool isDragging;

    private List<UnitRTS> selectedUnits = new List<UnitRTS>();

    private float lastClickTime;
    private const float doubleClickThreshold = 0.2f;
    private const float dragThreshold = 10f;

    private void Update()
    {
        HandleSelectionInput();
        DrawSelectionBox();
        CheckEscapeKey();
    }

    private void HandleSelectionInput()
    {
        // Begin drag selection
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            startMousePos = Input.mousePosition;
        }

        // End drag selection or handle click
        if (Input.GetMouseButtonUp(0))
        {
            endMousePos = Input.mousePosition;
            isDragging = false;

            if (Vector2.Distance(startMousePos, endMousePos) > dragThreshold)
                SelectUnitsInArea();
            else
                CheckSingleClick();
        }
    }

    private void DrawSelectionBox()
    {
        if (selectionBoxUI == null) return;

        selectionBoxUI.gameObject.SetActive(isDragging);

        if (isDragging)
        {
            selectionBoxUI.position = (startMousePos + (Vector2)Input.mousePosition) / 2;
            selectionBoxUI.sizeDelta = new Vector2(
                Mathf.Abs(startMousePos.x - Input.mousePosition.x),
                Mathf.Abs(startMousePos.y - Input.mousePosition.y));
        }
    }

    private void SelectUnitsInArea()
    {
        List<UnitRTS> unitsInBox = new List<UnitRTS>();

        Vector2 min = Vector2.Min(startMousePos, endMousePos);
        Vector2 max = Vector2.Max(startMousePos, endMousePos);
        Rect selectionRect = new Rect(min, max - min);

        // Gather units within selection rectangle
        foreach (UnitRTS unit in FindObjectsByType<UnitRTS>(FindObjectsSortMode.None))
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(unit.transform.position);
            if (selectionRect.Contains(screenPos, true))
                unitsInBox.Add(unit);
        }

        if (unitsInBox.Count == 0)
        {
            DeselectAll();
            return;
        }

        // Determine if all units are already selected
        bool allSelected = unitsInBox.TrueForAll(unit => selectedUnits.Contains(unit));

        foreach (UnitRTS unit in unitsInBox)
        {
            if (allSelected)
            {
                unit.Deselect();
                selectedUnits.Remove(unit);
            }
            else if (!selectedUnits.Contains(unit))
            {
                unit.Select();
                selectedUnits.Add(unit);
            }
        }
    }

    private void CheckSingleClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            UnitRTS clickedUnit = hit.collider.GetComponent<UnitRTS>();
            bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            if (clickedUnit != null)
            {
                float timeSinceLastClick = Time.time - lastClickTime;
                lastClickTime = Time.time;

                // Handle double-click selection
                if (timeSinceLastClick < doubleClickThreshold)
                    HandleDoubleClick(clickedUnit, shiftHeld);
                else
                    HandleSingleClick(clickedUnit, shiftHeld);
            }
            else if (!shiftHeld)
            {
                DeselectAll();
            }
        }
        else if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
        {
            DeselectAll();
        }
    }

    private void HandleSingleClick(UnitRTS clickedUnit, bool shiftHeld)
    {
        if (shiftHeld)
        {
            // Toggle selection state
            if (selectedUnits.Contains(clickedUnit))
            {
                clickedUnit.Deselect();
                selectedUnits.Remove(clickedUnit);
            }
            else
            {
                clickedUnit.Select();
                selectedUnits.Add(clickedUnit);
            }
        }
        else
        {
            // Select single unit exclusively
            if (selectedUnits.Count != 1 || !selectedUnits.Contains(clickedUnit))
            {
                DeselectAll();
                clickedUnit.Select();
                selectedUnits.Add(clickedUnit);
            }
            else
            {
                DeselectAll();
            }
        }
    }

    private void HandleDoubleClick(UnitRTS clickedUnit, bool shiftHeld)
    {
        if (!shiftHeld)
            DeselectAll();

        // Select all units of the same type
        foreach (UnitRTS unit in FindObjectsByType<UnitRTS>(FindObjectsSortMode.None))
        {
            if (unit.UnitType == clickedUnit.UnitType && !selectedUnits.Contains(unit))
            {
                unit.Select();
                selectedUnits.Add(unit);
            }
        }
    }

    private void CheckEscapeKey()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            DeselectAll();
    }

    private void DeselectAll()
    {
        foreach (UnitRTS unit in selectedUnits)
            unit.Deselect();

        selectedUnits.Clear();
    }
}
