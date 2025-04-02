// ─────────────────────────────────────
// ▶ EVENTS
// ─────────────────────────────────────

using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Manages unit selection (click, drag-box, double-click) in RTS gameplay.
/// Attach to an input controller GameObject.
/// </summary>
public class RTSController : MonoBehaviour
{
    public static event Action<List<UnitRTS>> OnSelectionChanged;

    // ─────────────────────────────────────
    // ▶ CONFIGURATION & STATE
    // ─────────────────────────────────────

    public RectTransform selectionBoxUI;

    private Vector2 startMousePos;
    private Vector2 endMousePos;
    private bool isDragging;

    private readonly List<UnitRTS> selectedUnits = new();
    private readonly List<UnitRTS> unitsInBoxBuffer = new();

    private float lastClickTime;
    private const float doubleClickThreshold = 0.2f;
    private const float dragThreshold = 10f;

    private Camera cam;

    // ─────────────────────────────────────
    // ▶ UNITY LIFECYCLE
    // ─────────────────────────────────────

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        HandleSelectionInput();
        DrawSelectionBox();
        CheckEscapeKey();
    }

    // ─────────────────────────────────────
    // ▶ INPUT HANDLING
    // ─────────────────────────────────────

    private void HandleSelectionInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            startMousePos = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            endMousePos = Input.mousePosition;
            isDragging = false;

            if (Vector2.Distance(startMousePos, endMousePos) > dragThreshold)
                SelectUnitsByBoxArea();
            else
                HandleClickSelection();
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

    private void CheckEscapeKey()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            DeselectAll();
    }

    // ─────────────────────────────────────
    // ▶ SELECTION HANDLING
    // ─────────────────────────────────────

    private void HandleClickSelection()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            UnitRTS clickedUnit = hit.collider.GetComponent<UnitRTS>();
            bool shiftHeld = IsShiftHeld;

            if (clickedUnit != null)
            {
                float timeSinceLastClick = Time.time - lastClickTime;
                lastClickTime = Time.time;

                if (timeSinceLastClick < doubleClickThreshold)
                    ProcessDoubleClick(clickedUnit, shiftHeld);
                else
                    ProcessSingleClick(clickedUnit, shiftHeld);
            }
            else if (!shiftHeld)
            {
                DeselectAll();
            }
        }
        else if (!IsShiftHeld)
        {
            DeselectAll();
        }
    }

    private void ProcessSingleClick(UnitRTS clickedUnit, bool shiftHeld)
    {
        if (shiftHeld)
        {
            if (selectedUnits.Contains(clickedUnit))
                DeselectUnit(clickedUnit);
            else
                SelectUnit(clickedUnit);
        }
        else
        {
            if (selectedUnits.Count != 1 || !selectedUnits.Contains(clickedUnit))
            {
                DeselectAll();
                SelectUnit(clickedUnit);
            }
            else
            {
                DeselectAll();
            }
        }

        OnSelectionChanged?.Invoke(selectedUnits);
    }

    private void ProcessDoubleClick(UnitRTS clickedUnit, bool shiftHeld)
    {
        if (!shiftHeld)
            DeselectAll();

        foreach (UnitRTS unit in FindObjectsByType<UnitRTS>(FindObjectsSortMode.None))
        {
            if (unit.UnitType == clickedUnit.UnitType && !selectedUnits.Contains(unit))
                SelectUnit(unit);
        }

        OnSelectionChanged?.Invoke(selectedUnits);
    }

    private void SelectUnitsByBoxArea()
    {
        unitsInBoxBuffer.Clear();

        Vector2 min = Vector2.Min(startMousePos, endMousePos);
        Vector2 max = Vector2.Max(startMousePos, endMousePos);
        Rect selectionRect = new Rect(min, max - min);

        foreach (UnitRTS unit in FindObjectsByType<UnitRTS>(FindObjectsSortMode.None))
        {
            Vector3 screenPos = cam.WorldToScreenPoint(unit.transform.position);
            if (selectionRect.Contains(screenPos, true))
                unitsInBoxBuffer.Add(unit);
        }

        foreach (UnitRTS unit in selectedUnits.ToArray())
        {
            if (!unitsInBoxBuffer.Contains(unit))
                DeselectUnit(unit);
        }

        foreach (UnitRTS unit in unitsInBoxBuffer)
        {
            if (!selectedUnits.Contains(unit))
                SelectUnit(unit);
        }

        OnSelectionChanged?.Invoke(selectedUnits);
    }

    // ─────────────────────────────────────
    // ▶ SELECTION HELPERS
    // ─────────────────────────────────────

    private void SelectUnit(UnitRTS unit)
    {
        unit.Select();
        selectedUnits.Add(unit);
    }

    private void DeselectUnit(UnitRTS unit)
    {
        unit.Deselect();
        selectedUnits.Remove(unit);
    }

    private void DeselectAll()
    {
        foreach (UnitRTS unit in selectedUnits)
            unit.Deselect();

        selectedUnits.Clear();
        OnSelectionChanged?.Invoke(selectedUnits);
    }

    private bool IsShiftHeld => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
}