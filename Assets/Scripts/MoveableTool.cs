using UnityEngine;

public class MoveTool : MonoBehaviour
{
    public GameObject axisGizmoPrefab; // Assign in Unity Inspector
    private GameObject axisGizmoInstance;
    
    private Transform selectedObject;
    private Camera mainCamera;
    private bool isDragging = false;
    private bool axisSelected = false; // Ensures gizmo stays visible after object selection

    private enum Axis { None, X, Y, Z }
    private Axis currentAxis = Axis.None;

    private Plane movementPlane;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        HandleSelection();

        if (isDragging && selectedObject != null)
        {
            Vector3 newMousePosition = GetMouseWorldPosition();
            selectedObject.position = ConstrainToAxis(newMousePosition);
        }

        if (Input.GetMouseButtonUp(0)) // Release object or axis
        {
            isDragging = false;
        }
    }

    private void HandleSelection()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.CompareTag("Celestial Body"))
                {
                    SelectObject(hit.transform);
                }
                else if (hit.transform.CompareTag("Axis")) 
                {
                    SelectAxis(hit.transform);
                }
                else 
                {
                    DeselectObject();
                }
            }
            else 
            {
                DeselectObject();
            }
        }
    }

    private void SelectObject(Transform obj)
    {
        selectedObject = obj;
        SpawnGizmo();
    }

    private void SelectAxis(Transform axisTransform)
    {
        axisSelected = true; // Prevent gizmo from disappearing
        isDragging = true; // Start moving object

        // Determine which axis was selected
        if (axisTransform.name.Contains("X")) currentAxis = Axis.X;
        else if (axisTransform.name.Contains("Y")) currentAxis = Axis.Y;
        else if (axisTransform.name.Contains("Z")) currentAxis = Axis.Z;

        SetMovementPlane();
    }

    private void SetMovementPlane()
    {
        if (selectedObject == null) return;

        // Plane perpendicular to the camera's view, passing through the selected object
        movementPlane = new Plane(mainCamera.transform.forward, selectedObject.position);
    }

    private void SpawnGizmo()
{
    // Destroy old gizmo if it exists
    if (axisGizmoInstance != null) 
        Destroy(axisGizmoInstance);

    if (selectedObject == null) 
        return;

    // Instantiate the gizmo at the selected object's exact world position
    axisGizmoInstance = Instantiate(axisGizmoPrefab, selectedObject.position, Quaternion.identity);

    // Ensure it's not parented initially to avoid local position issues
    axisGizmoInstance.transform.position = selectedObject.position;

    // Assign "Axis" tag to each arrow in the gizmo
    foreach (Transform child in axisGizmoInstance.transform)
    {
        child.gameObject.tag = "Axis";
    }

    // (Optional) Parent the gizmo to the object so it moves with it
    axisGizmoInstance.transform.SetParent(selectedObject, true);
}

    private void DeselectObject()
    {
        selectedObject = null;
        DestroyGizmo();
    }

    private void DestroyGizmo()
    {
        if (axisGizmoInstance != null)
        {
            Destroy(axisGizmoInstance);
            axisGizmoInstance = null;
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (movementPlane.Raycast(ray, out float enter))
        {
            return ray.GetPoint(enter);
        }

        return selectedObject != null ? selectedObject.position : Vector3.zero;
    }

    private Vector3 ConstrainToAxis(Vector3 position)
    {
        if (selectedObject == null) return position;

        Vector3 objectPosition = selectedObject.position;

        switch (currentAxis)
        {
            case Axis.X:
                return new Vector3(position.x, objectPosition.y, objectPosition.z);
            case Axis.Y:
                return new Vector3(objectPosition.x, position.y, objectPosition.z);
            case Axis.Z:
                return new Vector3(objectPosition.x, objectPosition.y, position.z);
            default:
                return objectPosition;
        }
    }
}
